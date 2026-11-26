using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        const string ChalkName="Tiza blanca", ChalkSprite="utileria-tiza-blanca";
        bool hasClassroomChalk, classroomLoopSeen, classroomChalkMarked;
        bool classroomMarkVisible;
        readonly GameObject[] classroomChalkViews=new GameObject[2];
        bool InClassroomLoopHall=>Room==ForFloor(4,true);
        bool InClassroom4A=>IsUpperInterior(Room)&&UpperInteriorName(Room)=="Aula 4A";
        Vector2 ClassroomLocal(Vector2 point)=>point-World.Cell(Room,0,0);

        void EnterClassrooms()
        {
            if(InClassroomLoopHall)
            {
                if(classroomLoopSeen)World.ShowClassroomTurnoff(Room);
                if(classroomChalkMarked)ShowClassroomMark();
                return;
            }
            if(!InClassroom4A||hasClassroomChalk)return;
            int phase=IsDark(Room)?1:0;
            if(!classroomChalkViews[phase])
                classroomChalkViews[phase]=World.PlaceCollectible(Room,World.Cell(Room,3,10),.58f,ChalkName,ChalkSprite);
        }

        void ShowClassroomMark()
        {
            if(classroomMarkVisible)return;
            World.ShowClassroomChalkMark(Room);
            classroomMarkVisible=true;
        }

        bool TryClassroomChalk(bool interact,out string prompt)
        {
            prompt="";
            if(!hasClassroomChalk&&InClassroom4A&&Near(World.Cell(Room,3,9),.88f))
            {
                prompt="Recoger · "+ChalkName;
                if(interact)
                {
                    hasClassroomChalk=true;
                    foreach(var view in classroomChalkViews)
                        if(view)view.SetActive(false);
                    Sound.PickUp();
                    Notify("Recogiste la tiza blanca.",3);
                }
                return true;
            }
            if(!InClassroomLoopHall)return false;
            var turnoff=World.ClassroomTurnoff(Room);
            if(classroomLoopSeen&&Near(turnoff,.83f))
            {
                prompt="Cruzar · Desvío hacia observación";
                if(interact)UseDoor(turnoff,Room,World.Cell(Room,22,8));
                return true;
            }
            if(!hasClassroomChalk||!Near(World.Cell(Room,11,4),.82f))return false;
            prompt=classroomChalkMarked?"Revisar marca · 4B":"Marcar con tiza · 4B";
            if(interact)
            {
                if(classroomChalkMarked)Notify("Mi marca sigue en el mismo sitio.",3);
                else
                {
                    classroomChalkMarked=true;
                    ShowClassroomMark();
                    Sound.Click();
                    Notify("Dejé una cruz bajo el 4B. Así sabré si vuelvo al mismo lugar.",4);
                }
            }
            return true;
        }

        internal bool ObserveClassroomLoop(Vector2 from,Vector2 to)
        {
            if(!InClassroomLoopHall||Horror!=HorrorState.Active)return false;
            var point=ClassroomLocal(to);
            if(point.x<22.45f||point.y<2.6f||point.y>5.4f||ClassroomLocal(from).x>=22.45f)return false;
            Player.transform.position=World.Cell(Room,11,4)+Vector2.right*.45f;
            if(!classroomLoopSeen)
            {
                classroomLoopSeen=true;
                World.ShowClassroomTurnoff(Room);
            }
            Sound.DistantKnock();
            Notify(classroomChalkMarked?"Volví a mi cruz bajo el 4B. Vi una luz verde antes de la segunda placa.":
                "Otra vez el 4B. La luz verde queda antes de la segunda placa.",4.5f);
            return true;
        }

        string ClassroomLoopThought()
        {
            if(!InClassroomLoopHall)return null;
            var point=ClassroomLocal(Player.Position);
            if(point.y>=5.6f||point.x<9||point.x>24)return null;
            if(classroomLoopSeen)return "El pasillo se repite. Puedo tomar el desvío junto a la luz verde antes del segundo 4B.";
            if(classroomChalkMarked)return "Dejé una cruz bajo el 4B. Si vuelvo a verla, sabré que el pasillo dio la vuelta.";
            return "El 4B aparece dos veces. Debería fijarme en las placas y en la luz verde.";
        }
    }
}
