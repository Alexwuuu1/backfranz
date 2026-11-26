using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static readonly Vector2Int[] GalleryLights =
        {
            new(4,5), new(9,5), new(16,5), new(23,5), new(27,6),
            new(27,12), new(24,16), new(17,16), new(10,16), new(5,13)
        };
        SpriteRenderer galleryAccessLight;

        public Vector2 GalleryEntrance => Cell(DarkGround,11,18);
        public Vector2 GalleryExit => Cell(ServiceGallery,4,3);
        public int GalleryLightCount => GalleryLights.Length;
        public Vector2 GalleryLight(int index) => Cell(ServiceGallery,GalleryLights[index].x,GalleryLights[index].y);

        void BuildGalleryApproach()
        {
            int room=DarkGround;
            PlaceWallDoor(room,GalleryEntrance,"Galería de servicio",DoorMaterial.Metal,false,"escena-muro-servicio");
            Prop(room,"bloque-a-tuberia",10,16,3.2f,.91f,false);
            FloorLight(room,11,17,new Vector2(4.4f,3.8f),new Color(.50f,.74f,.61f,.55f));
            galleryAccessLight=Art(room,"solid",GalleryEntrance+new Vector2(.69f,.81f),new Vector2(.17f,.13f),
                "Testigo de acceso a la galería",CampusLook.Depth(GalleryEntrance.y)+5).GetComponent<SpriteRenderer>();
            SetGalleryAccess(false);
        }

        internal void SetGalleryAccess(bool ready)
        {
            galleryAccessLight.color=ready?new Color(.48f,1f,.72f):new Color(1f,.48f,.28f);
        }

        void BuildServiceGallery()
        {
            int room=ServiceGallery;
            var area=new HashSet<Vector2Int>();
            for(int x=3;x<=28;x++)
                for(int y=3;y<=17;y++)
                {
                    if(y>=8&&y<=12&&x>=9&&x<=13||y>=9&&y<=13&&x>=17&&x<=22)continue;
                    if(x<=4&&y>=15||x>=26&&y>=16||x>=27&&y<=4)continue;
                    area.Add(new Vector2Int(x,y));
                }
            GroundSurface(room,area,"subsuelo","escena-muro-servicio",
                cell=>cell.y==7&&cell.x>=8&&cell.x<=24?"rejilla":
                    cell.y<=5||cell.y>=15||cell.x>=26||cell.x<=5?"metal":null,WallOpening(room,GalleryExit,true));
            var equipmentFloor=TileFor(room,"escena-suelo-rejilla");
            FillEquipmentWell(9,13,8,12);
            FillEquipmentWell(17,22,9,13);

            foreach(var light in GalleryLights)
            {
                var at=Cell(room,light.x,light.y);
                var size=light.x==27?new Vector2(.14f,2.6f):new Vector2(2.6f,.14f);
                Art(room,"solid",at,size,"Franja de luz estable",-884).GetComponent<SpriteRenderer>()
                    .color=new Color(.53f,.95f,.78f,.87f);
                FloorLight(room,light.x,light.y,new Vector2(3.9f,3.9f),new Color(.38f,1f,.79f,.93f));
            }

            Prop(room,"bloque-a-tuberia",11,10,4.9f,1.27f,false);
            Prop(room,"bloque-a-rack",11,8,2.75f,1.75f,false);
            Prop(room,"utileria-bomba-presion",19,12,3.45f,1.93f,false);
            Prop(room,"bloque-a-tuberia",19,10,4.45f,1.26f,false);
            Prop(room,"bloque-a-cable-caido",14,6,2.38f,.62f,false);
            Prop(room,"campus-prop-extintor",24,14,.65f,1.2f,false);
            Prop(room,"bloque-a-alarma",28,14,.76f,1.18f,false);
            Prop(room,"tour-banca-metal",6,16,2.0f,1.24f);
            FloorLight(room,28,10,new Vector2(3.5f,5.1f),new Color(.83f,.34f,.27f,.66f));
            PlaceWallDoor(room,GalleryExit,"Planta baja",DoorMaterial.Metal,true,"escena-muro-servicio");

            void FillEquipmentWell(int firstX,int lastX,int firstY,int lastY)
            {
                for(int x=firstX;x<=lastX;x++)
                    for(int y=firstY;y<=lastY;y++)
                        PaintSurface(rooms[room].Surface,x,y,equipmentFloor);
            }
        }
    }
}
