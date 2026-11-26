using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static bool ClassroomsVoid(Vector2Int cell)=>cell.x<=2&&cell.y<=6||cell.x>=26&&cell.y<=4;

        static void AddClassroomsWalls(HashSet<Vector2Int> walls)
        {
            for(int x=1;x<=30;x++)
            {
                if(!(x>=4&&x<=6||x>=14&&x<=17||x>=25&&x<=28))walls.Add(new Vector2Int(x,11));
                if(x>=10&&x<=23)walls.Add(new Vector2Int(x,6));
                for(int y=1;y<=6;y++)
                    if(ClassroomsVoid(new Vector2Int(x,y)))walls.Add(new Vector2Int(x,y));
            }
            for(int y=12;y<=18;y++)
                if(y!=14&&y!=15)walls.Add(new Vector2Int(22,y));
        }

        void DecorateClassroomsHall(int room)
        {
            bool dark=IsDark(room);
            for(int x=11;x<=20;x+=3)Prop(room,"tour-ventanal",x,19,2.8f,1.48f,false);
            Prop(room,"campus-prop-tablero-avisos",4,18,2.05f,1.36f,false);
            Prop(room,"mob-banca-espera",12,12,2.55f,1.2f);
            Prop(room,"campus-prop-dispensador-agua",19,12,1.02f,1.42f);
            Prop(room,"mob-banca-pasillo",24,17,2.26f,1.1f);
            Prop(room,"mob-estante-exhibidor",27,8,1.55f,1.63f);
            Sign("Piso 4 · Aulas",14,19);
            Sign("Observación",25,19);
            Sign("Prácticas",18,11);
            Sign("4B",11,6);
            Sign("4C",16,6);
            Sign(dark?"4B":"4D",22,6);
            rooms[room].Stops.Add(new Stop(Cell(room,7,17),"Sala Jurídica · prácticas de audiencia"));
            rooms[room].Stops.Add(new Stop(Cell(room,26,17),"Observación · Cámara Gesell"));
            rooms[room].Stops.Add(new Stop(Cell(room,14,8),"Aula 4A · prácticas"));
            FloorLight(room,14,16,new Vector2(15,5),dark?new Color(.49f,.69f,.84f,.64f):new Color(.80f,.94f,1f,.88f));
            FloorLight(room,26,15,new Vector2(7,6),dark?new Color(.6f,.49f,.71f,.76f):new Color(.95f,.88f,.73f,.65f));
            FloorLight(room,14,8,new Vector2(9,4),dark?new Color(.73f,.55f,.39f,.69f):new Color(1f,.89f,.68f,.8f));
            if(!dark)return;
            foreach(int x in new[]{11,22})
            {
                Prop(room,"campus-prop-extintor",x+1,5,.62f,1.14f,false);
                FloorLight(room,x,4,new Vector2(5,4),new Color(.64f,.56f,.84f,.72f));
            }
            Prop(room,"bloque-a-charco",14,3,2.5f,1.07f,false);
            var lamp=Cell(room,20,6)+new Vector2(.65f,.48f);
            int lampOrder=CampusLook.Depth(lamp.y)+4;
            Art(room,"solid",lamp,new Vector2(.24f,.30f),"Marco de la luz del desvío",lampOrder)
                .GetComponent<SpriteRenderer>().color=new Color(.13f,.20f,.23f);
            Art(room,"solid",lamp+Vector2.up*.035f,new Vector2(.14f,.12f),"Luz verde del desvío",lampOrder+1)
                .GetComponent<SpriteRenderer>().color=new Color(.52f,.96f,.74f);
            FloorLight(room,20,5,new Vector2(3,4),new Color(.47f,.95f,.75f,.87f));
            FloorLight(room,4,5,new Vector2(4,3),new Color(.44f,.85f,.73f,.8f));

            void Sign(string label,int x,int y)
            {
                var point=Cell(room,x,y);
                // The false classroom numbers face the lower corridor.
                float height=y==6?-.43f:.2f;
                MountPlaque(room,label,point+Vector2.up*height,.22f,CampusLook.Depth(point.y)+(y==6?18:4));
            }
        }

        internal Vector2 ClassroomTurnoff(int room)=>Cell(room,20,5);

        internal void ShowClassroomTurnoff(int room)
        {
            var contact=ClassroomTurnoff(room);
            if(DoorAt(room,contact)!=null)return;
            PlaceWallDoor(room,contact,"Desvío",DoorMaterial.Metal,false,"tour-muro-blanco","servicios");
        }

        internal void ShowClassroomChalkMark(int room)
        {
            var mark=Cell(room,11,4)+Vector2.down*.22f;
            var chalk=new Color(.93f,.90f,.77f);
            foreach(float angle in new[]{-45f,45f})
            {
                var stroke=Art(room,"solid",mark,new Vector2(.48f,.065f),"Marca de tiza",-876);
                stroke.GetComponent<SpriteRenderer>().color=chalk;
                stroke.transform.Rotate(0,0,angle);
            }
        }
    }
}
