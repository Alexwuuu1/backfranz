using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static void AddServicesWalls(HashSet<Vector2Int> walls)
        {
            // The north corridor serves the offices. The south corridor joins it at both ends.
            for(int x=7;x<=24;x++)walls.Add(new Vector2Int(x,11));
            for(int x=1;x<=6;x++)
                for(int y=1;y<=6;y++)walls.Add(new Vector2Int(x,y));
            for(int x=7;x<=18;x++)
                for(int y=1;y<=3;y++)walls.Add(new Vector2Int(x,y));
            for(int x=29;x<=30;x++)
                for(int y=1;y<=7;y++)walls.Add(new Vector2Int(x,y));
        }

        static bool ServicesVoid(Vector2Int cell)=>
            cell.x<=6&&cell.y<=6||cell.x<=18&&cell.y<=3||cell.x>=29&&cell.y<=7;

        void DecorateServicesHall(int room)
        {
            bool dark=IsDark(room);
            PlaceWallSign("Posgrado y Caja  ←",27,19);
            PlaceWallSign("Piso 1 · Servicios",22,19);
            PlaceWallSign("Arca",21,4);
            Prop(room,"campus-prop-tablero-avisos",10,18,3.1f,1.32f,false);
            Prop(room,"mob-banca-espera",11,12,3.55f,1.28f);
            Prop(room,"mob-banca-espera",14,12,2.25f,1.24f);
            Prop(room,"tour-jardinera",4,18,1.42f,1.45f);
            Prop(room,"tour-jardinera",20,18,1.45f,1.42f);
            Prop(room,"tour-mostrador-blanco",21,4,3.1f,1.85f);
            Prop(room,"mob-banca-pasillo",10,5,3.45f,1.19f);
            Prop(room,"mob-banca-pasillo",14,5,3.45f,1.19f);
            Prop(room,"campus-prop-extintor",6,12,.62f,1.1f,false);
            Prop(room,"mob-banca-espera",27,4,2.4f,1.24f);
            rooms[room].Stops.Add(new Stop(Cell(room,21,6),"Arca · orientación y trámites"));
            rooms[room].Stops.Add(new Stop(Cell(room,7,17),"Posgrado · orientación académica"));
            rooms[room].Stops.Add(new Stop(Cell(room,15,17),"Caja · turnos y pagos"));
            rooms[room].Stops.Add(new Stop(Cell(room,23,9),"Dirección de Carrera · citas"));
            for(int x=5;x<=29;x+=6)
                FloorLight(room,x,15,new Vector2(7,3.6f),dark?new Color(.52f,.70f,.79f,.6f):new Color(.94f,.92f,.79f,.76f));
            FloorLight(room,21,7,new Vector2(10,4),dark?new Color(.44f,.77f,.69f,.70f):new Color(.87f,.97f,1,.8f));
            if(!dark)return;

            for(int y=12;y<=18;y++)
                Prop(room,"bloque-a-reja-cerrada",18,y,.86f,1.25f,false);
            MountPlaque(room,"Persiana cerrada",Cell(room,18,16)+Vector2.up*.38f,.22f,CampusLook.Depth(Cell(room,18,16).y)+4);
            FloorLight(room,18,15,new Vector2(5,5),new Color(1,.30f,.27f,.74f));
            FloorLight(room,26,10,new Vector2(5,5),new Color(.46f,.93f,.84f,.85f));
            FloorLight(room,5,10,new Vector2(5,5),new Color(.46f,.93f,.84f,.85f));
            FloorLight(room,12,15,new Vector2(4.5f,3.2f),new Color(.65f,.83f,.63f,.80f));

            void PlaceWallSign(string label,int x,int y)
            {
                var position=Cell(room,x,y);
                MountPlaque(room,label,position+Vector2.up*.16f,.22f,CampusLook.Depth(position.y)+4);
            }
        }

        internal void MarkServicesDetour(int room)
        {
            foreach(var (x,y) in new[]{(26,10),(22,8),(17,8),(12,8),(7,9),(5,11)})
            {
                var point=Cell(room,x,y);
                var stripe=Art(room,"solid",point+Vector2.down*.24f,new Vector2(.72f,.115f),"Marca del desvío",-878);
                stripe.GetComponent<SpriteRenderer>().color=new Color(.50f,.88f,.76f,.94f);
            }
        }
    }
}
