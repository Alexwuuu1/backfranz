using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        static readonly Vector2Int[] WallDirections={Vector2Int.left,Vector2Int.right,Vector2Int.down,Vector2Int.up};
        readonly Dictionary<(int joins,string material,bool dark,bool low),Sprite> wallSprites=new();

        void PlaceJoinedWall(int room,Vector2Int cell,HashSet<Vector2Int> walls,string material,bool low=false)
        {
            int joins=0;
            for(int i=0;i<WallDirections.Length;i++)
                if(walls.Contains(cell+WallDirections[i]))joins|=1<<i;

            bool dark=IsDark(room);
            var key=(joins,material,dark,low);
            if(!wallSprites.TryGetValue(key,out var sprite)){
                sprite=DrawJoinedWall(joins,material,dark,low);
                wallSprites.Add(key,sprite);
            }
            var wall=new GameObject("Muro continuo");
            wall.transform.SetParent(rooms[room].Root.transform,false);
            var position=Cell(room,cell.x,cell.y);
            wall.transform.position=position;
            var renderer=wall.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite;
            renderer.sortingOrder=CampusLook.Depth(position.y)-(low?14:0);
        }

        static Sprite DrawJoinedWall(int joins,string material,bool dark,bool low)
        {
            const int size=32;
            bool glass=material.Contains("vidrio");
            bool wood=material.Contains("panel")||material.Contains("estudio")||material.Contains("acustico");
            var face=glass?new Color(.32f,.53f,.62f):wood?new Color(.56f,.43f,.34f):new Color(.73f,.70f,.62f);
            var cap=glass?new Color(.68f,.83f,.85f):wood?new Color(.76f,.61f,.46f):new Color(.91f,.86f,.72f);
            var trim=new Color(.24f,.29f,.33f);
            if(dark){face*=new Color(.48f,.51f,.61f,1);cap*=new Color(.51f,.54f,.65f,1);trim*=.67f;trim.a=1;}
            float DistanceToWall(Vector2 point)
            {
                float closest=point.sqrMagnitude;
                for(int i=0;i<WallDirections.Length;i++){
                    if((joins&(1<<i))==0)continue;
                    Vector2 direction=WallDirections[i];
                    float along=Mathf.Max(0,Vector2.Dot(point,direction));
                    closest=Mathf.Min(closest,(point-direction*along).sqrMagnitude);
                }
                return Mathf.Sqrt(closest);
            }

            int faceHeight=low?3:10;
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                var point=new Vector2(x-15.5f,y-15.5f);
                float distance=DistanceToWall(point);
                float front=DistanceToWall(point+Vector2.up*faceHeight);
                if(distance<12f){
                    pixels[y*size+x]=distance>10.5f?trim:Color.Lerp(cap,face,.22f);
                }else if(point.y<0&&front<12f){
                    bool seam=glass&&(x==3||x==28)||wood&&x%8==0;
                    pixels[y*size+x]=front>10.5f||seam?trim:face;
                }
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels32(pixels);
            texture.Apply(false,true);
            return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),size,0,SpriteMeshType.FullRect);
        }
    }
}
