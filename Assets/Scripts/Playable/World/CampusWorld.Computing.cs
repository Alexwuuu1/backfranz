using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static bool ComputingVoid(Vector2Int cell)=>cell.x<=5&&cell.y<=5||cell.x>=28&&cell.y<=4;

        static void AddComputingWalls(HashSet<Vector2Int> walls)
        {
            for(int x=1;x<=30;x++)
            {
                if(x<7||x>10)
                    if(x<16||x>18)
                        if(x<27||x>29)walls.Add(new Vector2Int(x,12));
                if(x>=6&&x<=27)walls.Add(new Vector2Int(x,5));
                for(int y=1;y<=5;y++)
                    if(ComputingVoid(new Vector2Int(x,y)))walls.Add(new Vector2Int(x,y));
            }
        }

        void DecorateComputingHall(int room)
        {
            bool dark=IsDark(room);
            Sign("Piso 3 · Computación",15,19);
            Sign("← Estudios",20,12);
            Prop(room,"campus-prop-tablero-avisos",11,18,2.65f,1.4f,false);
            Prop(room,"mob-banca-pasillo",11,13,2.9f,1.21f);
            Prop(room,"bloque-a-rack",20,17,1.55f,2);
            Prop(room,"tour-banca-metal",25,13,2.75f,1.26f);
            Prop(room,"bloque-a-rack",5,10,1.55f,1.9f);
            Prop(room,"utileria-cartel-aire",10,5,1.4f,.7f,false);
            Prop(room,"bloque-a-tripode",18,6,1.15f,1.45f);
            Prop(room,"mob-mesa-auxiliar",25,6,1.24f,1.03f);
            Prop(room,"campus-prop-extintor",27,6,.65f,1.12f,false);
            rooms[room].Stops.Add(new Stop(Cell(room,6,17),"Laboratorio PC · prácticas"));
            rooms[room].Stops.Add(new Stop(Cell(room,23,17),"Sala Mac · edición"));
            rooms[room].Stops.Add(new Stop(Cell(room,17,10),"Streaming, Fotografía y Sonido"));
            rooms[room].Stops.Add(new Stop(Cell(room,25,7),"Mesa de prácticas de Sonido"));
            for(int x=6;x<=30;x+=6)
                FloorLight(room,x,15,new Vector2(8,4),dark?new Color(.45f,.67f,.83f,.61f):new Color(.72f,.91f,1f,.82f));
            FloorLight(room,9,8,new Vector2(6,4),dark?new Color(.64f,.50f,.82f,.70f):new Color(.91f,.79f,1f,.68f));
            FloorLight(room,23,8,new Vector2(6,4),dark?new Color(.57f,.92f,.78f,.80f):new Color(.75f,.93f,.87f,.62f));
            if(!dark)return;
            for(int x=16;x<=18;x++)
                Prop(room,"bloque-a-rack",x,12,.94f,1.72f,false);
            Sign("Fuera de servicio",17,12);
            FloorLight(room,17,12,new Vector2(4,4),new Color(1f,.32f,.40f,.72f));
            FloorLight(room,8,12,new Vector2(5,6),new Color(.64f,.50f,.82f,.86f));
            FloorLight(room,28,11,new Vector2(5,6),new Color(.49f,.82f,.92f,.81f));
            Prop(room,"bloque-a-cable-caido",17,8,1.75f,.85f,false);
            Prop(room,"bloque-a-charco",22,8,3.1f,1.08f,false);

            void Sign(string label,int x,int y)
            {
                var point=Cell(room,x,y);
                MountPlaque(room,label,point+Vector2.up*.26f,.22f,CampusLook.Depth(point.y)+5);
            }
        }
    }
}
