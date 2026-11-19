using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace Backfranz.Playable
{
    public static class CampusLook
    {
        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        static readonly Dictionary<string,string> people=new Dictionary<string,string>();
        public static string Person(string name,string fallback)
        {
            if(fallback!="pb-estudiante"&&fallback!="pb-recepcionista"&&fallback!="pb-guardia")return fallback;
            if(people.TryGetValue(name,out var known))return known??fallback;
            string asset="npc-"+AssetKey(name);
            string selected=Resources.Load<Texture2D>("Backfranz/Art/"+asset)?asset:
                Resources.Load<Texture2D>("Backfranz/Art/"+asset+"-00")?asset+"-00":null;
            people.Add(name,selected);
            return selected??fallback;
        }
        internal static string AssetKey(string name)
        {
            var key=new StringBuilder();
            foreach(char letter in name.ToLowerInvariant().Normalize(NormalizationForm.FormD))
                if(CharUnicodeInfo.GetUnicodeCategory(letter)!=UnicodeCategory.NonSpacingMark)
                    key.Append(char.IsWhiteSpace(letter)?'-':letter);
            return key.ToString();
        }
        public static Sprite PersonPose(string name,string fallback,Vector2 facing,int frame)
        {
            string asset=Person(name,fallback);
            return asset.StartsWith("npc-",System.StringComparison.Ordinal)&&
                asset.EndsWith("-00",System.StringComparison.Ordinal)?
                Student(facing,frame,asset.Substring(0,asset.Length-3)):Sprite(asset);
        }
        public static Sprite Sprite(string name)
        {
            if (sprites.TryGetValue(name, out var cached) && cached) return cached;
            var texture = Resources.Load<Texture2D>("Backfranz/Art/" + name);
            if (!texture) throw new System.InvalidOperationException("Falta exportación del asset ASE: " + name);
            texture.filterMode = FilterMode.Point;
            int pixelsPerUnit = name.StartsWith("tile16-") ? 16 : 32;
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),pixelsPerUnit,0,SpriteMeshType.FullRect,
                name=="panel" || name.StartsWith("button") ? new Vector4(8,8,8,8) : Vector4.zero);
            sprite.name=name;
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
        public static Sprite Student(Vector2 facing,int frame,string character="yamil")
        {
            int direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x<0?1:2):(facing.y>0?3:0);
            return Sprite(character+"-"+(direction*4+Mathf.Clamp(frame,0,3)).ToString("00"));
        }
        public static void Release(){foreach(var sprite in sprites.Values)if(sprite)Object.Destroy(sprite);sprites.Clear();people.Clear();}
    }

    public sealed class CampusAtmosphere : MonoBehaviour
    {
        const float RoomVolume = .76f;
        readonly AudioSource[] channels = new AudioSource[2];
        readonly System.Random timing = new System.Random();
        static readonly string[] DarkDetails = {"terror-paso-lejano","terror-raspado","terror-interferencia","golpe-tras-muro"};
        static readonly string[] ServiceDetails = {"terror-gota","terror-raspado","golpe-tras-muro"};
        static readonly string[] CafeDetails = {"cafe-taza-1","cafe-taza-2"};
        AudioSource detail;
        float detailDelay;
        int lastDetail = -1;
        int audible = -1;
        string roomTone;
        string[] details;
        public bool Quiet { get; set; }
        public bool Dark { get; private set; }

        public void SetRoom(int room, AudioMixerGroup ambientGroup)
        {
            Dark=CampusWorld.IsDark(room);
            Camera.main.backgroundColor=Dark?new Color(.025f,.035f,.045f):new Color(.1f,.16f,.2f);
            string next = ToneForRoom(room);
            if(next==roomTone)return;
            var clip=Resources.Load<AudioClip>(BackfranzAudio.ClipFolder+"ambiente-"+next);
            if(!clip)
            {
                Debug.LogWarning("Falta el ambiente de Backfranz: "+next,this);
                return;
            }
            if(!channels[0])
            {
                for(int i=0;i<channels.Length;i++) channels[i]=CreateTrack(ambientGroup,true);
                detail=CreateTrack(ambientGroup,false);
            }
            roomTone=next;
            detail.Stop();
            details = next=="cafe" ? CafeDetails :
                next=="mantenimiento" || next=="galeria" ? ServiceDetails : Dark ? DarkDetails : null;
            ResetDetailDelay();
            lastDetail=-1;

            int matching=System.Array.FindIndex(channels,source=>source.clip==clip&&source.isPlaying);
            if(matching>=0)
            {
                audible=matching;
                return;
            }
            audible=channels[0].volume<=channels[1].volume?0:1;
            var incoming=channels[audible];
            incoming.Stop();
            incoming.clip=clip;
            incoming.volume=0;
            incoming.Play();
        }

        internal static string ToneForRoom(int room)
        {
            if(room==CampusWorld.Entrance)return "landaeta";
            if(room==CampusWorld.Basement)return "mantenimiento";
            if(room==CampusWorld.ServiceGallery)return "galeria";
            bool dark=CampusWorld.IsDark(room);
            if(CampusWorld.IsLiftCabin(room))return dark?"cabina-alterada":"cabina";
            if(CampusWorld.IsStairwell(room))return "gradas";
            if(CampusWorld.IsUpperInterior(room))
            {
                string surface=CampusWorld.UpperInteriorSurface(room);
                if(surface=="bano" || surface=="cocina" || surface=="laboratorio")
                    return dark?"servicio-alterado":"servicio";
                if(surface=="computo" || CampusWorld.UpperInteriorName(room)=="Sala Mac")
                    return dark?"electricidad-alterada":"computo";
                if(surface=="acustico" || CampusWorld.UpperInteriorFloor(room)==6)
                    return dark?"silencio-alterado":"silencio";
                return dark?"backfranz":"aula";
            }
            if(CampusWorld.IsInterior(room))
            {
                return CampusWorld.InteriorIndex(room) switch
                {
                    3 => dark?"silencio-alterado":"auditorio",
                    4 => dark?"electricidad-alterada":"juegos",
                    5 => dark?"servicio-alterado":"cafe",
                    _ => dark?"backfranz":"oficinas"
                };
            }
            return room==CampusWorld.Top?"silencio":dark?"backfranz":"campus";
        }

        AudioSource CreateTrack(AudioMixerGroup group,bool loop)
        {
            var source=gameObject.AddComponent<AudioSource>();
            source.playOnAwake=false;
            source.loop=loop;
            source.volume=0;
            source.outputAudioMixerGroup=group;
            return source;
        }

        void ResetDetailDelay() => detailDelay = 9f+(float)timing.NextDouble()*11f;

        void Update()
        {
            if(audible<0)return;
            float volume=Quiet?RoomVolume*.36f:RoomVolume;
            for(int i=0;i<channels.Length;i++)
            {
                var source=channels[i];
                source.volume=Mathf.MoveTowards(source.volume,i==audible?volume:0,Time.unscaledDeltaTime*.85f);
                if(i!=audible&&source.volume<=0&&source.isPlaying)source.Stop();
            }
            detail.volume=Mathf.MoveTowards(detail.volume,Quiet?0:.58f,Time.unscaledDeltaTime*1.5f);
            if(Quiet||details==null||Time.timeScale==0)return;
            detailDelay-=Time.deltaTime;
            if(detailDelay>0)return;
            int next=timing.Next(lastDetail<0?details.Length:details.Length-1);
            if(lastDetail>=0&&next>=lastDetail)next++;
            lastDetail=next;
            detail.clip=Resources.Load<AudioClip>(BackfranzAudio.ClipFolder+details[next]);
            detail.panStereo=(float)(timing.NextDouble()-.5)*1.32f;
            detail.Play();
            ResetDetailDelay();
        }

        void OnDestroy()=>CampusLook.Release();
    }
}
