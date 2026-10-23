using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Backfranz.Playable
{
    public sealed class CampusWorld : MonoBehaviour
    {
        public static readonly string[] Names={"Recepción", "Pasillos", "Biblioteca"};
        readonly List<Rect>[] walls={new List<Rect>(),new List<Rect>(),new List<Rect>()};
        readonly List<GameObject> gateViews=new List<GameObject>();
        readonly List<Rect> gates=new List<Rect>();
        readonly List<Tile> floorTiles=new List<Tile>();
        public readonly Vector2[] Notes=new Vector2[3];
        public readonly GameObject[] NoteViews=new GameObject[3];
        public GameObject CardView {get;private set;}
        public GameObject SwitchView {get;private set;}
        public bool GateOpen {get;private set;}
        public Vector2 Offset(int room)=>new Vector2(room*40,0);
        public Vector2 Cell(int room,int x,int y)=>Offset(room)+new Vector2(x-16,y-10);
        public Vector2 Entry(int room)=>Cell(room,2,2);
        public Vector2 Door(int room)=>Cell(room,29,17);
        public Vector2 EnemySpawn(int room)=>Cell(room,room==2?16:28,15);
        public Vector2 Card=>Cell(0,4,15);
        public Vector2 PowerSwitch=>Cell(1,13,15);
        public void Build()
        {
            for(int room=0;room<3;room++){
                var parent=new GameObject(Names[room]).transform;parent.SetParent(transform);
                Floors(room,parent);
                var cells=new HashSet<Vector2Int>();
                for(int x=0;x<32;x++){cells.Add(new Vector2Int(x,0));cells.Add(new Vector2Int(x,19));}
                for(int y=0;y<20;y++){cells.Add(new Vector2Int(0,y));cells.Add(new Vector2Int(31,y));}
                if(room==0){
                    Divider(cells,10,1,18,4,5);Divider(cells,21,1,18,13,14);
                    Horizontal(cells,10,1,9,5,6);Horizontal(cells,9,22,30,26,27);
                }else if(room==1){
                    Divider(cells,8,1,18,14,15);Divider(cells,16,1,18,5,6);Divider(cells,24,1,18,13,14);
                    Horizontal(cells,11,1,7,4,5);Horizontal(cells,11,25,30,27,28);
                }else{
                    Divider(cells,10,1,18,8,9,10);Divider(cells,22,1,18,8,9,10);
                    Horizontal(cells,6,1,30,4,5,15,16,27,28);
                    Horizontal(cells,12,1,30,4,5,15,16,27,28);
                }
                foreach(var cell in cells){
                    Vector2 p=Cell(room,cell.x,cell.y);walls[room].Add(new Rect(p-new Vector2(.5f,.5f),Vector2.one));
                    CampusLook.Art(parent,room==0?"wall-clean":"wall-dark",p+Vector2.up*.25f,new Vector2(1,1.5f),"Muro",CampusLook.Depth(p.y));
                }
                Decorate(room,parent);
                Notes[room]=Cell(room,room==0?15:28,room==0?15:3);
                NoteViews[room]=CampusLook.Art(parent,"note",Notes[room],new Vector2(.7f,.7f),"Apunte");
                CampusLook.Art(parent,room==2?"exit":"elevator",Door(room)+Vector2.up*.45f,new Vector2(1.6f,2),"Salida",CampusLook.Depth(Door(room).y));
                if(room==0)CardView=CampusLook.Art(parent,"card",Card,new Vector2(.7f,.7f),"Tarjeta de acceso");
                if(room==1){
                    SwitchView=CampusLook.Art(parent,"switch",PowerSwitch,new Vector2(.8f,1.2f),"Cuadro eléctrico");
                    foreach(int y in new[]{5,6}){Vector2 p=Cell(1,16,y);gates.Add(new Rect(p-new Vector2(.5f,.5f),Vector2.one));gateViews.Add(CampusLook.Art(parent,"gate",p+Vector2.up*.2f,new Vector2(1,1.5f),"Reja eléctrica"));}
                }
            }
        }
        static void Divider(HashSet<Vector2Int> cells,int x,int from,int to,params int[] gaps)
        {for(int y=from;y<=to;y++)if(System.Array.IndexOf(gaps,y)<0)cells.Add(new Vector2Int(x,y));}
        static void Horizontal(HashSet<Vector2Int> cells,int y,int from,int to,params int[] gaps)
        {for(int x=from;x<=to;x++)if(System.Array.IndexOf(gaps,x)<0)cells.Add(new Vector2Int(x,y));}
        void Floors(int room,Transform parent)
        {
            var grid=new GameObject("Suelos",typeof(Grid));grid.transform.SetParent(parent);grid.transform.position=Offset(room)-new Vector2(.5f,.5f);
            var obj=new GameObject("Tilemap",typeof(Tilemap),typeof(TilemapRenderer));obj.transform.SetParent(grid.transform,false);
            obj.GetComponent<TilemapRenderer>().sortingOrder=-1000;var map=obj.GetComponent<Tilemap>();
            foreach(string name in new[]{room==0?"floor-clean":room==1?"floor-dark":"floor-classroom","floor-carpet"}){
                var tile=ScriptableObject.CreateInstance<Tile>();tile.sprite=CampusLook.Sprite(name);floorTiles.Add(tile);
            }
            for(int x=0;x<32;x++)for(int y=0;y<20;y++)map.SetTile(new Vector3Int(x-16,y-10,0),floorTiles[floorTiles.Count-2+((room==0&&x<10&&y>10)||(room==2&&y>6&&y<12)?1:0)]);
        }
        void Prop(int room,Transform parent,string asset,int x,int y,Vector2 size,bool solid=true)
        {
            Vector2 p=Cell(room,x,y);
            CampusLook.Art(parent,asset,p+Vector2.up*(size.y/2-.35f),size,asset,CampusLook.Depth(p.y));
            if(solid)walls[room].Add(new Rect(p-new Vector2(size.x*.38f,.3f),new Vector2(size.x*.76f,.6f)));
        }
        void Decorate(int room,Transform p)
        {
            if(room==0){
                Prop(room,p,"reception-desk",15,8,new Vector2(3,2));
                Prop(room,p,"computer",14,17,new Vector2(1,1.5f));Prop(room,p,"desk",18,17,new Vector2(1.5f,1.5f));
                Prop(room,p,"chair",18,15,new Vector2(.8f,1.2f));
                Prop(room,p,"bookshelf",2,17,new Vector2(1.5f,2));Prop(room,p,"plant",8,17,new Vector2(1,1.5f));
                Prop(room,p,"plant",2,8,new Vector2(1,1.5f));Prop(room,p,"plant",29,14,new Vector2(1,1.5f));
                Prop(room,p,"vending",29,3,new Vector2(1,2));Prop(room,p,"lockers",24,3,new Vector2(1.5f,2));
                Prop(room,p,"desk",25,6,new Vector2(1.5f,1.5f));Prop(room,p,"chair",27,5,new Vector2(.8f,1.2f));
            }else if(room==1){
                foreach(int y in new[]{3,6,17})Prop(room,p,"lockers",2,y,new Vector2(1.5f,2));
                Prop(room,p,"computer",11,17,new Vector2(1,1.5f));Prop(room,p,"desk",13,9,new Vector2(1.5f,1.5f));
                Prop(room,p,"desk",20,16,new Vector2(1.5f,1.5f));Prop(room,p,"chair",20,14,new Vector2(.8f,1.2f));
                Prop(room,p,"bookshelf",26,8,new Vector2(1.5f,2));Prop(room,p,"plant",30,6,new Vector2(1,1.5f));
                Prop(room,p,"vending",18,2,new Vector2(1,2));
            }else{
                foreach(int x in new[]{2,7,12,20,24})Prop(room,p,"bookshelf",x,17,new Vector2(1.5f,2));
                foreach(int x in new[]{2,7,12,20})Prop(room,p,"bookshelf",x,3,new Vector2(1.5f,2));
                Prop(room,p,"desk",15,14,new Vector2(1.5f,1.5f));Prop(room,p,"computer",18,14,new Vector2(1,1.5f));
                Prop(room,p,"desk",26,15,new Vector2(1.5f,1.5f));Prop(room,p,"chair",26,13,new Vector2(.8f,1.2f));
            }
            foreach(int x in new[]{5,15,27}){
                Prop(room,p,"lamp",x,18,new Vector2(.7f,1.5f),false);
                CampusLook.Art(p,"lightpool",Cell(room,x,16),new Vector2(5,5),"Luz de fluorescente",-900);
            }
        }
        public void RestorePower(){GateOpen=true;foreach(var gate in gateViews)gate.SetActive(false);SwitchView.GetComponent<SpriteRenderer>().sprite=CampusLook.Sprite("switch-on");}
        public bool CanStand(Vector2 position)
        {
            int room=Mathf.RoundToInt(position.x/40);Vector2 p=position-Offset(room);
            if(room<0||room>2||p.x< -15.2f||p.x>14.2f||p.y< -9.2f||p.y>8.2f)return false;
            foreach(var r in walls[room])if(Expanded(r).Contains(position))return false;
            if(room==1&&!GateOpen)foreach(var r in gates)if(Expanded(r).Contains(position))return false;
            return true;
        }
        static Rect Expanded(Rect r)=>new Rect(r.xMin-.22f,r.yMin-.22f,r.width+.44f,r.height+.44f);
        public bool HasLineOfSight(Vector2 a,Vector2 b)
        {int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)/.15f));for(int i=0;i<=steps;i++)if(!CanStand(Vector2.Lerp(a,b,i/(float)steps)))return false;return true;}
        public List<Vector2> FindRoute(Vector2 startPosition,Vector2 goalPosition)
        {
            Vector2 o=Offset(Mathf.RoundToInt(startPosition.x/40));var start=Vector2Int.RoundToInt(startPosition-o);var goal=Vector2Int.RoundToInt(goalPosition-o);
            var frontier=new Queue<Vector2Int>();var parents=new Dictionary<Vector2Int,Vector2Int>();frontier.Enqueue(start);parents[start]=start;
            var directions=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
            while(frontier.Count>0){var cell=frontier.Dequeue();if(cell==goal)break;foreach(var d in directions){var n=cell+d;if(parents.ContainsKey(n)||!CanStand(o+n))continue;parents[n]=cell;frontier.Enqueue(n);}}
            var path=new List<Vector2>();if(!parents.ContainsKey(goal))return path;
            for(var node=goal;node!=start;node=parents[node])path.Add(o+node);path.Reverse();return path;
        }
        void OnDestroy(){foreach(var tile in floorTiles)Destroy(tile);}
    }
}
