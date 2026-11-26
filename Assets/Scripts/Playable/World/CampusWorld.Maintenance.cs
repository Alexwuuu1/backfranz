using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        const int MaintenanceGateX=17, MaintenanceGateFirst=7, MaintenanceGateLast=9;
        readonly List<GameObject> maintenanceLights=new List<GameObject>();
        GameObject maintenanceGate;
        SpriteRenderer maintenanceGateSignal;

        static bool MaintenanceVoid(Vector2Int cell)=>
            cell.x<=3&&cell.y>=16||cell.x>=27&&cell.y<=4;

        static void AddMaintenanceWalls(HashSet<Vector2Int> walls)
        {
            for(int y=1;y<=18;y++)
                if(y<MaintenanceGateFirst||y>MaintenanceGateLast)
                    walls.Add(new Vector2Int(MaintenanceGateX,y));
            for(int x=1;x<=30;x++)
            {
                if(x<=3||x>=7&&x<=13)walls.Add(new Vector2Int(x,9));
                if(x>=18&&x<=25&&x!=21&&x!=22)walls.Add(new Vector2Int(x,14));
                for(int y=1;y<=18;y++)
                    if(MaintenanceVoid(new Vector2Int(x,y)))walls.Add(new Vector2Int(x,y));
            }
        }

        static bool MaintenanceGateBlocks(Vector2 local)=>
            Mathf.Abs(local.x-(MaintenanceGateX-16))<.72f&&
            local.y>=MaintenanceGateFirst-10-.72f&&local.y<=MaintenanceGateLast-10+.72f;

        static string MaintenanceSurface(int x,int y)
        {
            if(x==21&&y>=11||y==13&&x>=18&&x<=26)return "rejilla";
            if(y>=7&&y<=8&&x>=4&&x<=29||x>=4&&x<=6&&y>=5&&y<=15||
                x>=28&&y>=6&&y<=17)return "metal";
            return "subsuelo";
        }

        void DecorateBasement(int room)
        {
            Prop(room,"bloque-a-tuberia",8,18,7.3f,1.65f,false);
            Prop(room,"bloque-a-tuberia",24,18,7.5f,1.65f,false);
            Prop(room,"escena-panel-herramientas",9,18,4.2f,1.45f,false);
            Prop(room,"mob-mesa-taller",8,16,3.7f,1.75f);
            Prop(room,"bloque-a-rack",13,15,1.65f,2.12f);
            Prop(room,"bloque-a-rack",3,12,1.75f,2.1f);
            Prop(room,"bloque-a-cable-caido",5,5,2.25f,.66f,false);
            Prop(room,"campus-prop-extintor",16,6,.7f,1.17f,false);
            Prop(room,"bloque-a-charco",22,12,2.25f,.83f,false);
            Prop(room,"utileria-bomba-presion",23,13,3.35f,1.86f);
            Prop(room,"utileria-bomba-presion",27,13,3.35f,1.86f);
            Prop(room,"utileria-ventilador-muro",25,18,2.04f,1.79f,false);
            Prop(room,"bloque-a-tuberia",23,6,5.45f,1.18f,false);
            Prop(room,"mob-banca-espera",25,16,2.75f,1.22f);

            var gateCenter=Cell(room,MaintenanceGateX,8);
            Art(room,"solid",gateCenter,new Vector2(.14f,3.26f),"Riel de la reja",-895)
                .GetComponent<SpriteRenderer>().color=new Color(.58f,.62f,.61f);
            maintenanceGate=Art(room,"bloque-a-reja-cerrada",gateCenter+.12f*Vector2.up,new Vector2(3.21f,1.02f),
                "Reja de mantenimiento",CampusLook.Depth(gateCenter.y-1.25f));
            maintenanceGate.transform.Rotate(0,0,90);
            maintenanceGateSignal=Art(room,"solid",Cell(room,17,10)+Vector2.up*.65f,new Vector2(.24f,.15f),
                "Testigo de la reja",CampusLook.Depth(1f)+4).GetComponent<SpriteRenderer>();

            floorSign("Almacén de repuestos",8,17,.98f);
            floorSign("Cuadro eléctrico",12,9,.30f);
            floorSign("Bombas y presión",24,14,.36f);
            floorSign("Ascensor a PB  →",27,17,.92f);
            FloorLight(room,5,8,new Vector2(7,8),new Color(.92f,.43f,.28f,.7f));
            FloorLight(room,8,15,new Vector2(6,5),new Color(.95f,.64f,.38f,.62f));
            AddLight(12,7,5.8f,4.6f);
            AddLight(17,8,5f,3.1f);
            AddLight(24,11,10f,6.8f);
            AddLight(28,16,5.5f,6f);
            RefreshMaintenancePower(false);

            void floorSign(string title,int x,int y,float lift)
            {
                var point=Cell(room,x,y);
                MountPlaque(room,title,point+Vector2.up*lift,.22f,CampusLook.Depth(point.y)+4);
            }
            void AddLight(int x,int y,float w,float h)
            {
                var glow=Art(room,"lightpool",Cell(room,x,y),new Vector2(w,h),"Luz de emergencia de mantenimiento",-888);
                glow.GetComponent<SpriteRenderer>().color=new Color(.44f,.93f,.74f,.92f);
                maintenanceLights.Add(glow);
            }
        }

        void RefreshMaintenancePower(bool active)
        {
            maintenanceGate.SetActive(!active);
            maintenanceGateSignal.color=active?new Color(.48f,1f,.72f):new Color(1f,.40f,.23f);
            foreach(var light in maintenanceLights)light.SetActive(active);
        }

        internal SpriteRenderer PlaceServiceEquipment(int room,Vector2 position,string name,string asset,Vector2 size)
        {
            var basePoint=position+Vector2.up*.71f;
            rooms[room].Obstacles.Add(new Rect(basePoint-new Vector2(size.x*.33f,.19f),new Vector2(size.x*.66f,.38f)));
            var body=basePoint+Vector2.up*(size.y/2-.30f);
            return Art(room,asset,body,size,name,CampusLook.Depth(basePoint.y)).GetComponent<SpriteRenderer>();
        }
    }
}
