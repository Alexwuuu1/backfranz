using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly int DarkAuditorium=InteriorRoom(AuditoriumIndex,true);
        static readonly Vector2Int[] AuditoriumPatrol=
        {
            new(19,11),new(11,11),new(11,5),new(11,11),new(19,11),new(19,7)
        };
        bool auditoriumWedged;
        SpriteRenderer auditoriumShadow, auditoriumRecorder;
        Vector2 auditoriumShadowFeet;
        int auditoriumWaypoint;
        float auditoriumWait, auditoriumPlayback, auditoriumGrace;
        float nextAuditoriumSound;

        Vector2 AuditoriumCell(Vector2Int cell)=>World.Cell(DarkAuditorium,cell.x,cell.y);
        Vector2 RecorderContact()=>World.Cell(DarkAuditorium,19,6);
        bool MauroFollowing=>escorting==MauroEscort+1;
        bool MauroSafe=>HasEscapeFlag(escortedPeople,MauroEscort);

        void EnterAuditoriumRescue()
        {
            if(Room!=DarkAuditorium&&Room!=DarkGround)return;
            if(auditoriumWedged)World.DoorAt(Room,World.AuditoriumServiceDoor(Room)).SetOpening(1);
            if(Room!=DarkAuditorium)return;
            if(!auditoriumShadow)
            {
                auditoriumShadow=World.MarkEscapeContact(Room,AuditoriumCell(AuditoriumPatrol[0]),"Presencia del Auditorio","shadow-00")
                    .GetComponent<SpriteRenderer>();
                auditoriumRecorder=World.PlaceCollectible(Room,World.Cell(Room,20,6),.49f,"Grabadora en reproducción",RecorderSprite)
                    .GetComponent<SpriteRenderer>();
            }
            ResetAuditoriumPatrol();
            auditoriumShadow.gameObject.SetActive(!MauroSafe);
            if(!MauroSafe)
            {
                Sound.AuditoriumSpeaker();
                Notify("Yamil · Se giró cuando crujió el altavoz.",3);
            }
        }

        void ResetAuditoriumPatrol()
        {
            auditoriumWaypoint=1;
            auditoriumShadowFeet=AuditoriumCell(AuditoriumPatrol[0]);
            auditoriumWait=5f;
            auditoriumGrace=2f;
            auditoriumPlayback=0;
            auditoriumRecorder.gameObject.SetActive(false);
            DrawAuditoriumShadow();
        }

        void DrawAuditoriumShadow()
        {
            auditoriumShadow.transform.position=auditoriumShadowFeet+Vector2.up*.65f;
            auditoriumShadow.sortingOrder=CampusLook.Depth(auditoriumShadowFeet.y)-4;
            auditoriumShadow.sprite=CampusLook.Sprite("shadow-"+((int)(Time.time*6)%4).ToString("00"));
            auditoriumShadow.transform.localScale=new Vector3(1.1f,1.1f,1);
            auditoriumShadow.color=auditoriumPlayback>0?new Color(.78f,.83f,.94f):new Color(.58f,.62f,.76f);
        }

        void UpdateAuditoriumRescue(bool playing)
        {
            if(!playing||Room!=DarkAuditorium||MauroSafe)return;
            float delta=Time.deltaTime;
            auditoriumGrace-=delta;
            if(auditoriumPlayback>0)
            {
                auditoriumPlayback-=delta;
                if(auditoriumPlayback<=0)
                {
                    auditoriumRecorder.gameObject.SetActive(false);
                    auditoriumWaypoint=0;
                    auditoriumWait=0;
                }
                else if(Time.time>=nextAuditoriumSound)
                {
                    Sound.RecordedVoice();
                    nextAuditoriumSound=Time.time+2.6f;
                }
            }
            if(auditoriumPlayback>0)
            {
                var east=AuditoriumCell(AuditoriumPatrol[0]);
                var listening=AuditoriumCell(AuditoriumPatrol[AuditoriumPatrol.Length-1]);
                // Cross in front of the stage before moving between the wall and the east seats.
                var target=Mathf.Abs(auditoriumShadowFeet.x-east.x)<.025f?listening:
                    Mathf.Abs(auditoriumShadowFeet.y-east.y)<.025f?east:
                    new Vector2(auditoriumShadowFeet.x,east.y);
                auditoriumShadowFeet=Vector2.MoveTowards(auditoriumShadowFeet,target,3.5f*delta);
            }
            else if((auditoriumWait-=delta)<=0)
            {
                var target=AuditoriumCell(AuditoriumPatrol[auditoriumWaypoint]);
                auditoriumShadowFeet=Vector2.MoveTowards(auditoriumShadowFeet,target,1.65f*delta);
                if(auditoriumShadowFeet==target)
                {
                    auditoriumWaypoint=(auditoriumWaypoint+1)%AuditoriumPatrol.Length;
                    auditoriumWait=auditoriumWaypoint==1?4f:.85f;
                }
            }
            DrawAuditoriumShadow();
            if(auditoriumGrace>0)return;
            if(ShadowReached(Player.Position)||(MauroFollowing&&!escortInHall&&ShadowReached(EscortFeet())))
                RetryAuditoriumRescue();
        }

        bool ShadowReached(Vector2 point)=>(auditoriumShadowFeet-point).sqrMagnitude<.76f*.76f&&
            World.HasLineOfSight(auditoriumShadowFeet,point,false);

        void RetryAuditoriumRescue()
        {
            if(MauroFollowing&&!escortInHall)
            {
                escorting=0;
                escortRoute=null;
                PoseEscort(escortActors[MauroEscort],EscortContact(MauroEscort),Vector2.down,0);
            }
            Player.transform.position=World.Entry(Room);
            Player.TouchAxis(Vector2.zero);
            ResetAuditoriumPatrol();
            Sound.ShadowCaught();
            Notify("La presencia cerró el paso. Puedo volver por Mauro.",4);
        }

        bool TryAuditoriumRescue(bool interact,out string prompt)
        {
            prompt="";
            if(Room==DarkAuditorium&&!MauroSafe&&Near(RecorderContact(),.86f))
            {
                if(!HasAuditoriumTool(AuditoriumTool.Recorder))
                    prompt="Revisar · Mesa de sonido";
                else prompt=auditoriumPlayback>0?"La grabadora está sonando":"Reproducir · Grabadora de prácticas";
                if(interact&&auditoriumPlayback<=0)
                {
                    if(!HasAuditoriumTool(AuditoriumTool.Recorder))
                        Notify("Podría dejar aquí la grabadora de Sonido, en el piso 3.",4);
                    else
                    {
                        auditoriumPlayback=24f;
                        nextAuditoriumSound=Time.time+2.6f;
                        auditoriumRecorder.gameObject.SetActive(true);
                        Sound.RecordedVoice();
                        Notify("La presencia se acerca a la grabadora. Tengo que rodear por el otro lado.",4);
                    }
                }
                return true;
            }
            if((Room!=DarkAuditorium&&Room!=DarkGround)||!AtAuditoriumServiceDoor())return false;
            if(!auditoriumWedged&&HasAuditoriumTool(AuditoriumTool.Wedge))
            {
                prompt="Calzar puerta · Cuña de goma";
                if(interact)
                {
                    auditoriumWedged=true;
                    World.DoorAt(Room,World.AuditoriumServiceDoor(Room)).SetOpening(1);
                    Sound.Latch();
                    Notify("La cuña mantiene abierta la puerta de servicio.",4);
                }
                return true;
            }
            if(!auditoriumWedged&&Room==DarkGround&&!MauroSafe)
            {
                prompt="Revisar · Puerta de servicio";
                if(interact)Notify("La puerta se cierra sola. Con una cuña podría dejarla abierta.",4);
                return true;
            }
            if(Room!=DarkAuditorium||!MauroFollowing||escortInHall)return false;
            if(!auditoriumWedged)
            {
                prompt="La puerta se cierra · Llevar a Mauro por el vestíbulo";
                if(interact)Notify("Mauro no alcanzará a pasar. Podemos salir por las butacas.",4);
                return true;
            }
            if(EscortCloseEnough())return false;
            prompt="Espera a Mauro";
            if(interact)Notify("Mauro viene detrás. Espera junto a la puerta.",3);
            return true;
        }

        string AuditoriumRescueThought()
        {
            if(MauroFollowing&&(Room==DarkAuditorium||Room==DarkGround))
            {
                if(escortInHall)return "Tengo que esperar a Mauro en la luz verde del vestíbulo oeste.";
                return auditoriumWedged?"Puedo llevar a Mauro al refugio por la puerta que calzamos.":
                    "Tengo que llevar a Mauro por las butacas hasta la luz verde del vestíbulo oeste.";
            }
            if(Room!=DarkAuditorium||MauroSafe)return null;
            return auditoriumPlayback>0?"Ahora mira hacia la grabadora. Debería ir por el otro lado y buscar a Mauro.":
                "Hay alguien junto al escenario. Puedo acercarme cuando la presencia mire al otro lado.";
        }
    }
}
