using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static readonly Vector2Int[] WorkshopSockets={new(9,16),new(20,5)};
        static readonly Vector2Int[] WorkshopRefuges={new(8,14),new(22,6)};
        static readonly string[] WorkshopLights={"Banco de trabajo","Mesa de montaje"};
        readonly SpriteRenderer[] workshopLamps=new SpriteRenderer[WorkshopSockets.Length];
        readonly SpriteRenderer[] workshopBeacons=new SpriteRenderer[WorkshopSockets.Length];
        readonly GameObject[] workshopGlows=new GameObject[WorkshopSockets.Length];
        readonly GameObject[] workshopCables=new GameObject[WorkshopSockets.Length];

        static bool WorkshopVoid(Vector2Int cell)=>cell.x<=4&&cell.y<=5||cell.x>=28&&cell.y<=4;

        static void AddWorkshopWalls(HashSet<Vector2Int> walls)
        {
            for(int x=1;x<=30;x++)
            {
                if(x>=3&&x<=27&&!(x>=6&&x<=8||x>=14&&x<=17||x>=24&&x<=27))
                    walls.Add(new Vector2Int(x,11));
                if(x>=22)walls.Add(new Vector2Int(x,7));
                for(int y=1;y<=5;y++)
                    if(WorkshopVoid(new Vector2Int(x,y)))walls.Add(new Vector2Int(x,y));
            }
        }

        void DecorateWorkshopHall(int room)
        {
            bool dark=IsDark(room);
            Sign("Piso 5 · Taller",19,19);
            if(!dark)Sign("Taller  ↓",27,11);
            Prop(room,"campus-prop-tablero-avisos",11,18,2.6f,1.28f,false);
            Prop(room,"bloque-a-pizarra-movil",4,17,2.25f,1.65f,false);
            Prop(room,"mob-mesa-taller",9,17,2.7f,1.56f);
            Prop(room,"mob-mesa-taller",20,4,2.86f,1.64f);
            Prop(room,"bloque-a-rack",21,17,1.65f,2.0f);
            Prop(room,"escena-panel-herramientas",24,18,3.6f,1.55f,false);
            Prop(room,"tour-banca-metal",26,5,2.7f,1.23f);
            Prop(room,"utileria-prensa-banco",15,16,2.67f,2.14f);
            PlaceCampusProp(room,"caja-componentes",9,17,.9f,.6f,.72f);
            PlaceRoomDetail(room,"placa-electronica",20,4,.95f,.57f,.72f);
            rooms[room].Stops.Add(new Stop(Cell(room,7,17),"Aula de grupos · proyectos"));
            rooms[room].Stops.Add(new Stop(Cell(room,25,9),"Taller · banco y herramientas"));
            rooms[room].Stops.Add(new Stop(Cell(room,20,6),"Mesa de montaje"));
            FloorLight(room,9,15,new Vector2(12,5),dark?new Color(.38f,.54f,.71f,.57f):new Color(.86f,.95f,1f,.86f));
            FloorLight(room,24,15,new Vector2(11,5),dark?new Color(.70f,.47f,.40f,.6f):new Color(1f,.80f,.52f,.88f));
            FloorLight(room,21,8,new Vector2(15,4),dark?new Color(.36f,.59f,.69f,.62f):new Color(1f,.86f,.57f,.65f));
            if(!dark)return;
            for(int x=24;x<=27;x++)
                Prop(room,"bloque-a-reja-cerrada",x,11,.98f,1.7f,false);
            Sign("Desvío  ←",25,11);
            Sign("Refugio",26,4);
            FloorLight(room,26,6,new Vector2(5,4),new Color(.48f,.87f,.69f,.86f));
            FloorLight(room,16,10,new Vector2(5,5),new Color(.62f,.82f,.92f,.67f));

            for(int i=0;i<WorkshopSockets.Length;i++)
            {
                var contact=WorkshopSocket(room,i);
                var refuge=WorkshopRefuge(room,i);
                var basePosition=contact+Vector2.up*.8f;
                workshopLamps[i]=Art(room,"aula-luz-estudio",basePosition,new Vector2(.80f,1.34f),
                    WorkshopLights[i],CampusLook.Depth(contact.y)).GetComponent<SpriteRenderer>();
                workshopBeacons[i]=Art(room,"solid",basePosition+Vector2.up*.36f,new Vector2(.31f,.12f),
                    "Testigo de lámpara",CampusLook.Depth(contact.y)+3).GetComponent<SpriteRenderer>();
                workshopGlows[i]=Art(room,"lightpool",refuge,new Vector2(6.1f,4.1f),"Luz portátil · "+WorkshopLights[i],-891);
                workshopGlows[i].GetComponent<SpriteRenderer>().color=new Color(.42f,.96f,.67f,.91f);
                workshopCables[i]=Art(room,"bloque-a-cable-caido",contact+Vector2.left*.65f+Vector2.down*.32f,
                    new Vector2(1.56f,.63f),"Cable conectado",-879);
            }
            SetWorkshopLight(-1);

            void Sign(string label,int x,int y)
            {
                var point=Cell(room,x,y);
                MountPlaque(room,label,point+Vector2.up*.20f,.22f,CampusLook.Depth(point.y)+4);
            }
        }

        internal Vector2 WorkshopSocket(int room,int index)=>Cell(room,WorkshopSockets[index].x,WorkshopSockets[index].y);
        internal Vector2 WorkshopRefuge(int room,int index)=>Cell(room,WorkshopRefuges[index].x,WorkshopRefuges[index].y);

        internal void SetWorkshopLight(int active)
        {
            for(int i=0;i<workshopGlows.Length;i++)
            {
                bool powered=i==active;
                workshopLamps[i].color=powered?new Color(.83f,1f,.89f):new Color(.45f,.48f,.54f);
                workshopBeacons[i].color=powered?new Color(.54f,1f,.73f):new Color(.38f,.37f,.37f);
                workshopGlows[i].SetActive(powered);
                workshopCables[i].SetActive(powered);
            }
        }
    }
}
