using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static void AddHealthWalls(HashSet<Vector2Int> walls)
        {
            for(int x=3;x<=25;x++)
                if(x<11||x>14)walls.Add(new Vector2Int(x,12));
            for(int x=1;x<=30;x++)
                for(int y=1;y<=5;y++)
                    if(HealthVoid(new Vector2Int(x,y)))walls.Add(new Vector2Int(x,y));
        }

        static bool HealthVoid(Vector2Int cell)=>cell.x<=8&&cell.y<=4||cell.x>=27&&cell.y<=5;

        void DecorateHealthHall(int room)
        {
            bool dark=IsDark(room);
            Sign("Piso 2 · Salud y cocina",19,19);
            Sign("Cocina  ↓",11,19);
            Prop(room,"mob-banca-pasillo",10,17,3.4f,1.19f);
            Prop(room,"mob-banca-espera",24,13,2.75f,1.24f);
            Prop(room,"campus-prop-tablero-avisos",18,18,2.4f,1.45f,false);
            Prop(room,"campus-prop-dispensador-agua",26,17,1.1f,1.62f);
            PlaceWashStation(room,5,8,1.85f,1.49f);
            Prop(room,"bloque-a-mesa-registro",7,6,3.15f,1.68f);
            PlaceRoomDetail(room,"bandeja-gastro",7,6,1,.62f,.74f);
            Prop(room,"tour-banca-metal",23,5,3.15f,1.25f);
            Prop(room,"campus-prop-extintor",10,11,.65f,1.18f,false);
            rooms[room].Stops.Add(new Stop(Cell(room,6,10),"Cocina · despacho de prácticas"));
            rooms[room].Stops.Add(new Stop(Cell(room,15,17),"Simulación · control de ingreso"));
            rooms[room].Stops.Add(new Stop(Cell(room,23,17),"Histología y Fisiología"));
            for(int x=5;x<=29;x+=6)
                FloorLight(room,x,15,new Vector2(7,4),dark?new Color(.41f,.58f,.73f,.68f):new Color(.81f,.94f,1f,.82f));
            FloorLight(room,6,9,new Vector2(9,5),dark?new Color(.91f,.58f,.36f,.57f):new Color(1f,.8f,.58f,.88f));
            FloorLight(room,23,7,new Vector2(7,5),dark?new Color(.48f,.93f,.75f,.9f):new Color(.82f,.94f,1f,.78f));
            if(!dark)return;
            for(int y=13;y<=18;y++)
                Prop(room,"bloque-a-reja-cerrada",20,y,.88f,1.25f,false);
            Sign("Aislamiento",20,16);
            Sign("Refugio",23,6);
            Prop(room,"bloque-a-cable-caido",18,9,2.3f,.75f,false);
            Prop(room,"bloque-a-alarma",25,12,.77f,1.22f,false);
            FloorLight(room,13,11,new Vector2(4,5),new Color(.62f,.81f,1f,.64f));
            FloorLight(room,27,10,new Vector2(5,4),new Color(.62f,.81f,1f,.64f));

            void Sign(string label,int x,int y)
            {
                var point=Cell(room,x,y);
                MountPlaque(room,label,point+Vector2.up*.2f,.22f,CampusLook.Depth(point.y)+4);
            }
        }
    }
}
