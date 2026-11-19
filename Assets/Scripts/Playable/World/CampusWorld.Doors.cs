using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        internal enum DoorMaterial { Glass, Wood, Metal, SideGlass }

        internal sealed class FramedDoor
        {
            const int LastFrame=6;
            readonly string asset;
            internal readonly SpriteRenderer Renderer;
            internal readonly Vector2 Facing;
            internal readonly DoorMaterial Material;

            internal FramedDoor(string asset,SpriteRenderer renderer,Vector2 facing,DoorMaterial material)
            {
                this.asset=asset;
                Renderer=renderer;
                Facing=facing;
                Material=material;
            }

            internal void SetOpening(float fraction)
            {
                int frame=Mathf.RoundToInt(Mathf.Clamp01(fraction)*LastFrame);
                Renderer.sprite=CampusLook.Sprite(asset+"-"+frame.ToString("00"));
            }
        }

        FramedDoor CreateDoor(int room,Vector2 contact,Vector2 center,Vector2 size,string name,int order,
            DoorMaterial material,string caption=null,string finish=null)
        {
            finish??=material switch
            {
                DoorMaterial.Wood=>"madera",DoorMaterial.Metal=>"metal",DoorMaterial.SideGlass=>"lateral",_=>"vidrio"
            };
            string asset=(IsDark(room)?"backfranz-":"")+"puerta-marco-"+finish;
            var renderer=Art(room,asset+"-00",center,size,name,order).GetComponent<SpriteRenderer>();
            if(!string.IsNullOrEmpty(caption))
                MountPlaque(room,caption,center+Vector2.up*(size.y*.415f),.25f,order+1);
            return new FramedDoor(asset,renderer,center-contact,material);
        }

        void PlaceDoor(int room,Vector2 contact,Vector2 center,Vector2 size,string name,int order,
            DoorMaterial material=DoorMaterial.Glass,string caption=null)
        {
            rooms[room].Doors.Add(contact,CreateDoor(room,contact,center,size,name,order,material,caption));
        }

        internal FramedDoor DoorAt(int room,Vector2 contact)=>
            rooms[room].Doors.TryGetValue(contact,out var door)?door:null;

        HashSet<Vector2Int> WallOpening(int room,Vector2 contact,bool south)=>new HashSet<Vector2Int>{
            Vector2Int.RoundToInt(contact-Cell(room,0,0))+(south?Vector2Int.down:Vector2Int.up)
        };

        FramedDoor CreateWallDoor(int room,Vector2 contact,string label,DoorMaterial material,bool south,string wall,string finish=null)
        {
            if(finish!=null){
                int depth=CampusLook.Depth(contact.y)+(south?13:-7);
                var inset=contact+Vector2.up*(south?-.94f:.94f);
                return CreateDoor(room,contact,inset,new Vector2(1.47f,south?1.18f:1.50f),label,depth,material,label,finish);
            }
            int order=CampusLook.Depth(contact.y)+(south?16:-8);
            float face=south?-.78f:1.13f;
            var center=contact+Vector2.up*face;
            // The portal occupies an aperture in the border, with matching wall returns at each jamb.
            foreach(int side in new[]{-1,1})
                Art(room,wall,center+Vector2.right*(side*.71f),new Vector2(.57f,south?1.02f:1.42f),"Jamba de muro",order-1);
            var sill=Art(room,"solid",contact+Vector2.up*(south?-.49f:.46f),new Vector2(1.48f,.11f),"Umbral",-870);
            sill.GetComponent<SpriteRenderer>().color=IsDark(room)?new Color(.23f,.27f,.28f):new Color(.55f,.53f,.48f);
            return CreateDoor(room,contact,center,new Vector2(1.78f,south?1.28f:1.62f),label,order,material,label);
        }

        void PlaceWallDoor(int room,Vector2 contact,string label,DoorMaterial material,bool south=false,string wall="pb-muro-yeso",string finish=null)
        {
            rooms[room].Doors.Add(contact,CreateWallDoor(room,contact,label,material,south,wall,finish));
        }

        void PlaceWallStair(int room,Vector2 contact,string label)
        {
            var center=contact+Vector2.up*.56f;
            int order=CampusLook.Depth(contact.y)-9;
            string asset=IsDark(room)?"backfranz-escalera-empotrada":"escalera-empotrada";
            Art(room,asset,center,new Vector2(3,2.85f),label,order);
            MountPlaque(room,label,center+Vector2.up*1.13f,.22f,order+1);
        }

        void MountPlaque(int room,string label,Vector2 center,float maxHeight,int order)
        {
            const float pixelsPerUnit = 64f;
            var plaque = new GameObject("Rótulo · " + label, typeof(RectTransform), typeof(Canvas));
            plaque.transform.SetParent(rooms[room].Root.transform, false);
            plaque.transform.localScale = Vector3.one / pixelsPerUnit;
            var canvas = plaque.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = order + 1;
            var background = plaque.AddComponent<Image>();
            background.color = new Color(.055f, .09f, .12f, .94f);
            background.raycastTarget = false;
            var caption = new GameObject("Nombre del ambiente", typeof(RectTransform), typeof(Text));
            caption.transform.SetParent(plaque.transform, false);
            var rect = caption.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(7, 3);
            rect.offsetMax = new Vector2(-7, -3);
            var text = caption.GetComponent<Text>();
            var ui = FindFirstObjectByType<BackfranzUI>();
            text.font = ui && ui.status ? ui.status.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = PlaqueCaption(label);
            text.color = new Color(1f, .94f, .79f);
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 24;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            // Size the single line from its actual font metrics, including ascenders and descenders.
            // The original bottom edge stays attached to the door lintel.
            var bounds = plaque.GetComponent<RectTransform>();
            bounds.sizeDelta = new Vector2(Mathf.Ceil(text.preferredWidth) + 14,
                Mathf.Ceil(text.preferredHeight) + 6);
            float height = bounds.sizeDelta.y / pixelsPerUnit;
            plaque.transform.position = center + Vector2.up * ((height - maxHeight) * .5f);
        }


        static string PlaqueCaption(string label) => label switch
        {
            "Servicios Estudiantiles" => "Serv. estud.",
            "Dirección de Carrera" => "Dir. carrera",
            "Gradas centrales" => "Gradas",
            "Gradas del auditorio" => "Gradas aud.",
            "Galería de servicio" => "Galería",
            "Servicio eléctrico" => "Electricidad",
            "Bombas y presión" => "Bombas",
            "Cocina de prácticas" => "Cocina",
            "Sala de Simulación" => "Simulación",
            "Histología y Fisiología" => "Hist./Fisio.",
            "Laboratorio PC" => "Lab. PC",
            "Sala Jurídica" => "S. jurídica",
            "Aula de grupos" => "Aula grupos",
            "Sala de lectura" => "Lectura",
            _ => label
        };

        void PlaceLettering(int room,string label,string asset,Vector2 center,float maxWidth,float maxHeight,int order)
        {
            var sprite=CampusLook.Sprite(asset);
            float scale=Mathf.Min(maxWidth/sprite.bounds.size.x,maxHeight/sprite.bounds.size.y);
            Art(room,asset,center,(Vector2)sprite.bounds.size*scale,label,order);
        }
    }
}
