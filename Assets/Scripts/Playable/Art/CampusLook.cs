using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public static class CampusLook
    {
        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public static Sprite Sprite(string name)
        {
            if (sprites.TryGetValue(name, out var cached) && cached) return cached;
            var texture = Resources.Load<Texture2D>("Backfranz/Art/" + name);
            if (!texture) throw new System.InvalidOperationException("Falta exportación del asset ASE: " + name);
            texture.filterMode = FilterMode.Point;
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect,
                name=="panel" || name.StartsWith("button") ? new Vector4(8,8,8,8) : Vector4.zero);
            sprites[name] = sprite; return sprite;
        }
        public static int Depth(float y) => 500-Mathf.RoundToInt(y*10);
        public static GameObject Art(Transform parent,string asset,Vector2 position,Vector2 size,string name,int? order=null)
        {
            var g = new GameObject(name);g.transform.SetParent(parent);g.transform.position=position;
            var renderer=g.AddComponent<SpriteRenderer>();renderer.sprite=Sprite(asset);
            renderer.sortingOrder=order ?? Depth(position.y);
            g.transform.localScale=new Vector3(size.x/renderer.sprite.bounds.size.x,size.y/renderer.sprite.bounds.size.y,1);
            return g;
        }
        public static GameObject Tile(Transform parent,Vector2 position,Vector2 size,Color color,int order,string name,bool patterned=false)
        {
            var g=Art(parent,patterned?"floor-clean":"solid",position,size,name,order);g.GetComponent<SpriteRenderer>().color=color;return g;
        }
        public static Sprite Student(Vector2 facing,int frame)
        {
            int direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x<0?1:2):(facing.y>0?3:0);
            return Sprite("yamil-"+(direction*4+Mathf.Clamp(frame,0,3)).ToString("00"));
        }
        public static void Release(){foreach(var sprite in sprites.Values)if(sprite)Object.Destroy(sprite);sprites.Clear();}
    }

    public sealed class CampusAtmosphere : MonoBehaviour
    {
        AudioSource source;AudioClip hum;
        public bool Dark { get; private set; }
        public void SetRoom(int room)
        {
            Dark=room>0;Camera.main.backgroundColor=Dark?new Color(.025f,.035f,.045f):new Color(.1f,.16f,.2f);
            if(!source){source=gameObject.AddComponent<AudioSource>();source.loop=true;source.volume=.25f;
                const int rate=22050;var samples=new float[rate];
                for(int i=0;i<samples.Length;i++)samples[i]=.1f*Mathf.Sin(i*2*Mathf.PI*60/rate)+.03f*Mathf.Sin(i*2*Mathf.PI*121/rate);
                hum=AudioClip.Create("Ambiente fluorescente",rate,1,rate,false);hum.SetData(samples,0);source.clip=hum;}
            if(Dark)source.Play();else source.Stop();
        }
        void OnDestroy(){if(hum)Destroy(hum);CampusLook.Release();}
    }
}
