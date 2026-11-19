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

        public Vector2 GalleryEntrance => Cell(DarkGround,11,18);
        public Vector2 GalleryExit => Cell(ServiceGallery,4,3);
        public int GalleryLightCount => GalleryLights.Length;
        public Vector2 GalleryLight(int index) => Cell(ServiceGallery,GalleryLights[index].x,GalleryLights[index].y);

        void BuildServiceGallery()
        {
            int room=ServiceGallery;
            var area=new HashSet<Vector2Int>();
            for(int x=3;x<=28;x++)
                for(int y=3;y<=17;y++)
                {
                    bool equipmentRow=y>=8&&y<=12;
                    if(equipmentRow&&(x>=10&&x<=13||x>=18&&x<=21))continue;
                    area.Add(new Vector2Int(x,y));
                }
            GroundSurface(room,area,"subsuelo","escena-muro-servicio",
                cell=>cell.y==7&&cell.x>=8&&cell.x<=24?"rejilla":
                    cell.y<=5||cell.y==15?"metal":null,WallOpening(room,GalleryExit,true));

            foreach(var light in GalleryLights)
            {
                var at=Cell(room,light.x,light.y);
                var line=Art(room,"solid",at,new Vector2(2.3f,.12f),"Franja de luz estable",-906);
                line.GetComponent<SpriteRenderer>().color=new Color(.57f,.90f,.80f,.75f);
                FloorLight(room,light.x,light.y,new Vector2(5,4.8f),new Color(.34f,1,.79f,.9f));
            }
            foreach(int x in new[]{11,19})
            {
                Prop(room,"bloque-a-tuberia",x,12,4.1f,1.75f,false);
                Prop(room,"bloque-a-rack",x+1,9,1.5f,1.8f,false);
            }
            Prop(room,"escena-panel-herramientas",17,17,5.5f,1.65f,false);
            PlaceWallDoor(room,GalleryExit,"Salida",DoorMaterial.Metal,true,"escena-muro-servicio");
        }
    }
}
