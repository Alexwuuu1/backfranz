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
        public static readonly string[] Floors={"PB · Recepción","1 · Servicios","2 · Salud y cocina","3 · Computación","4 · Aulas","5 · Aula dinámica","6 · Biblioteca"};
        static readonly string[] floorMaterials={"terrazo","oficina","laboratorio","computo","linoleo","taller","roble"};
        static readonly string[] floorSigns={"Recepción","Cajas","Simulación","Sistemas","Aulas","Aula dinámica","Biblioteca"};
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
        readonly List<GameObject> gateViews=new List<GameObject>();
        readonly GameObject[] breakers=new GameObject[3];
        GameObject fuseView,keyView,badgeView,powerView;
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
            if(room==Basement){for(int y=1;y<19;y++)if(y!=7&&y!=8&&y!=9)walls.Add(new Vector2Int(17,y));}
            else{
                AddFloorWalls(walls,floor);
                if(IsDark(room))AddDarkHallWalls(walls,floor);
            }
            data.Cells=walls;
            FloorTiles(room);
            var liftOpening=WallOpening(room,Lift(room),false);
            foreach(var cell in walls){
                Vector2 p=Cell(room,cell.x,cell.y);data.Obstacles.Add(new Rect(p-Vector2.one*.5f,Vector2.one));
                if(liftOpening.Contains(cell))continue;
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
                foreach(int y in new[]{7,8,9}){
                    Vector2 p=Cell(room,17,y);gateViews.Add(Art(room,"bloque-a-reja-cerrada",p+Vector2.up*.2f,new Vector2(1,1.5f),"Reja",CampusLook.Depth(p.y)));
                }
                DecorateBasement(room);
                fuseView=Item(room,"bloque-a-item-fusible",Fuse,"Fusible");powerView=Item(room,"bloque-a-item-tablero-apagado",PowerPanel,"Cuadro del ascensor",1.1f);return;
            }
            if(floor==0)Portal(room,"tour-entrada",Exit(room));
            Decorate(room,floor);
            BuildUpperDoors(room);
            PopulateUpper(room,floor);
            MountPlaque(room,floorSigns[floor],Cell(room,15,19)+Vector2.down*.12f,.25f,CampusLook.Depth(9)+6);
            if(!IsDark(room)){
                Item(room,floor==6?"bloque-a-item-guia-biblioteca":"note",MainStop(room),"Punto de inducción",.85f);
            }else{
                DecorateDarkHall(room,floor);
                if(floor==1)keyView=Item(room,"bloque-a-item-llave-tecnica",Key,"Llave técnica");
                if(floor==3)for(int i=0;i<3;i++)breakers[i]=Item(room,"bloque-a-item-luz-"+Colors[i],Breaker(i),"Interruptor",1.15f);
                if(floor==5)badgeView=Item(room,"bloque-a-vitrina-codigo",Badge,"Vitrina",1.25f);
            }
        }
        static string HallWall(int room,Vector2Int cell)
        {
            if(IsGroundHall(room))return cell.x>=54||cell.y==12&&cell.x>=24&&cell.x<=30?"pb-muro-vidrio":"pb-muro-yeso";
            int floor=Floor(room);
            if(room==Basement)return "escena-muro-servicio";
            if(floor==2)return "tour-muro-clinica";
            if(floor<=1&&cell.y>=9&&cell.y<=11||floor==6&&cell.y==19)return "tour-muro-vidrio";
            if(floor==3&&cell.y==4&&cell.x>=18)return "tour-muro-estudio";
            return floor==5&&cell.y==19?"campus-muro-panel":"tour-muro-blanco";
        }

        static void AddFloorWalls(HashSet<Vector2Int> walls,int floor)
        {
            void Horizontal(int y,int from,int to,params int[] openings)
            {for(int x=from;x<=to;x++)if(System.Array.IndexOf(openings,x)<0)walls.Add(new Vector2Int(x,y));}
            void Vertical(int x,int from,int to,params int[] openings)
            {for(int y=from;y<=to;y++)if(System.Array.IndexOf(openings,y)<0)walls.Add(new Vector2Int(x,y));}
            switch(floor){
                case 0: Horizontal(10,3,26,7,8,9,15,16,22,23);Vertical(19,12,18,14,15,16);break;
                // The open waiting hall bends around the records office.
                case 1:
                    Horizontal(11,0,26,4,5,11,12,15,16,21,22);
                    Vertical(17,11,19,13,14,15,16);
                    Vertical(8,0,5,3,4);
                    Horizontal(5,0,8,3,4,5);
                    break;
                // The clinical wing uses parallel corridors and short cross-passages.
                case 2:
                    Horizontal(10,0,29,4,5,10,11,15,16,21,22,27,28);
                    Vertical(8,10,19,13,14,15);Vertical(17,10,19,13,14,15);Vertical(24,10,19,13,14,15);
                    Horizontal(5,9,21,12,13,18,19);
                    break;
                case 3:
                    Horizontal(10,0,29,4,5,10,11,15,16,22,23,27,28);
                    Horizontal(4,8,25,11,12,16,17,21,22);
                    Vertical(8,0,4,2,3);Vertical(25,0,4,2,3);
                    Vertical(8,10,19,13,14,15);Vertical(16,10,19,13,14,15);Vertical(24,10,19,13,14,15);
                    break;
                case 4:
                    Horizontal(11,0,31,5,6,12,13,15,16,22,23,27,28);
                    Vertical(9,11,19,14,15);Vertical(20,11,19,14,15);
                    Horizontal(4,10,20,11,18);Vertical(20,0,4,2,3);
                    break;
                case 5:
                    Horizontal(10,0,29,4,5,11,12,15,16,21,22,27,28);
                    Vertical(10,10,19,13,14,15);
                    Horizontal(6,21,31,21,22,26,27);
                    break;
                case 6:
                    Horizontal(10,0,14,5,6);Horizontal(10,19,31,22,23,28,29);
                    Vertical(10,0,5,3,4);Vertical(21,0,5,3,4);
                    break;
            }
        }
        public static readonly string[] Colors={"azul","roja","verde"};

        void DecorateBasement(int room)
        {
            Prop(room,"bloque-a-tuberia",6,18,7,1.65f,false);
            Prop(room,"bloque-a-tuberia",23,18,6,1.65f,false);
            Prop(room,"escena-panel-herramientas",12,18,3.6f,1.5f,false);
            Prop(room,"bloque-a-rack",3,15,2,2.2f);
            Prop(room,"bloque-a-rack",14,15,1.7f,2);
            Prop(room,"bloque-a-rack",25,14,1.5f,1.9f);
            Prop(room,"bloque-a-charco",21,13,2.8f,1.6f,false);
            Prop(room,"bloque-a-alarma",28,14,1,1.2f,false);
            Prop(room,"campus-prop-extintor",16,6,.65f,1.2f,false);
            MountPlaque(room,"Servicio eléctrico",Cell(room,11,19),.23f,CampusLook.Depth(9)+6);
            MountPlaque(room,"Bombas y presión",Cell(room,24,19),.23f,CampusLook.Depth(9)+6);
            FloorLight(room,8,13,new Vector2(10,8),new Color(.69f,.81f,.75f,.76f));
            FloorLight(room,24,13,new Vector2(8,9),new Color(.77f,.43f,.31f,.70f));
        }

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
                case 1:
                    Place("tour-mostrador-blanco",14,17,3.4f,2);Place("bloque-a-maquina-ticket",10,16,1.4f,2);
                    Place("pb-servicios",5,18,3.2f,1.65f);Place("pb-servicios",21,18,3.2f,1.65f);Place("tour-banca-metal",24,6,3.3f,1.4f);
                    Place("bloque-a-panel-horario",12,17,2,1.5f,false);Place("tour-mostrador-blanco",20,5,3,2);
                    Place("mob-estante-archivo",6,3,1.7f,1.85f);
                    PlaceRoomDetail(room,"carpetas",15,17,1.02f,.66f,.8f);
                    PlaceCampusProp(room,"registro-turno",11,16,1.28f,.94f,1.37f);
                    Place("mob-banca-espera",13,5,3.35f,1.30f);
                    MountPlaque(room,"Arca",Cell(room,20,5)+Vector2.up*.62f,.22f,CampusLook.Depth(-5)+6);
                    foreach(int x in new[]{11,13,15})Place("tour-marca-piso",x,13,1,1,false);
                    Mark("Cajas · consulta de pagos",12,15);Mark("Posgrado",5,14);Mark("Dirección de Carrera",21,14);Mark("Arca",20,7);break;
                case 2:
                    Place("bloque-a-horno",4,18,1.45f,2);Place("bloque-a-nevera",6,18,1.45f,2);
                    MountPlaque(room,"Cocina",Cell(room,5,19),.23f,CampusLook.Depth(9)+6);
                    Place("bloque-a-armario-medico",12,18,1.65f,2.15f);Place("bloque-a-maniqui",14,18,1.45f,1.65f);
                    Place("tour-banca-metal",20,18,3,1.25f);Place("campus-prop-microscopio",27,18,1.7f,1.8f,false);
                    Place("campus-prop-lavamanos",7,6,2,1.45f);Place("campus-prop-extintor",25,6,.7f,1.2f,false);
                    Place("bloque-a-armario-medico",16,2,1.65f,1.9f);
                    PlaceRoomDetail(room,"dispensador",3,18,.65f,.89f,1.07f);
                    PlaceCampusProp(room,"maletin-clinico",20,18,.81f,.62f,.78f);
                    Mark("Cocina de prácticas",5,15);Mark("Sala de Simulación",12,15);Mark("Histología y Fisiología",26,15);break;
                case 3:
                    Place("bloque-a-pc",5,18,1.8f,1.8f);Place("bloque-a-rack",12,18,1.6f,2);
                    Place("campus-prop-tablero-avisos",20,18,3,1.7f,false);
                    Place("tour-monitor-mac",26,17,2,1.85f);
                    MountPlaque(room,"Sala Mac",Cell(room,26,19),.23f,CampusLook.Depth(9)+6);
                    Place("bloque-a-rack",6,5,1.65f,1.95f);Place("tour-banca-metal",14,3,3.5f,1.35f);
                    Place("bloque-a-tripode",20,2,1.38f,1.52f);
                    PlaceRoomDetail(room,"horarios",22,18,1.66f,1.07f,1.33f);
                    PlaceCampusProp(room,"cartel-aire",14,4,1.46f,.92f,.46f);
                    Place("campus-prop-extintor",27,5,.65f,1.1f,false);
                    Mark("Laboratorios PC · salas 1, 2 y 3",5,15);Mark("Sala Mac",26,15);Mark("Streaming, fotografía y sonido",14,7);break;
                case 4:
                    Place("campus-prop-tablero-avisos",5,18,3.2f,1.6f,false);
                    Place("bloque-a-pantalla-gesell",24,18,3.2f,1.9f,false);
                    Place("mob-banca-espera",6,6,3.2f,1.35f);
                    Place("mob-estante-exhibidor",22,6,2.1f,1.65f);
                    PlaceRoomDetail(room,"horarios",13,18,1.7f,1.05f,1.33f);
                    PlaceCampusProp(room,"panel-notas",16,18,1.85f,1.19f,1.38f);
                    Mark("Sala Jurídica",5,15);Mark("Cámara Gesell",26,15);
                    break;
                case 5:
                    Place("escena-panel-herramientas",24,18,4.1f,1.7f,false);
                    Place("bloque-a-pizarra-movil",6,17,2.5f,1.9f,false);
                    Place("mob-mesa-grupo",6,6,3.5f,1.85f);
                    Place("mob-mesa-taller",25,3,3.2f,1.65f);
                    PlaceRoomDetail(room,"placa-electronica",25,3,.93f,.62f,.7f);
                    PlaceCampusProp(room,"caja-componentes",24,3,1.14f,.76f,.71f);
                    Mark("Aula dinámica · mesas de trabajo en grupo",15,15);Mark("Zona de aulas",26,15);
                    break;
                case 6:
                    foreach(int x in new[]{9,21}){
                        Place("tour-ventanal",x,18,5.4f,2.2f,false);
                        Place("tour-escritorio-ventana",x,17,3.6f,1.5f);
                    }
                    foreach(int x in new[]{4,6,24,26})Place(x<10?"mob-estante-roble":"mob-estante-acero",x,18,1.85f,2);
                    foreach(int x in new[]{6,23})Place(x==6?"mob-mesa-lectura-lampara":"mob-mesa-lectura-doble",x,5,2.6f,1.75f);
                    Place("tour-mostrador-blanco",15,17,3.7f,2.2f);
                    Place("mob-carro-libros",4,16,1.3f,1.65f);
                    Place("mob-estante-bajo",3,7,2.65f,1.55f);
                    PlaceRoomDetail(room,"lector",16,17,.87f,.55f,.83f);
                    PlaceCampusProp(room,"separalibros",5,18,.91f,.72f,1.22f);
                    Mark("Consulta y préstamo de material",15,15);Mark("Sala de lectura",17,8);break;
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
            for(int x=0;x<32;x++)
                for(int y=0;y<20;y++)
                {
                    bool serviceWalkway=y>=7&&y<=9&&x>=11;
                    var tile=basement
                        ? (x==21&&y>9?drain:serviceWalkway?aisle:baseTile)
                        : (darkAisle!=null&&DarkHallAisle(floor,x,y)?darkAisle:HallAisle(floor,x,y)?aisle:baseTile);
                    PaintSurface(map,x,y,tile);
                }
        }

        static bool HallAisle(int floor,int x,int y)=>floor switch
        {
            1 => y>=7&&y<=10 || x>=25&&y>=9&&y<=15,
            2 => y>=7&&y<=9 || y>=13&&y<=15,
            3 => y>=7&&y<=9 || y>=5&&x>=10&&x<=24&&y<=6,
            4 => y>=8&&y<=10 || x>=12&&x<=17&&y>=4&&y<=10,
            5 => y>=8&&y<=9 || x>=21&&y>=6&&y<=9,
            6 => x>=12&&x<=18&&y>=3&&y<=14 || y>=11&&y<=13,
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
        public void RestorePower(){Power=true;foreach(var gate in gateViews)gate.SetActive(false);powerView.GetComponent<SpriteRenderer>().sprite=CampusLook.Sprite("bloque-a-item-tablero-activo");}
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
            if(room==Basement&&!Power&&Mathf.Abs(position.x-Cell(room,17,0).x)<.72f&&p.y>=-3.72f&&p.y<=-.28f)return false;
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
