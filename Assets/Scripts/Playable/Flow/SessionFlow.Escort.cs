using System.Collections.Generic;
using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (int Floor,string Room,int OffsetX,int SafeX,int SafeY,string Name,string RefugeDirection,string Request)[] EscortPeople =
        {
            (1,"Posgrado",-2,26,5,"Inés","Pasillo inferior, al este, pasando las bancas.",
                "Escuché el ascensor abrirse tres veces y nunca vi a nadie salir. ¿Me acompañas a la señal verde? Está en el pasillo inferior, pasando las bancas del lado este."),
            (4,"Cámara Gesell",-2,4,5,"Marcos","Extremo oeste del pasillo inferior.",
                "El espejo todavía refleja el aula vacía, pero escucho a alguien detrás. Si vas conmigo, llegamos al refugio del extremo oeste del pasillo inferior."),
            (5,"Aula de grupos",2,26,6,"Elena","Pasillo inferior, al este, pasando las mesas.",
                "Volví a buscar a una amiga y se cerraron las gradas. Vi una señal verde al este, pasando las mesas del pasillo de abajo. Espera si me quedo atrás.")
        };
        const int EscortMask=(1<<3)-1;
        int escortSeen, escortedPeople, escorting;
        bool escortInHall;
        Vector2 escortLocalFeet;
        readonly SpriteRenderer[] escortActors=new SpriteRenderer[EscortPeople.Length];
        readonly SpriteRenderer[] escortHallActors=new SpriteRenderer[EscortPeople.Length];
        readonly bool[] escortRefugesShown=new bool[EscortPeople.Length];
        readonly bool[] escortArrivalsShown=new bool[EscortPeople.Length];
        List<Vector2> escortRoute;
        Vector2 escortRouteGoal;
        int escortWaypoint;
        float nextEscortRoute, escortStride;
        Vector2 EscortContact(int index) => World.Entry(Room)+new Vector2(EscortPeople[index].OffsetX,2);
        int EscortHall(int index) => ForFloor(EscortPeople[index].Floor,true);
        Vector2 EscortRefuge(int index) => World.Cell(EscortHall(index),EscortPeople[index].SafeX,EscortPeople[index].SafeY);
        Vector2 EscortArrival(int index) => World.Cell(DarkGround,60,7+2*index);
        bool IsEscortSource(int index,int room) => IsDarkUpperInterior(room)&&UpperInteriorName(room)==EscortPeople[index].Room;

        void EnterEscortRoom(int previous)
        {
            escortRoute=null;
            nextEscortRoute=0;
            if(!IsDark(Room))return;
            for(int i=0;i<EscortPeople.Length;i++)
            {
                bool safe=HasEscapeFlag(escortedPeople,i);
                if(Room==DarkGround&&safe&&!escortArrivalsShown[i])
                {
                    World.MarkEscapeContact(Room,EscortArrival(i),EscortPeople[i].Name,"pb-estudiante");
                    escortArrivalsShown[i]=true;
                }
                if(IsEscortSource(i,Room)&&!safe&&
                    !escortActors[i]&&!(escorting==i+1&&escortInHall))
                    escortActors[i]=MakeEscortActor(i,EscortContact(i));
                if(Room!=EscortHall(i))continue;
                if(safe&&escortHallActors[i])escortHallActors[i].gameObject.SetActive(false);

                if(!escortRefugesShown[i])
                {
                    var refuge=World.MarkEscapeContact(Room,EscortRefuge(i),"Refugio de "+EscortPeople[i].Name,"tour-marca-piso");
                    refuge.GetComponent<SpriteRenderer>().color=new Color(.57f,.97f,.78f);
                    escortRefugesShown[i]=true;
                }
                if(escorting!=i+1)continue;
                if(IsEscortSource(i,previous)&&!escortInHall)
                {
                    escortInHall=true;
                    var arrival=Player.Position+Vector2.right*.55f;
                    escortLocalFeet=(World.CanStand(arrival)?arrival:Player.Position)-World.Offset(Room);
                    escortActors[i].gameObject.SetActive(false);
                }
                if(escortInHall&&!escortHallActors[i])
                    escortHallActors[i]=MakeEscortActor(i,EscortFeet());
            }
            if(escorting==0)return;
            var actor=CurrentEscortActor();
            if(!actor)return;
            if(!World.CanStand(EscortFeet()))
                escortLocalFeet=(escortInHall?Player.Position:EscortContact(escorting-1))-World.Offset(Room);
            PoseEscort(actor,EscortFeet(),Vector2.down,0);
        }

        SpriteRenderer MakeEscortActor(int index,Vector2 at)
        {
            var actor=World.MarkEscapeContact(Room,at,EscortPeople[index].Name,"pb-estudiante").GetComponent<SpriteRenderer>();
            actor.color=index==0?new Color(.84f,.81f,1):index==1?new Color(.78f,.93f,.77f):new Color(1,.84f,.73f);
            return actor;
        }

        SpriteRenderer CurrentEscortActor()
        {
            if(escorting==0)return null;
            int index=escorting-1;
            if(escortInHall)return Room==EscortHall(index)?escortHallActors[index]:null;
            return IsEscortSource(index,Room)?escortActors[index]:null;
        }

        Vector2 EscortFeet() => escortLocalFeet+World.Offset(Room);

        void UpdateEscort(bool playing)
        {
            if(!playing)return;
            var actor=CurrentEscortActor();
            if(!actor)return;
            Vector2 feet=EscortFeet();
            Vector2 player=Player.Position;
            bool visible=World.HasLineOfSight(feet,player,false);
            if(visible&&(player-feet).sqrMagnitude<1.15f*1.15f)return;
            Vector2 target=player;
            bool followingRoute=escortRoute!=null&&escortWaypoint<escortRoute.Count;
            if(!visible||followingRoute)
            {
                bool needsRoute=!followingRoute||(player-escortRouteGoal).sqrMagnitude>1f;
                if(needsRoute&&Time.time>=nextEscortRoute)
                {
                    escortRoute=World.FindRoute(feet,player);
                    escortRouteGoal=player;
                    escortWaypoint=0;
                    nextEscortRoute=Time.time+.24f;
                }
                if(escortRoute==null)return;
                while(escortWaypoint<escortRoute.Count&&
                    (escortRoute[escortWaypoint]-feet).sqrMagnitude<.012f*.012f)escortWaypoint++;
                if(escortWaypoint>=escortRoute.Count)return;
                target=escortRoute[escortWaypoint];
            }
            else escortRoute=null;
            float distance=Vector2.Distance(feet,target);
            if(distance<.008f)return;
            float stride=Mathf.Min(Time.deltaTime,.05f)*3.65f;
            Vector2 destination=Vector2.MoveTowards(feet,target,stride);
            if(!World.HasLineOfSight(feet,destination,false))
            {
                escortRoute=null;
                return;
            }
            escortLocalFeet=destination-World.Offset(Room);
            escortStride+=Vector2.Distance(feet,destination);
            PoseEscort(actor,destination,destination-feet,(int)(escortStride*4f)%4);
        }

        static void PoseEscort(SpriteRenderer actor,Vector2 feet,Vector2 facing,int frame)
        {
            actor.transform.position=feet+Vector2.up*.62f;
            actor.sortingOrder=CampusLook.Depth(feet.y)-4;
            actor.sprite=CampusLook.PersonPose(actor.name,"pb-estudiante",facing,frame);
        }

        bool TryEscortInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(IsDarkUpperInterior(Room))
                for(int i=0;i<EscortPeople.Length;i++)
                {
                    if(HasEscapeFlag(escortedPeople,i)||!IsEscortSource(i,Room))continue;
                    int index=i;
                    var person=EscortPeople[i];
                    if(escorting==index+1)
                    {
                        if(!escortInHall&&Near(World.UpperInteriorExit(Room),1.05f)&&!EscortCloseEnough())
                        {
                            prompt="Espera a "+person.Name;
                            if(interact)Notify(person.Name+" viene detrás. Espera antes de cruzar la puerta.");
                            return true;
                        }
                        continue;
                    }
                    if(!Near(EscortContact(i),.86f))continue;
                    prompt="Hablar · "+person.Name;
                    if(interact)
                    {
                        if(escorting>0)
                        {
                            escortSeen|=1<<index;
                            Notify(person.Name+" espera aquí. Primero acompaña a "+EscortPeople[escorting-1].Name+".",4);
                        }
                        else OpenDialogue(new[]{new DialogueLine(person.Name,"pb-estudiante",person.Request)},()=>StartEscort(index));
                    }
                    return true;
                }

            if(!IsInterior(Room)&&Room!=ServiceGallery)
                for(int i=0;i<EscortPeople.Length;i++)
                {
                    if(Room==DarkGround&&HasEscapeFlag(escortedPeople,i)&&Near(EscortArrival(i),.82f))
                    {
                        prompt="Hablar · "+EscortPeople[i].Name;
                        if(interact)OpenDialogue(new[]{new DialogueLine(EscortPeople[i].Name,"pb-estudiante",
                            "El equipo me trajo hasta aquí desde el refugio. Ahora esperamos junto al portón.")});
                        return true;
                    }
                    if(escorting!=i+1||!escortInHall||Room!=EscortHall(i)||!Near(EscortRefuge(i),.94f))continue;
                    prompt=EscortCloseEnough()?"Dar aviso por radio · "+EscortPeople[i].Name:"Espera a "+EscortPeople[i].Name;
                    if(interact)
                    {
                        if(EscortCloseEnough())CompleteEscort(i);
                        else Notify("La radio está lista. Espera a "+EscortPeople[i].Name+" en la señal verde.");
                    }
                    return true;
                }
            return false;
        }

        bool EscortCloseEnough() => (EscortFeet()-Player.Position).sqrMagnitude<=1.75f*1.75f&&
            World.HasLineOfSight(EscortFeet(),Player.Position,false);

        void StartEscort(int index)
        {
            escortSeen|=1<<index;
            escorting=index+1;
            escortInHall=false;
            escortLocalFeet=EscortContact(index)-World.Offset(Room);
            escortStride=0;
            nextEscortRoute=0;
            escortRoute=null;
            Notify("Acompaña a "+EscortPeople[index].Name+" hasta la señal verde",4);
        }

        void CompleteEscort(int index)
        {
            escortedPeople|=1<<index;
            escorting=0;
            escortInHall=false;
            escortRoute=null;
            Sound.Gathered();
            OpenDialogue(new[]{new DialogueLine(EscortPeople[index].Name,"pb-estudiante",
                "Ya respondieron por radio. Voy a esperar a la brigada en esta luz. Nos vemos en planta baja.")});
        }

        string EscortRescueNotes()
        {
            var lines=new System.Text.StringBuilder("ACOMPAÑAMIENTOS");
            for(int i=0;i<EscortPeople.Length;i++)
            {
                if(!HasEscapeFlag(escortSeen,i))continue;
                var person=EscortPeople[i];
                lines.Append("\n\n").Append(person.Name).Append(" · Piso ").Append(person.Floor).AppendLine();
                if(HasEscapeFlag(escortedPeople,i))lines.Append("Con el equipo en planta baja.");
                else if(escorting==i+1&&escortInHall)lines.Append(person.RefugeDirection).Append(" Busca la señal verde.");
                else lines.Append("Te espera en ").Append(person.Room).Append('.');
            }
            if(escortSeen==0)lines.Append("\n\nHabla con quienes siguen en las salas.");
            return lines.ToString();
        }
    }
}
