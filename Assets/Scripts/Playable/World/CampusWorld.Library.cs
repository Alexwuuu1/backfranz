using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static bool LibraryVoid(Vector2Int cell)=>cell.x<=4&&cell.y<=5||cell.x>=28&&cell.y<=4;
        static bool LibraryArchiveOpening(Vector2Int cell)=>cell.y==10&&cell.x>=26&&cell.x<=28;

        static void AddLibraryWalls(HashSet<Vector2Int> walls)
        {
            for(int x=1;x<=30;x++)
            {
                var partition=new Vector2Int(x,10);
                if(x!=6&&x!=7&&!(x>=15&&x<=18)&&!LibraryArchiveOpening(partition))walls.Add(partition);
                for(int y=1;y<=5;y++)
                    if(LibraryVoid(new Vector2Int(x,y)))walls.Add(new Vector2Int(x,y));
            }
        }

        void DecorateLibraryHall(int room)
        {
            bool dark=IsDark(room);
            Prop(room,"campus-prop-mostrador-biblioteca",15,17,4.7f,2.15f);
            PlaceRoomDetail(room,"lector",16,17,.86f,.57f,.82f);
            if(!dark)PlaceRoomDetail(room,"cuaderno",14,17,.86f,.58f,.86f);
            Prop(room,"campus-prop-estante-revistas",4,17,2.45f,1.7f);
            Prop(room,"tour-ventanal",23,18,6.0f,1.54f,false);
            Prop(room,"mob-banca-espera",23,16,3.3f,1.25f);
            Prop(room,"mob-carro-libros",9,16,1.5f,1.45f);
            Prop(room,"mob-estante-bajo",5,7,2.55f,1.58f);
            Prop(room,"mob-mesa-lectura-lampara",15,10,1.78f,1.46f);
            PlaceRoomDetail(room,"cuaderno",15,10,.72f,.52f,.68f);

            for(int y=5;y<=7;y+=2)
            {
                Prop(room,"mob-estante-roble",19,y,3.18f,1.88f);
                Prop(room,"mob-estante-archivo",25,y,3.25f,2.06f);
            }
            Prop(room,"mob-mesa-lectura-doble",10,5,3.4f,1.68f);
            PlaceCampusProp(room,"separalibros",19,7,.8f,.59f,1.33f);

            Sign("Préstamos",15,19);
            Sign(dark?"Archivo · vía central":"Archivo  →",22,10);
            rooms[room].Stops.Add(new Stop(Cell(room,15,15),"Consulta y préstamo de material"));
            rooms[room].Stops.Add(new Stop(Cell(room,9,6),"Mesa de consulta libre"));
            FloorLight(room,16,16,new Vector2(11,6),dark?new Color(.84f,.58f,.37f,.71f):new Color(1f,.86f,.61f,.91f));
            FloorLight(room,15,9,new Vector2(5,4),dark?new Color(.88f,.69f,.42f,.8f):new Color(.97f,.84f,.64f,.71f));
            FloorLight(room,23,13,new Vector2(10,6),dark?new Color(.39f,.58f,.71f,.51f):new Color(.77f,.9f,1f,.79f));
            FloorLight(room,10,5,new Vector2(7,4),dark?new Color(.35f,.54f,.61f,.4f):new Color(1f,.85f,.67f,.75f));
            if(!dark)return;

            Prop(room,"mob-estante-archivo",27,10,3.13f,2.07f,false);
            Prop(room,"bloque-a-cable-caido",27,11,2.3f,.58f,false);
            Prop(room,"bloque-a-charco",22,5,2.05f,.83f,false);
            FloorLight(room,22,7,new Vector2(5,8),new Color(.37f,.66f,.71f,.56f));

            void Sign(string title,int x,int y)
            {
                Vector2 point=Cell(room,x,y);
                MountPlaque(room,title,point+Vector2.up*.38f,.21f,CampusLook.Depth(point.y)+4);
            }
        }

        void DecorateReadingRoom(int room)
        {
            bool dark=IsDark(room);
            Prop(room,"tour-ventanal",6,13,6.2f,1.55f,false);
            Prop(room,"mob-mesa-lectura-lampara",6,11,3.7f,1.75f);
            Prop(room,"mob-mesa-lectura-doble",6,8,3.45f,1.72f);
            foreach(int y in new[]{7,10})
                foreach(int x in new[]{5,7})
                    Prop(room,"mob-silla-visita",x,y,1.06f,1.12f);
            PlaceRoomDetail(room,"cuaderno",6,11,.79f,.55f,.74f);
            PlaceCampusProp(room,"lupa-lectura",7,8,.64f,.48f,.75f);
            Prop(room,"mob-estante-exhibidor",17,13,3.66f,1.95f);
            Prop(room,"mob-estante-roble",14,10,2.48f,2.04f);
            Prop(room,"mob-estante-roble",18,10,2.48f,2.04f);
            Prop(room,"mob-carro-libros",19,7,1.27f,1.41f);
            Prop(room,"mob-banca-espera",5,6,3.3f,1.32f);
            FloorLight(room,6,10,new Vector2(7,8),dark?new Color(.82f,.68f,.48f,.67f):new Color(1f,.88f,.64f,.85f));
            FloorLight(room,16,11,new Vector2(6,6),dark?new Color(.37f,.59f,.72f,.64f):new Color(.78f,.93f,1f,.63f));
        }
    }
}
