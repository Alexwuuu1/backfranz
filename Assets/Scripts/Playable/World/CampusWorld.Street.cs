using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        const int StreetWidth=40;
        public Vector2 StudentContact=>Cell(Entrance,5,11);
        public Vector2 StreetFoot=>Cell(Entrance,10,10);
        public Vector2 StreetLanding=>Cell(Entrance,10,14);
        public Vector2 StreetDoor=>Cell(Entrance,14,14);
        public Vector2 StreetReturn=>StreetDoor+Vector2.left;

        void BuildStreet()
        {
            var walkable=new HashSet<Vector2Int>();
            void Fill(int left,int bottom,int right,int top)
            {
                for(int x=left;x<=right;x++)
                    for(int y=bottom;y<=top;y++)walkable.Add(new Vector2Int(x,y));
            }

            // Landaeta, the opposite pavement and the lower walk along the university.
            Fill(0,3,StreetWidth-1,10);
            Fill(0,11,7,13);
            Fill(15,11,StreetWidth-1,11);
            // Side walls leave a single approach to the upper landing and its turn east.
            Fill(9,11,11,12);
            Fill(9,13,14,15);
            Fill(9,16,12,19);
            rooms[Entrance].Cells=walkable;
            var surface=CreateSurfaceMap(Entrance);
            foreach(var cell in walkable)
            {
                int x=cell.x,y=cell.y;
                string asset;
                if(y>=4&&y<=7)
                    asset="acceso-calle-"+((x+y*2)%3)+"-adoquin";
                else if(y==3)asset="acceso-bordillo-sur";
                else if(y==8)asset="acceso-bordillo-norte";
                else if(x>=9&&x<=11&&y>=11&&y<=12)asset="acceso-escalon";
                else if(x>=9&&x<=14&&y>=13)asset="acceso-rellano";
                else asset="acceso-vereda";
                PaintSurface(surface,x,y,TileFor(Entrance,asset));
            }

            BuildStreetFrontages();
            BuildStreetSteps();
            StreetVehicle("acceso-auto",20,6,4.45f,1.88f);
            StreetVehicle("acceso-furgoneta",31,6,4.9f,2.05f);
            Prop(Entrance,"pb-arbol",2,10,3.15f,3.7f);
            Prop(Entrance,"tour-jardinera",20,11,1.85f,1.4f);
            PlaceSpeaker(Entrance,"pb-estudiante","Estudiante",StudentContact);
        }

        void BuildStreetFrontages()
        {
            var shops=Cell(Entrance,4,16)+new Vector2(-.5f,.12f);
            Art(Entrance,"acceso-tiendas",shops,new Vector2(8f,5.24f),"Tiendas de Landaeta",CampusLook.Depth(shops.y-2.62f));
            StreetLettering("Tiendas",Cell(Entrance,4,17)+new Vector2(-.05f,-.09f),.061f);

            for(int x=17;x<StreetWidth;x+=5)
            {
                var point=Cell(Entrance,x,15);
                Art(Entrance,x==17?"acceso-campus-esquina":"acceso-campus-modulo",point,
                    new Vector2(5f,7f),"Fachada del Bloque A",CampusLook.Depth(point.y-3.5f));
            }
            StreetLettering("Bloque A",Cell(Entrance,17,16)+new Vector2(.16f,.45f),.075f);
            var upperFace=Cell(Entrance,14,18)+new Vector2(.18f,.27f);
            Art(Entrance,"acceso-lateral",upperFace,new Vector2(2.45f,5.4f),
                "Lateral del Bloque A",CampusLook.Depth(upperFace.y-2.7f));

            // The sign and glazing share the side wall at the end of the landing.
            var jamb=StreetDoor+new Vector2(.77f,.31f);
            int wallOrder=CampusLook.Depth(Cell(Entrance,15,11).y)+2;
            Art(Entrance,"acceso-lateral",jamb+Vector2.right*.16f,new Vector2(1.68f,3.15f),
                "Marco lateral de la entrada",wallOrder);
            PlaceDoor(Entrance,StreetDoor,jamb,new Vector2(1.34f,2.28f),"Entrada lateral acristalada",
                wallOrder+1,DoorMaterial.SideGlass);

            for(int x=2;x<StreetWidth;x+=5)
            {
                var point=Cell(Entrance,x,1);
                Art(Entrance,"acceso-instituto",point,new Vector2(5f,2.85f),"Instituto Americano",
                    CampusLook.Depth(point.y-1.4f));
            }
            var institute=Cell(Entrance,8,2)+new Vector2(.35f,.08f);
            var plaque=Art(Entrance,"solid",institute,new Vector2(5.2f,.4f),"Placa del Instituto",855);
            plaque.GetComponent<SpriteRenderer>().color=new Color(.22f,.28f,.27f);
            StreetLettering("Instituto Americano",institute,.052f);
        }

        void BuildStreetSteps()
        {
            var middle=Cell(Entrance,10,12)+Vector2.down*.5f;
            var retainingWall=Cell(Entrance,8,13);
            Art(Entrance,"acceso-murete",retainingWall,new Vector2(.94f,5f),
                "Murete de las gradas",CampusLook.Depth(retainingWall.y));
            var plinth=Cell(Entrance,13,12)+Vector2.down*.5f;
            Art(Entrance,"acceso-plinto",plinth,new Vector2(3f,2f),"Base del rellano",
                CampusLook.Depth(plinth.y)-1);
            foreach(float side in new[]{-1.52f,1.52f})
                Art(Entrance,"acceso-baranda",middle+new Vector2(side,.13f),new Vector2(.58f,2.32f),
                    "Baranda fija",CampusLook.Depth(middle.y)+10);
            var alleyEnd=Cell(Entrance,10,20)+Vector2.right*.5f;
            Art(Entrance,"campus-arq-baranda",alleyEnd,new Vector2(4.1f,1.0f),
                "Límite de la calle lateral",CampusLook.Depth(alleyEnd.y));
        }

        void StreetVehicle(string asset,int x,int y,float width,float height)
        {
            var point=Cell(Entrance,x,y);
            Art(Entrance,asset,point,new Vector2(width,height),"Vehículo estacionado",
                CampusLook.Depth(point.y-height*.4f));
            var body=new Vector2(width*.9f,height*.8f);
            rooms[Entrance].Obstacles.Add(new Rect(point-body*.5f,body));
        }

        void StreetLettering(string words,Vector2 position,float size)
        {
            PlaceLettering(Entrance,words,"letras-"+CampusLook.AssetKey(words),position,
                words=="Instituto Americano"?4.7f:2.75f,size*3.1f,856);
        }
    }
}
