using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        const int CampusOutfitCount=18;
        public sealed class CampusPerson
        {
            public readonly string Name, Sprite, Greeting;
            internal readonly Transform Actor;
            internal readonly Vector2[] Route;
            internal int Waypoint;
            internal float Wait;
            internal float Travelled;
            internal string WalkingArt;
            public Vector2 Position => Actor.position;

            internal CampusPerson(Transform actor,string name,string sprite,string greeting,Vector2[] route)
            {
                Actor=actor; Name=name; Sprite=sprite; Greeting=greeting; Route=route; Waypoint=route.Length>1?1:0;
            }
        }

        readonly Dictionary<int,List<CampusPerson>> campusPeople=new Dictionary<int,List<CampusPerson>>();
        CampusPerson classTraveler;
        int classHall, classRoom;
        Vector2[] classHallRoute;
        bool attendingClass, leavingClass;
        float nextClassChange;

        CampusPerson PlacePerson(int room,string name,string sprite,string greeting,bool seated,params Vector2Int[] route)
        {
            var waypoints=new Vector2[route.Length];
            for(int i=0;i<route.Length;i++)waypoints[i]=Cell(room,route[i].x,route[i].y);
            string appearance=seated&&name=="Aylin"?"npc-aylin-sentado":CampusLook.Person(name,sprite);
            var actor=Art(room,appearance,waypoints[0],new Vector2(.83f,seated?.88f:1.13f),name);
            var person=new CampusPerson(actor.transform,name,sprite,greeting,waypoints);
            PeopleIn(room).Add(person);
            return person;
        }

        List<CampusPerson> PeopleIn(int room)
        {
            if(!campusPeople.TryGetValue(room,out var people)){
                people=new List<CampusPerson>();
                campusPeople.Add(room,people);
            }
            return people;
        }

        void PopulateHall()
        {
            PlacePerson(Ground,"Aylin","pb-estudiante","El Game Room queda al fondo. Si buscas un sitio tranquilo, la biblioteca está en el sexto.",true,
                new Vector2Int(22,14));
            PlacePerson(Ground,"Damián","pb-estudiante","¿También estás buscando tu aula? En Recepción me dieron el horario.",false,
                new Vector2Int(19,17),new Vector2Int(26,17));
            PlaceCrowdMember(Ground,0,9,14,14,14);
            PlaceCrowdMember(Ground,1,18,15,25,15);
            PlaceCrowdMember(Ground,2,29,14,33,14);
            PlaceCrowdMember(Ground,3,44,15,50,15);
            PlaceCrowdMember(Ground,4,59,6,59,11);
            PlaceCrowdMember(Ground,5,16,15,seated:true);
            PlaceCrowdMember(Ground,6,28,16,seated:true);
            PlaceCrowdMember(Ground,7,9,16,13,16);
            PlaceCrowdMember(Ground,8,31,17,34,17);
            PlaceCrowdMember(Ground,9,48,17,51,17);
            PlaceCrowdMember(Ground,10,49,20,seated:true);
            PlaceCrowdMember(Ground,11,52,19);
            PlaceCrowdMember(Ground,12,6,15,10,15);
        }

        void PopulateUpper(int room,int floor)
        {
            if(IsDark(room))return;
            if(floor==1)
                PlacePerson(room,"Personal administrativo","pb-recepcionista","Caja y Posgrado están por el pasillo de arriba. Para llegar a Dirección, baja por el cruce de este lado.",false,
                    new Vector2Int(25,15));
            string teacher=floor switch
            {
                2=>"Profesora",3=>"Docente de Computación",4=>"Docente de Derecho",5=>"Docente del Taller",_=>null
            };
            if(teacher!=null){
                var (line,position)=floor switch{
                    2=>("Simulación e Histología quedan en el corredor de luz fría. Para las prácticas de cocina, cruza hacia el lado de las bandejas.",new Vector2Int(12,14)),
                    3=>("Los laboratorios dan al corredor de arriba. Cruza por el medio para bajar a Streaming, Fotografía o Sonido.",new Vector2Int(16,11)),
                    4=>("Derecho queda al final del corredor de ventanas. Gesell está junto al ascensor y 4A se alcanza por el cruce del centro.",new Vector2Int(16,9)),
                    _=>("El Aula de grupos queda en el extremo oeste. Para Taller, baja por el corredor metálico del este.",new Vector2Int(22,14))
                };
                PlacePerson(room,teacher,"pb-recepcionista",line,false,position);
            }
            if(floor==4){
                classHall=room;
                foreach(int interior in UpperInteriorsOn(room))
                    if(UpperInteriorName(interior)=="Sala Jurídica"){classRoom=interior;break;}
                classTraveler=PlacePerson(room,"Estudiante de Derecho","pb-estudiante","En un rato entro a la sala jurídica. Hoy me toca práctica.",false,
                    new Vector2Int(8,17),new Vector2Int(7,18));
                classHallRoute=classTraveler.Route;
                nextClassChange=Time.time+5f;
            }
            if(floor==6)
                PlacePerson(room,"Bibliotecaria","pb-recepcionista","Para entregar tu ficha, baja por el centro y entra en la Sala de lectura. El archivo queda a la derecha.",false,
                    new Vector2Int(16,18));
            var routes=floor switch{
                1=>new[]{(4,10,4,14),(27,12,27,15),(19,16,23,16),(11,12,11,12),(18,8,22,8),(6,13,9,13)},
                2=>new[]{(9,9,13,9),(18,14,22,14),(26,8,26,11),(5,7,9,7),(18,8,22,8),(16,13,16,16)},
                3=>new[]{(5,7,9,7),(18,9,22,9),(28,8,28,11),(12,8,16,8),(9,15,13,15),(18,14,22,14)},
                4=>new[]{(6,8,11,8),(24,9,26,9),(14,13,17,13),(25,16,28,16),(23,14,26,14),(25,7,28,7)},
                5=>new[]{(8,8,13,8),(23,9,27,9),(12,13,15,13),(3,13,7,13),(24,12,27,12),(12,5,16,5)},
                _=>new[]{(10,13,13,13),(26,14,29,14),(22,5,22,8),(11,7,13,7),(6,8,6,11),(23,16,23,16)}
            };
            var (studentName,greeting)=floor switch{
                1=>("Jimena","Estoy esperando que llamen mi turno. Si vas a Caja, saca tu ficha en el dispensador."),
                2=>("Nadia","Estamos preparando las bandejas para la práctica. Las muestras de Histología van por separado."),
                3=>("Bruno","Estoy buscando una cámara para el proyecto. El estudio de Fotografía queda junto a Streaming."),
                4=>("Abril","Hoy nos toca observar una práctica en Cámara Gesell. El grupo se reúne afuera primero."),
                5=>("Sergio","En el taller estamos montando un circuito. En el aula de grupos revisamos el esquema antes de conectarlo."),
                _=>("Rebeca","Dejé los libros de consulta en la mesa. Si quieres llevarte uno, pregunta en el mostrador.")
            };
            for(int i=0;i<routes.Length;i++){
                var (fromX,fromY,toX,toY)=routes[i];
                PlaceCrowdMember(room,i,fromX,fromY,toX,toY,seated:floor==1&&i==3,
                    name:i==3?studentName:"Estudiante",greeting:i==3?greeting:null);
            }
        }

        void PopulateCafe(int room)
        {
            PlacePerson(room,"Personal de cafetería","pb-recepcionista","Buen día. Si recién llegaste, puedes sentarte donde haya lugar.",false,
                new Vector2Int(14,11));
        }

        void PopulateGroundInterior(int room,int index)
        {
            switch(index){
                case 1:
                    PlaceCrowdMember(room,0,6,12);PlaceCrowdMember(room,1,9,12);
                    PlaceCrowdMember(room,3,5,9,8,9);PlaceCrowdMember(room,4,15,8,18,8);
                    PlaceCrowdMember(room,5,16,6,seated:true);
                    break;
                case 2:
                    PlaceCrowdMember(room,0,16,11);PlaceCrowdMember(room,1,6,6,seated:true,facesAway:true);
                    PlaceCrowdMember(room,2,8,6,seated:true,facesAway:true);PlaceCrowdMember(room,3,16,8,seated:true,facesAway:true);
                    PlaceCrowdMember(room,4,4,10,10,10);PlaceCrowdMember(room,5,12,6,15,6);
                    break;
                case 3:
                    PlacePerson(room,"Mauro","pb-estudiante","Estamos probando el sonido antes de la charla. La puerta junto al escenario da al paso de servicio.",false,
                        new Vector2Int(12,12),new Vector2Int(14,12));
                    PlaceCrowdMember(room,1,3,5,3,9);
                    for(int i=0;i<8;i++)
                        PlaceCrowdMember(room,i+2,i%4<2?5+2*(i%4):13+2*(i%4-2),6+2*(i/4),seated:true,facesAway:true);
                    break;
                case 4:
                    PlaceCrowdMember(room,0,7,11);PlaceCrowdMember(room,1,10,11);PlaceCrowdMember(room,2,5,6,seated:true);
                    PlaceCrowdMember(room,3,9,9,12,9);PlaceCrowdMember(room,4,15,7,18,7);PlaceCrowdMember(room,5,17,10);
                    break;
                case 5:
                    PopulateCafe(room);
                    var leftSeat=new Vector2(-.7f,-.32f);
                    var rightSeat=new Vector2(.7f,-.32f);
                    PlaceCrowdMember(room,0,6,6,seated:true,facesAway:true,seatOffset:rightSeat);
                    PlaceCrowdMember(room,1,8,9,seated:true,facesAway:true,seatOffset:leftSeat);
                    PlaceCrowdMember(room,2,16,7,seated:true,facesAway:true,seatOffset:leftSeat);
                    PlaceCrowdMember(room,3,17,10,seated:true,facesAway:true,seatOffset:rightSeat);
                    PlaceCrowdMember(room,4,5,8,5,10);PlaceCrowdMember(room,5,12,7,12,10);
                    break;
                case 6:
                    PlaceCrowdMember(room,0,16,9);PlaceCrowdMember(room,1,4,7,seated:true);PlaceCrowdMember(room,2,6,7,seated:true);
                    PlaceCrowdMember(room,3,6,10,9,10);PlaceCrowdMember(room,4,13,11,16,11);
                    PlaceCrowdMember(room,5,16,6,18,6);
                    break;
            }
        }

        void PopulateUpperInterior(int room,UpperRoom layout,int center)
        {
            if(layout.Surface=="bano")return;
            if(layout.Floor==1)
            {
                PopulateServicesInterior(room,layout.Name);
                return;
            }
            if(layout.Floor==2)
            {
                PopulateHealthInterior(room,layout.Name);
                return;
            }
            if(layout.Floor==3)
            {
                PopulateComputingInterior(room,layout.Name);
                return;
            }
            if(layout.Floor==4)
            {
                PopulateClassroomsInterior(room,layout.Name);
                return;
            }
            if(layout.Floor==5)
            {
                PopulateWorkshopInterior(room,layout.Name);
                return;
            }
            if(layout.Floor==6)
            {
                PlaceCrowdMember(room,0,5,7,seated:true,facesAway:true);
                PlaceCrowdMember(room,1,7,10,seated:true,facesAway:true);
                PlaceCrowdMember(room,2,16,9,16,12);
                PlaceCrowdMember(room,3,5,6,seated:true);
                return;
            }
            int index=0;
            if(layout.Name=="Aula 4A"||layout.Name=="Aula de grupos"){
                int spread=layout.Name=="Aula 4A"?4:5;
                int backRow=layout.Name=="Aula 4A"?10:11;
                foreach(int row in new[]{8,backRow})
                    foreach(int side in new[]{-1,1}){
                        if(PlaceCrowdMember(room,index,center+side*spread,row-1,seated:true,facesAway:true))index++;
                    }
            }
            for(int y=5;y<layout.Height+2;y+=2){
                for(int side=0;side<2;side++){
                    int left=side==0?3:center+2,right=side==0?center-2:layout.Width;
                    for(int x=left;x<right;x++){
                        if(!PlaceCrowdMember(room,index,x,y,x+2,y))continue;
                        index++;
                        break;
                    }
                    if(index==8)return;
                }
            }
        }

        void PopulateServicesInterior(int room,string name)
        {
            switch(name)
            {
                case "Posgrado":
                    PlaceCrowdMember(room,0,5,9,seated:true);
                    PlaceCrowdMember(room,1,4,10,seated:true);
                    PlaceCrowdMember(room,2,7,10,7,8);
                    PlaceCrowdMember(room,3,13,10,13,11);
                    break;
                case "Caja":
                    for(int i=0;i<3;i++)
                        PlaceCrowdMember(room,i,5+4*i,12,seated:true);
                    PlaceCrowdMember(room,3,5,9);
                    PlaceCrowdMember(room,4,13,9);
                    PlaceCrowdMember(room,5,4,7,seated:true);
                    PlaceCrowdMember(room,6,12,6,13,7);
                    break;
                case "Dirección de Carrera":
                    PlaceCrowdMember(room,0,4,7,seated:true);
                    PlaceCrowdMember(room,1,11,8,seated:true,facesAway:true);
                    PlaceCrowdMember(room,2,12,13,seated:true);
                    PlaceCrowdMember(room,3,6,8,6,11);
                    break;
            }
        }

        void PopulateHealthInterior(int room,string name)
        {
            switch(name)
            {
                case "Cocina de prácticas":
                    PlaceCrowdMember(room,0,5,10,8,10);
                    PlaceCrowdMember(room,1,7,7,7,8);
                    PlaceCrowdMember(room,2,16,7,16,9);
                    PlaceCrowdMember(room,3,16,13,18,13);
                    break;
                case "Sala de Simulación":
                    PlaceCrowdMember(room,0,4,7,seated:true);
                    PlaceCrowdMember(room,1,6,8,6,7);
                    PlaceCrowdMember(room,2,12,10,13,10);
                    break;
                case "Histología y Fisiología":
                    PlaceCrowdMember(room,0,5,7,5,6);
                    PlaceCrowdMember(room,1,14,7,14,9);
                    PlaceCrowdMember(room,2,5,12,7,12);
                    PlaceCrowdMember(room,3,14,12,13,12);
                    break;
            }
        }

        void PopulateComputingInterior(int room,string name)
        {
            switch(name)
            {
                case "Laboratorio PC":
                    foreach(int y in new[]{7,10})
                        foreach(int x in new[]{5,7,13,15})
                            PlaceCrowdMember(room,x+y,x,y,seated:true,facesAway:true);
                    break;
                case "Sala Mac":
                    PlaceCrowdMember(room,0,7,9,seated:true,facesAway:true);
                    PlaceCrowdMember(room,1,11,9,seated:true,facesAway:true);
                    PlaceCrowdMember(room,2,4,8,4,9);
                    break;
                case "Streaming":
                    PlaceCrowdMember(room,0,10,11,seated:true);
                    PlaceCrowdMember(room,1,11,9,11,10);
                    break;
                case "Fotografía":
                    PlaceCrowdMember(room,0,5,9,7,9);
                    PlaceCrowdMember(room,1,11,10,11,11);
                    break;
                case "Sonido":
                    PlaceCrowdMember(room,0,11,8,seated:true,facesAway:true);
                    PlaceCrowdMember(room,1,9,10,10,10);
                    break;
            }
        }

        void PopulateClassroomsInterior(int room,string name)
        {
            switch(name)
            {
                case "Sala Jurídica":
                    PlaceCrowdMember(room,0,5,8,seated:true,facesAway:true);
                    PlaceCrowdMember(room,1,13,8,seated:true,facesAway:true);
                    PlaceCrowdMember(room,2,14,12,16,12);
                    break;
                case "Cámara Gesell":
                    PlaceCrowdMember(room,0,10,8,seated:true,facesAway:true);
                    PlaceCrowdMember(room,1,12,8,seated:true,facesAway:true);
                    PlaceCrowdMember(room,2,4,8,4,9);
                    break;
                case "Aula 4A":
                    foreach(int y in new[]{8,10})
                        foreach(int x in new[]{6,14})
                            PlaceCrowdMember(room,x+y,x,y,seated:true,facesAway:true);
                    PlaceCrowdMember(room,2,9,12,12,12);
                    break;
            }
        }

        void PopulateWorkshopInterior(int room,string name)
        {
            if(name=="Aula de grupos")
            {
                PlaceCrowdMember(room,0,5,8,seated:true);
                PlaceCrowdMember(room,1,14,8,seated:true,facesAway:true);
                PlaceCrowdMember(room,2,16,11,seated:true,facesAway:true);
                PlaceCrowdMember(room,3,10,11,10,12);
                return;
            }
            PlaceCrowdMember(room,0,6,10,7,10);
            PlaceCrowdMember(room,1,12,11,12,12);
            PlaceCrowdMember(room,2,7,6,5,6);
        }

        bool PlaceCrowdMember(int room,int index,int x,int y,int toX=-1,int toY=-1,bool seated=false,
            bool facesAway=false,Vector2 seatOffset=default,string name="Estudiante",string greeting=null)
        {
            var start=Cell(room,x,y)+seatOffset;
            var end=toX<0?start:Cell(room,toX,toY);
            if(!seated&&!HasLineOfSight(start,end))return false;
            if(PersonBlocks(room,start))return false;
            int outfit=(room*5+index)%CampusOutfitCount+1;
            string art="estudiante-campus-"+outfit.ToString("00");
            string pose=seated?(facesAway?"-sentado-atras":"-sentado"):"-00";
            var actor=Art(room,art+pose,start+Vector2.up*(seated?.23f:0),
                new Vector2(.83f,seated?.92f:1.13f),greeting!=null?name:"Estudiante del campus",CampusLook.Depth(start.y)+1);
            var route=start==end?new[]{start}:new[]{start,end};
            PeopleIn(room).Add(new CampusPerson(actor.transform,name,art+"-00",greeting,route){
                WalkingArt=art,Wait=(index%4)*.28f
            });
            return true;
        }

        public CampusPerson NearbyPerson(int room,Vector2 position)
        {
            if(!campusPeople.TryGetValue(room,out var people))return null;
            foreach(var person in people)
                if(person.Greeting!=null&&Vector2.Distance(person.Position,position)<1.08f&&HasLineOfSight(position,person.Position,false))return person;
            return null;
        }

        bool PersonBlocks(int room,Vector2 position,CampusPerson ignore=null)
        {
            if(!campusPeople.TryGetValue(room,out var people))return false;
            foreach(var person in people)
                if(person!=ignore&&(person.Position-position).sqrMagnitude<.39f*.39f)return true;
            return false;
        }

        public bool CanPlayerStep(Vector2 from,Vector2 to)
        {
            if(!CanStand(to,false))return false;
            if(!campusPeople.TryGetValue(RoomAt(to),out var people))return true;
            foreach(var person in people){
                float nextDistance=(person.Position-to).sqrMagnitude;
                // A loaded checkpoint or a doorway can temporarily put both actors at the same spot.
                if(nextDistance<.39f*.39f&&nextDistance<=(person.Position-from).sqrMagnitude)return false;
            }
            return true;
        }

        public void UpdatePeople(int room,Vector2 player,float delta,float clock)
        {
            UpdateClassTraveler(room,player,delta,clock);
            if(!campusPeople.TryGetValue(room,out var people))return;
            foreach(var person in people){
                if(person==classTraveler||person.Route.Length<2)continue;
                if(person.Wait>0){person.Wait-=delta;continue;}
                var target=person.Route[person.Waypoint];
                var next=Vector2.MoveTowards(person.Position,target,1.15f*delta);
                if(!CanPersonMove(room,person,next)||Vector2.Distance(next,player)<.77f){person.Wait=.45f;continue;}
                MovePerson(person,next);
                if(next==target){
                    person.Waypoint=(person.Waypoint+1)%person.Route.Length;
                    person.Wait=1.25f;
                }
            }
        }

        void UpdateClassTraveler(int activeRoom,Vector2 player,float delta,float clock)
        {
            if(classTraveler==null||clock<nextClassChange)return;
            if(attendingClass){
                var arrival=classHallRoute[1];
                if(activeRoom==classHall&&Vector2.Distance(player,arrival)<.8f)return;
                if(!MoveClassTraveler(classRoom,classHall,arrival))return;
                attendingClass=false;
                leavingClass=true;
                return;
            }
            var target=classHallRoute[leavingClass?0:1];
            var next=Vector2.MoveTowards(classTraveler.Position,target,1.25f*delta);
            if(!CanPersonMove(classHall,classTraveler,next)||activeRoom==classHall&&Vector2.Distance(next,player)<.8f)return;
            MovePerson(classTraveler,next);
            if(next!=target)return;
            if(leavingClass){
                leavingClass=false;
                nextClassChange=clock+9f;
                return;
            }
            if(rooms[classRoom]==null)Build(classRoom);
            var classroomArrival=Entry(classRoom)+new Vector2(-2,3);
            if(activeRoom==classRoom&&Vector2.Distance(player,classroomArrival)<.8f)return;
            if(!MoveClassTraveler(classHall,classRoom,classroomArrival))return;
            attendingClass=true;
            nextClassChange=clock+14f;
        }

        void MovePerson(CampusPerson person,Vector2 next)
        {
            Vector2 advance=next-person.Position;
            if(advance.sqrMagnitude<.000001f)return;
            person.Travelled+=advance.magnitude;
            person.Actor.position=next;
            var renderer=person.Actor.GetComponent<SpriteRenderer>();
            int frame=(int)(person.Travelled*4f)%4;
            renderer.sprite=person.WalkingArt==null?CampusLook.PersonPose(person.Name,person.Sprite,advance,frame):
                CampusLook.Student(advance,frame,person.WalkingArt);
            renderer.sortingOrder=CampusLook.Depth(next.y);
        }

        bool MoveClassTraveler(int from,int to,Vector2 arrival)
        {
            if(!CanPersonMove(to,classTraveler,arrival))return false;
            PeopleIn(from).Remove(classTraveler);
            PeopleIn(to).Add(classTraveler);
            classTraveler.Actor.SetParent(rooms[to].Root.transform,true);
            classTraveler.Actor.position=arrival;
            classTraveler.Actor.GetComponent<SpriteRenderer>().sortingOrder=CampusLook.Depth(arrival.y);
            return true;
        }

        bool CanPersonMove(int room,CampusPerson person,Vector2 position)=>
            CanStand(position,false)&&!PersonBlocks(room,position,person);
    }
}
