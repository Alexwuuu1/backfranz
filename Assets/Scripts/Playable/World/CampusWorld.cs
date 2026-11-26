using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Backfranz.Playable
{
    public enum SurfaceSound { Baldosa, Adoquin, Madera, Metal, Alfombra }

    // Both phases use the same floor templates, including walls and landmarks.
    public sealed partial class CampusWorld : MonoBehaviour
    {
        public const int Entrance=0, Ground=1, Top=7, Basement=8, DarkGround=9, DarkTop=15;
        public const int FirstStairwell=30, StairwellCount=2;
        const int RoomSpacing=96;
        public static readonly string[] Floors={"PB · Recepción","1 · Servicios","2 · Salud y cocina","3 · Computación","4 · Aulas","5 · Taller","6 · Biblioteca"};
        static readonly string[] floorMaterials={"terrazo","oficina","laboratorio","computo","linoleo","taller","roble"};
        static readonly string[] floorSigns={"Recepción","Cajas","Simulación","Sistemas","Aulas","Taller","Biblioteca"};
        public readonly struct Stop
        {
            public readonly Vector2 Position;
            public readonly string Label;
            public Stop(Vector2 position,string label){Position=position;Label=label;}
        }
        sealed class RoomData
        {
            public GameObject Root;
            public readonly List<Rect> Obstacles=new List<Rect>();
            public readonly List<Stop> Stops=new List<Stop>();
            public readonly Dictionary<Vector2,FramedDoor> Doors=new Dictionary<Vector2,FramedDoor>();
            public FramedDoor Lift;
            public HashSet<Vector2Int> Cells;
            public Tilemap Surface;
        }
        readonly RoomData[] rooms=new RoomData[TotalRooms];
        readonly Dictionary<string,Tile> floorTiles=new Dictionary<string,Tile>();
        readonly GameObject[] breakers=new GameObject[3];
        GameObject fuseView,keyView,badgeView,powerView;
        internal bool ShowLegacyPuzzleProps {get;set;}
        public bool Power {get;private set;}
        public static bool IsDark(int room)=>room==ServiceGallery||room==LiftCabin(true)||IsDarkUpperInterior(room)||room>=Basement&&room<FirstStairwell&&(room<FirstInterior||room>=FirstDarkInterior);
        public static int Floor(int room)=>room==ServiceGallery?-1:IsUpperInterior(room)?UpperInteriorFloor(room):IsInterior(room)?0:room>=DarkGround?room-DarkGround:room-Ground;
        public static int ForFloor(int floor,bool dark)=>floor+(dark?DarkGround:Ground);
        public static bool IsStairwell(int room)=>room>=FirstStairwell&&room<FirstUpperInterior;
        public static string StairName(int core)=>core==0?"Gradas centrales":"Gradas del auditorio";
        public static string Title(int room)=>room==ServiceGallery?"Galería de servicio":IsLiftCabin(room)?"Cabina":IsUpperInterior(room)?UpperInteriorName(room):IsStairwell(room)?StairName(room-FirstStairwell):IsInterior(room)?GroundDoors[InteriorIndex(room)].Name:room==Entrance?"Acceso exterior":room==Basement?"-1 · Subsuelo":IsGroundHall(room)?"PB · Pasillo":Floors[Floor(room)];
        public Vector2 Offset(int room)=>new Vector2(room*RoomSpacing,0);
        public Vector2 Cell(int room,int x,int y)=>Offset(room)+new Vector2(x-16,y-10);
        public Vector2 Entry(int room)=>room==ServiceGallery?Cell(room,4,5):IsUpperInterior(room)?UpperInteriorExit(room)+Vector2.up:room==Entrance?Cell(room,4,5):IsGroundHall(room)?Cell(room,58,4):IsInterior(room)?Cell(room,11,4):Cell(room,room==Basement?4:3,3);
        public Vector2 Lift(int room)=>Cell(room,30,IsGroundHall(room)?21:18);
        public Vector2 Exit(int room)=>IsGroundHall(room)?Cell(room,58,2):Cell(room,2,5);
        public Vector2 MainStop(int room)=>IsInterior(room)?Cell(room,11,11):Cell(room,15,15);
        public Vector2 Fuse=>Cell(Basement,8,15);
        public Vector2 PowerPanel=>Cell(Basement,12,7);
        public Vector2 Key=>Cell(ForFloor(1,true),12,15);
        public Vector2 Badge=>Cell(ForFloor(5,true),15,15);
        public Vector2 Breaker(int index)=>Cell(ForFloor(3,true),8+index*7,14);
        public IReadOnlyList<Stop> Stops(int room)=>rooms[room].Stops;
        public void Show(int room)
        {
            if(rooms[room]==null)Build(room);
            for(int i=0;i<rooms.Length;i++)if(rooms[i]!=null)rooms[i].Root.SetActive(i==room);
        }
        static bool TryDarkVariant(string asset)=>asset.StartsWith("mob-")||asset.StartsWith("escena-")||asset.StartsWith("pb-")||asset.StartsWith("tour-")||asset.StartsWith("campus-suelo-")||asset.StartsWith("campus-muro-")||asset.StartsWith("campus-prop-")||asset.StartsWith("campus-arq-")||asset.StartsWith("campus-puerta-")||(asset.StartsWith("bloque-a-")&&!asset.StartsWith("bloque-a-item-"))||asset=="bookshelf"||asset=="desk"||asset=="chair"||asset=="plant"||asset=="elevator"||asset=="lamp"||asset=="lockers";
        string Style(int room,string asset)=>IsDark(room)&&(asset.StartsWith("utileria-")||TryDarkVariant(asset))?"backfranz-"+asset:asset;
        void Build(int room)
        {
            var data=new RoomData{Root=new GameObject(Title(room))};rooms[room]=data;
            data.Root.transform.SetParent(transform,false);data.Root.SetActive(false);
            if(room==Entrance){BuildStreet();return;}
            if(room==ServiceGallery){BuildServiceGallery();return;}
            if(IsLiftCabin(room)){BuildCabin(room);return;}
            if(IsStairwell(room)){BuildStairwell(room);return;}
            if(IsUpperInterior(room)){BuildUpperInterior(room);return;}
            if(IsGroundHall(room)){BuildGroundHall(room);return;}
            if(IsInterior(room)){BuildGroundInterior(room);return;}
            int floor=Floor(room);var walls=new HashSet<Vector2Int>();
            for(int x=0;x<32;x++){walls.Add(new Vector2Int(x,0));walls.Add(new Vector2Int(x,19));}
            for(int y=0;y<20;y++){walls.Add(new Vector2Int(0,y));walls.Add(new Vector2Int(31,y));}
            if(room==Basement)AddMaintenanceWalls(walls);
            else{
                AddFloorWalls(walls,floor);
                if(IsDark(room))AddDarkHallWalls(walls,floor);
            }
            data.Cells=walls;
            FloorTiles(room);
            var liftOpening=WallOpening(room,Lift(room),false);
            if(floor>=1&&floor<=5)AddUpperDoorOpenings(room,liftOpening);
            foreach(var cell in walls){
                Vector2 p=Cell(room,cell.x,cell.y);data.Obstacles.Add(new Rect(p-Vector2.one*.5f,Vector2.one));
                if(liftOpening.Contains(cell)||room==DarkTop&&LibraryArchiveOpening(cell))continue;
                if(room==Basement&&MaintenanceVoid(cell)&&MaintenanceVoid(cell+Vector2Int.up)&&
                    MaintenanceVoid(cell+Vector2Int.left)&&MaintenanceVoid(cell+Vector2Int.right))continue;
                if(floor==1&&ServicesVoid(cell)&&ServicesVoid(cell+Vector2Int.up)&&
                    ServicesVoid(cell+Vector2Int.left)&&ServicesVoid(cell+Vector2Int.right))continue;
                if(floor==2&&HealthVoid(cell)&&HealthVoid(cell+Vector2Int.up)&&
                    HealthVoid(cell+Vector2Int.left)&&HealthVoid(cell+Vector2Int.right))continue;
                if(floor==3&&ComputingVoid(cell)&&ComputingVoid(cell+Vector2Int.up)&&
                    ComputingVoid(cell+Vector2Int.left)&&ComputingVoid(cell+Vector2Int.right))continue;
                if(floor==4&&ClassroomsVoid(cell)&&ClassroomsVoid(cell+Vector2Int.up)&&
                    ClassroomsVoid(cell+Vector2Int.left)&&ClassroomsVoid(cell+Vector2Int.right))continue;
                if(floor==5&&WorkshopVoid(cell)&&WorkshopVoid(cell+Vector2Int.up)&&
                    WorkshopVoid(cell+Vector2Int.left)&&WorkshopVoid(cell+Vector2Int.right))continue;
                if(floor==6&&LibraryVoid(cell)&&LibraryVoid(cell+Vector2Int.up)&&
                    LibraryVoid(cell+Vector2Int.left)&&LibraryVoid(cell+Vector2Int.right))continue;
                if(room>=Ground&&room<=Top&&cell.y==19&&(cell.x>=1&&cell.x<=3||cell.x>=26&&cell.x<=28))continue;
                PlaceJoinedWall(room,cell,walls,HallWall(room,cell),cell.y==0);
            }
            PlaceLift(room);
            if(room>=Ground&&room<=Top)
                for(int core=0;core<StairwellCount;core++){
                    var stair=Stairs(room,core);
                    PlaceWallStair(room,stair,StairName(core));
                }
            if(room==Basement){
                DecorateBasement(room);
                fuseView=PlaceCollectible(room,Cell(room,8,16),.84f,"Fusible","bloque-a-item-fusible");
                powerView=PlaceServiceEquipment(room,PowerPanel,"Cuadro del ascensor","bloque-a-item-tablero-apagado",
                    new Vector2(1.55f,1.77f)).gameObject;
                return;
            }
            if(floor==0)Portal(room,"tour-entrada",Exit(room));
            Decorate(room,floor);
            BuildUpperDoors(room);
            PopulateUpper(room,floor);
            if(floor<1)MountPlaque(room,floorSigns[floor],Cell(room,15,19)+Vector2.down*.12f,.25f,CampusLook.Depth(9)+6);
            if(!IsDark(room)){
                if(floor!=6)MarkInductionPosition(room,MainStop(room),"Punto de inducción");
            }else{
                if(floor==1)
                {
                    keyView=Item(room,"bloque-a-item-llave-tecnica",Key,"Llave técnica");
                    keyView.SetActive(ShowLegacyPuzzleProps);
                }
                if(floor==3)
                    for(int i=0;i<breakers.Length;i++)
                    {
                        breakers[i]=Item(room,"bloque-a-item-luz-"+Colors[i],Breaker(i),"Interruptor",1.15f);
                        breakers[i].SetActive(ShowLegacyPuzzleProps);
                    }
                if(floor==5){
                    badgeView=Item(room,"bloque-a-vitrina-codigo",Badge,"Vitrina",1.25f);
                    badgeView.SetActive(ShowLegacyPuzzleProps);
                }
            }
        }
        static string HallWall(int room,Vector2Int cell)
        {
            if(IsGroundHall(room))return cell.x>=54||cell.y==12&&cell.x>=24&&cell.x<=30?"pb-muro-vidrio":"pb-muro-yeso";
            int floor=Floor(room);
            if(room==Basement)return "escena-muro-servicio";
            if(floor==1)return cell.y==19||cell.y==11?"tour-muro-vidrio":"campus-muro-panel";
            if(floor==2)return cell.x<=10&&cell.y<=12?"campus-muro-azulejo":"tour-muro-clinica";
            if(floor<=1&&cell.y>=9&&cell.y<=11)return "tour-muro-vidrio";
            if(floor==3)return cell.y<=12?"tour-muro-estudio":"tour-muro-blanco";
            if(floor==4)return cell.x>=23&&cell.y>=11?"tour-muro-vidrio":"tour-muro-blanco";
            if(floor==5)return cell.x>=19&&cell.y<=12?"escena-muro-servicio":"campus-muro-panel";
            if(floor==6)return cell.y<=11?"campus-muro-panel":"tour-muro-vidrio";
            return "tour-muro-blanco";
        }

        static void AddFloorWalls(HashSet<Vector2Int> walls,int floor)
        {
            void Horizontal(int y,int from,int to,params int[] openings)
            {for(int x=from;x<=to;x++)if(System.Array.IndexOf(openings,x)<0)walls.Add(new Vector2Int(x,y));}
            void Vertical(int x,int from,int to,params int[] openings)
            {for(int y=from;y<=to;y++)if(System.Array.IndexOf(openings,y)<0)walls.Add(new Vector2Int(x,y));}
            switch(floor){
                case 0: Horizontal(10,3,26,7,8,9,15,16,22,23);Vertical(19,12,18,14,15,16);break;
                case 1: AddServicesWalls(walls);break;
                case 2: AddHealthWalls(walls);break;
                case 3: AddComputingWalls(walls);break;
                case 4: AddClassroomsWalls(walls);break;
                case 5: AddWorkshopWalls(walls);break;
                case 6: AddLibraryWalls(walls);break;
            }
        }
        public static readonly string[] Colors={"azul","roja","verde"};

        void Decorate(int room,int floor)
        {
            void Place(string asset,int x,int y,float width=1.8f,float height=1.7f,bool solid=true)=>Prop(room,asset,x,y,width,height,solid);
            void Mark(string label,int x,int y){rooms[room].Stops.Add(new Stop(Cell(room,x,y),label));}
            switch(floor){
                case 0:
                    Place("tour-mostrador-blanco",15,17,3.7f,2);Place("tour-jardinera",11,17,3,2);Place("campus-prop-barra-cafeteria",23,17,3,2);Place("campus-prop-mesa-cafeteria",24,13);Place("campus-prop-mesa-cafeteria",21,13);
                    Place("tour-sofa-juego",4,17,2.25f,1.7f);Place("bloque-a-arcade",7,17,1.5f,2);Place("tour-puerta-vidrio",5,13,1.6f,2,false);
                    Place("tour-listones",18,7,4.5f,2.25f);Place("tour-banca-metal",10,6,3.4f,1.35f);Place("tour-mostrador-blanco",23,5,3.4f,2);
                    Place("tour-sillon-odontologia",26,6,1.75f,1.6f);Place("tour-atril-auditorio",4,8,1.65f,1.4f);Place("tour-marca-piso",7,11,1,1,false);
                    MountPlaque(room,"Game Room",Cell(room,5,19),.23f,CampusLook.Depth(9)+6);
                    Mark("Game Room · sala de recreación",6,15);Mark("Cafetería",23,15);Mark("Admisiones y servicios estudiantiles",20,7);break;
                case 1: DecorateServicesHall(room);break;
                case 2: DecorateHealthHall(room);break;
                case 3: DecorateComputingHall(room);break;
                case 4: DecorateClassroomsHall(room);break;
                case 5: DecorateWorkshopHall(room);break;
                case 6: DecorateLibraryHall(room);return;
            }
            foreach(int x in new[]{4,12,20,27}){
                if(IsDark(room))Place("bloque-a-alarma",x,18,.7f,1.3f,false);
                var light=Art(room,"lightpool",Cell(room,x,14),new Vector2(6,6),"Luz",-900);
                if(IsDark(room))light.GetComponent<SpriteRenderer>().color=new Color(.9f,.27f,.25f,.6f);
            }
            if(floor==3)
                for(int x=6;x<=26;x+=5)
                    Art(room,"tour-linea-led",Cell(room,x,8),new Vector2(5,.7f),"Reflejo de luz lineal",-910);
        }
        GameObject Art(int room,string asset,Vector2 point,Vector2 size,string name,int? order=null)=>
            CampusLook.Art(rooms[room].Root.transform,Style(room,asset),point,size,name,order);
        internal GameObject PlaceCollectible(int room,Vector2 support,float height,string name,string sprite)=>
            Art(room,sprite,support+Vector2.up*height,new Vector2(.64f,.57f),name,CampusLook.Depth(support.y)+4);
        public GameObject MarkEscapeContact(int room,Vector2 contact,string name,string sprite)
        {
            var point=contact+Vector2.up*.62f;
            string portrait=CampusLook.Person(name,sprite);
            var size=portrait.StartsWith("npc-")?new Vector2(.79f,1.17f):new Vector2(.74f,.88f);
            var mark=CampusLook.Art(rooms[room].Root.transform,portrait,point,size,name,
                CampusLook.Depth(contact.y)-4);
            mark.GetComponent<SpriteRenderer>().color=new Color(.74f,.84f,.88f);
            return mark;
        }
        void Portal(int room,string asset,Vector2 point)=>Art(room,asset,point+Vector2.up*.45f,new Vector2(1.6f,2),"Conexión",CampusLook.Depth(point.y));
        GameObject Item(int room,string asset,Vector2 point,string label,float scale=.8f)=>Art(room,asset,point,new Vector2(scale,scale),label,CampusLook.Depth(point.y)+1);
        void Prop(int room,string asset,int x,int y,float width,float height,bool solid=true)
        {
            Vector2 point=Cell(room,x,y);Art(room,asset,point+Vector2.up*(height/2-.35f),new Vector2(width,height),asset,CampusLook.Depth(point.y));
            if(solid)rooms[room].Obstacles.Add(new Rect(point-new Vector2(width*.36f,.3f),new Vector2(width*.72f,.6f)));
        }
        Tilemap CreateSurfaceMap(int room)
        {
            var grid=new GameObject("Suelo",typeof(Grid));
            grid.transform.SetParent(rooms[room].Root.transform);
            grid.transform.position=Cell(room,0,0)-Vector2.one*.5f;
            var tiles=new GameObject("Baldosas",typeof(Tilemap),typeof(TilemapRenderer));
            tiles.transform.SetParent(grid.transform,false);
            tiles.GetComponent<TilemapRenderer>().sortingOrder=-1000;
            var map=tiles.GetComponent<Tilemap>();
            rooms[room].Surface=map;
            return map;
        }

        public SurfaceSound Footing(int room,Vector2 position)
        {
            var map=rooms[room].Surface;
            var sprite=map?map.GetSprite(map.WorldToCell(position)):null;
            if(!sprite)return SurfaceSound.Baldosa;
            string material=sprite.name;
            if(material.EndsWith("-metal")||material.EndsWith("-rejilla"))return SurfaceSound.Metal;
            if(material.EndsWith("-roble")||material.EndsWith("-madera"))return SurfaceSound.Madera;
            if(material.EndsWith("-acustico")||material.EndsWith("-alfombra")||
                material.EndsWith("-auditorio")||material.EndsWith("-juegos"))return SurfaceSound.Alfombra;
            return material.EndsWith("-adoquin")||material.EndsWith("-vereda")?SurfaceSound.Adoquin:SurfaceSound.Baldosa;
        }

        static void PaintSurface(Tilemap map,int x,int y,Tile tile)
        {
            var position=new Vector3Int(x,y,0);
            map.SetTile(position,tile);
            float shade=Mathf.Lerp(.92f,1f,Mathf.PerlinNoise(x*.47f,y*.53f));
            map.SetTileFlags(position,TileFlags.None);
            map.SetColor(position,new Color(shade,shade,shade));
        }

        void FloorTiles(int room)
        {
            var map=CreateSurfaceMap(room);
            bool basement=room==Basement;
            string material=basement?"subsuelo":floorMaterials[Floor(room)];
            var baseTile=TileFor(room,"escena-suelo-"+material);
            int floor=Floor(room);
            string aisleMaterial=basement?"metal":floor switch{2=>"linoleo",3=>"acustico",4=>"roble",5=>"metal",6=>"auditorio",_=>"terrazo"};
            var aisle=TileFor(room,"escena-suelo-"+aisleMaterial);
            var drain=basement?TileFor(room,"escena-suelo-rejilla"):null;
            var darkAisle=IsDark(room)&&!basement?TileFor(room,"escena-suelo-rejilla"):null;
            var kitchen=floor==2?TileFor(room,"escena-suelo-cocina"):null;
            for(int x=0;x<32;x++)
                for(int y=0;y<20;y++)
                {
                    if(basement&&MaintenanceVoid(new Vector2Int(x,y)))continue;
                    if(floor==1&&ServicesVoid(new Vector2Int(x,y)))continue;
                    if(floor==2&&HealthVoid(new Vector2Int(x,y)))continue;
                    if(floor==3&&ComputingVoid(new Vector2Int(x,y)))continue;
                    if(floor==4&&ClassroomsVoid(new Vector2Int(x,y)))continue;
                    if(floor==5&&WorkshopVoid(new Vector2Int(x,y)))continue;
                    if(floor==6&&LibraryVoid(new Vector2Int(x,y)))continue;
                    var tile=basement
                        ? MaintenanceSurface(x,y) switch{"rejilla"=>drain,"metal"=>aisle,_=>baseTile}
                        : (darkAisle!=null&&DarkHallAisle(floor,x,y)?darkAisle:
                            kitchen!=null&&x<=10&&y<=11?kitchen:HallAisle(floor,x,y)?aisle:baseTile);
                    PaintSurface(map,x,y,tile);
                }
        }

        static bool HallAisle(int floor,int x,int y)=>floor switch
        {
            1 => y>=14&&y<=16 || y>=8&&y<=9 || (x>=4&&x<=6||x>=26&&x<=28)&&y>=9&&y<=14,
            2 => y>=14&&y<=16 || y>=8&&y<=10&&x>=11 || (x>=11&&x<=14||x>=26&&x<=28)&&y>=10&&y<=14,
            3 => y>=14&&y<=16 || y>=7&&y<=8 || x>=16&&x<=18&&y>=8&&y<=14,
            4 => y>=14&&y<=16 || y>=8&&y<=9 || x>=14&&x<=17&&y>=8&&y<=14,
            5 => y>=13&&y<=16 || y>=8&&y<=9&&x>=15 || x>=24&&x<=27&&y>=8&&y<=13,
            6 => y>=13&&y<=15 || x>=13&&x<=17&&y>=7&&y<=13 || x>=21&&x<=23&&y>=4&&y<=8,
            _ => false
        };
        Tile TileFor(int room,string asset)
        {
            string styled="tile16-"+Style(room,asset);if(floorTiles.TryGetValue(styled,out var tile))return tile;
            tile=ScriptableObject.CreateInstance<Tile>();tile.sprite=CampusLook.Sprite(styled);floorTiles.Add(styled,tile);return tile;
        }
        public void TakeFuse()=>fuseView.SetActive(false);
        public void TakeKey()=>keyView.SetActive(false);
        public void TakeBadge()=>badgeView.SetActive(false);
        public void RestoreAccess(int lanes,bool power,bool fuse,bool key,bool badge)
        {
            if(lanes!=0){
                Show(Ground);
                for(int lane=0;lane<AccessLaneCount;lane++)
                    if((lanes&(1<<lane))!=0)ShowSecurityOpening(lane,1f);
            }
            if(power||fuse){
                Show(Basement);
                if(fuse)TakeFuse();
                if(power)RestorePower();
            }
            if(key){Show(ForFloor(1,true));TakeKey();}
            if(badge){Show(ForFloor(5,true));TakeBadge();}
        }
        public void RestorePower()
        {
            Power=true;
            RefreshMaintenancePower(true);
            powerView.GetComponent<SpriteRenderer>().sprite=CampusLook.Sprite("bloque-a-item-tablero-activo");
        }
        public void ShowBreakers(int progress,bool solved)
        {
            for(int i=0;i<breakers.Length;i++){
                var renderer=breakers[i].GetComponent<SpriteRenderer>();renderer.color=solved||i==1&&progress>=1||i==0&&progress>=2||i==2&&progress>=3?Color.white:new Color(.73f,.73f,.73f);
            }
        }
        public bool CanStand(Vector2 position,bool includePeople=true)
        {
            int room=RoomAt(position);
            if(room<0||room>=rooms.Length||rooms[room]==null)return false;
            if(!CanStandOnFloor(room,position,includePeople))return false;
            foreach(var obstacle in rooms[room].Obstacles)
                if(Expanded(obstacle).Contains(position))return false;
            return true;
        }

        bool CanStandOnFloor(int room,Vector2 position,bool includePeople)
        {
            var p=position-Offset(room);
            if(room==Entrance||room==ServiceGallery||IsGroundHall(room)||IsInterior(room)||IsStairwell(room)||IsLiftCabin(room)){
                var area=rooms[room].Cells;
                var center=p+new Vector2(16,10);
                int left=Mathf.RoundToInt(center.x-.22f),right=Mathf.RoundToInt(center.x+.22f);
                int bottom=Mathf.RoundToInt(center.y-.22f),top=Mathf.RoundToInt(center.y+.22f);
                if(!area.Contains(new Vector2Int(left,bottom))||!area.Contains(new Vector2Int(left,top))||
                    !area.Contains(new Vector2Int(right,bottom))||!area.Contains(new Vector2Int(right,top)))return false;
                if(IsGroundHall(room)&&(!GroundBendContains(center+new Vector2(-.22f,-.22f))||
                    !GroundBendContains(center+new Vector2(.22f,.22f))||
                    !GroundBendContains(center+new Vector2(-.22f,.22f))||
                    !GroundBendContains(center+new Vector2(.22f,-.22f))))return false;
            }else if(p.x< -15.2f||p.x>14.2f||p.y< -9.2f||p.y>8.2f)return false;
            if(room==Ground&&SecurityBlocks(position))return false;
            if(includePeople&&PersonBlocks(room,position))return false;
            if(room==Basement&&!Power&&MaintenanceGateBlocks(p))return false;
            return true;
        }
        static int RoomAt(Vector2 position)=>Mathf.FloorToInt((position.x+24)/RoomSpacing);
        static Rect Expanded(Rect r)=>new Rect(r.xMin-.22f,r.yMin-.22f,r.width+.44f,r.height+.44f);
        public bool HasLineOfSight(Vector2 a,Vector2 b,bool includePeople=true)
        {
            int room=RoomAt(a);
            if(room<0||room>=rooms.Length||rooms[room]==null||RoomAt(b)!=room)return false;
            var advance=b-a;
            foreach(var obstacle in rooms[room].Obstacles)
            {
                var rect=Expanded(obstacle);
                if(rect.Contains(a)||rect.Contains(b))return false;
                float first=0,last=1;
                if(ClipAxis(a.x,advance.x,rect.xMin,rect.xMax,ref first,ref last)&&
                    ClipAxis(a.y,advance.y,rect.yMin,rect.yMax,ref first,ref last))return false;
            }
            int steps=Mathf.Max(1,Mathf.CeilToInt(advance.magnitude/.12f));
            for(int i=0;i<=steps;i++)
                if(!CanStandOnFloor(room,Vector2.Lerp(a,b,i/(float)steps),includePeople))return false;
            return true;
        }

        static bool ClipAxis(float position,float advance,float min,float max,ref float first,ref float last)
        {
            if(Mathf.Abs(advance)<.00001f) return position>=min&&position<max;
            float low=(min-position)/advance,high=(max-position)/advance;
            first=Mathf.Max(first,Mathf.Min(low,high));
            last=Mathf.Min(last,Mathf.Max(low,high));
            return first<last;
        }
        public List<Vector2> FindRoute(Vector2 from,Vector2 to)
        {
            int room=RoomAt(from);
            var origin=Offset(room);
            var path=new List<Vector2>();
            if(!CanStand(from)||!CanStand(to)||RoomAt(to)!=room)return path;
            if(!RouteCell(from,origin,out var start)||!RouteCell(to,origin,out var end))return path;

            var frontier=new Queue<Vector2Int>();
            var parents=new Dictionary<Vector2Int,Vector2Int>{{start,start}};
            frontier.Enqueue(start);
            var directions=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
            while(frontier.Count>0)
            {
                var cell=frontier.Dequeue();
                if(cell==end)break;
                foreach(var direction in directions)
                {
                    var next=cell+direction;
                    if(parents.ContainsKey(next)||!CanStand(origin+next)||
                        !HasLineOfSight(origin+cell,origin+next))continue;
                    parents[next]=cell;
                    frontier.Enqueue(next);
                }
            }
            if(!parents.ContainsKey(end))return path;
            if(Vector2.Distance(to,origin+end)>.045f)path.Add(to);
            for(var cell=end;cell!=start;cell=parents[cell])path.Add(origin+cell);
            if(Vector2.Distance(from,origin+start)>.045f)path.Add(origin+start);
            path.Reverse();
            return path;
        }

        bool RouteCell(Vector2 point,Vector2 origin,out Vector2Int result)
        {
            var closest=Vector2Int.RoundToInt(point-origin);
            result=closest;
            if(HasLineOfSight(point,origin+closest))return true;
            float distance=float.PositiveInfinity;
            for(int y=-1;y<=1;y++)
                for(int x=-1;x<=1;x++)
                {
                    var candidate=closest+new Vector2Int(x,y);
                    float squared=(origin+candidate-point).sqrMagnitude;
                    if(squared>=distance||!HasLineOfSight(point,origin+candidate))continue;
                    distance=squared;
                    result=candidate;
                }
            return !float.IsInfinity(distance);
        }
        void OnDestroy()
        {
            foreach(var tile in floorTiles.Values)Destroy(tile);
            foreach(var sprite in wallSprites.Values){
                Destroy(sprite.texture);
                Destroy(sprite);
            }
        }
    }
}
