using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        public const int FirstUpperInterior = FirstStairwell + StairwellCount;

        sealed class UpperRoom
        {
            public readonly int Floor, X, Y, Width, Height;
            public readonly string Name, Surface, Wall;
            public readonly bool South;

            public UpperRoom(int floor,int x,int y,string name,int width,int height,string surface,string wall="pb-muro-yeso",bool south=false)
            {
                Floor=floor; X=x; Y=y; Name=name; Width=width; Height=height;
                Surface=surface; Wall=wall; South=south;
            }
        }

        // Door locations and room sizes are provisional where surveyed plans are unavailable.
        static readonly UpperRoom[] upperRooms =
        {
            new UpperRoom(1,7,18,"Posgrado",15,10,"oficina","campus-muro-panel"),
            new UpperRoom(1,15,18,"Caja",14,10,"terrazo"),
            new UpperRoom(1,23,10,"Dirección de Carrera",14,11,"roble","campus-muro-panel"),
            new UpperRoom(2,6,11,"Cocina de prácticas",19,12,"cocina","campus-muro-azulejo"),
            new UpperRoom(2,15,18,"Sala de Simulación",15,11,"laboratorio","tour-muro-clinica"),
            new UpperRoom(2,23,18,"Histología y Fisiología",16,11,"laboratorio","campus-muro-azulejo"),
            new UpperRoom(3,6,18,"Laboratorio PC",17,11,"computo"),
            new UpperRoom(3,23,18,"Sala Mac",14,10,"linoleo"),
            new UpperRoom(3,8,6,"Streaming",12,9,"acustico","tour-muro-estudio",south:true),
            new UpperRoom(3,15,6,"Fotografía",14,10,"taller","tour-muro-estudio",south:true),
            new UpperRoom(3,23,6,"Sonido",12,9,"acustico","pb-muro-acustico",south:true),
            new UpperRoom(4,7,18,"Sala Jurídica",17,11,"roble","campus-muro-panel"),
            new UpperRoom(4,26,18,"Cámara Gesell",12,10,"acustico","pb-muro-acustico"),
            new UpperRoom(4,14,7,"Aula 4A",16,11,"linoleo",south:true),
            new UpperRoom(5,7,18,"Aula de grupos",19,12,"linoleo"),
            new UpperRoom(5,25,8,"Taller",15,11,"taller","escena-muro-servicio",south:true),
            new UpperRoom(6,13,9,"Sala de lectura",19,12,"roble","campus-muro-panel"),
            new UpperRoom(0,8,20,"Baños",10,8,"bano","campus-muro-azulejo")
        };

        public static int LiftCabin(bool dark)=>FirstUpperInterior+upperRooms.Length*2+(dark?1:0);
        public static int ServiceGallery => LiftCabin(true)+1;
        public static int TotalRooms => ServiceGallery+1;
        public static bool IsLiftCabin(int room)=>room==LiftCabin(false)||room==LiftCabin(true);
        public static bool IsUpperInterior(int room)=>room>=FirstUpperInterior&&room<LiftCabin(false);
        static UpperRoom InteriorDefinition(int room)=>upperRooms[(room-FirstUpperInterior)%upperRooms.Length];
        public static int UpperInteriorFloor(int room)=>InteriorDefinition(room).Floor;
        public static bool IsDarkUpperInterior(int room)=>room>=FirstUpperInterior+upperRooms.Length&&room<LiftCabin(false);
        public static string UpperInteriorName(int room)=>InteriorDefinition(room).Name;
        internal static string UpperInteriorSurface(int room)=>InteriorDefinition(room).Surface;
        public int UpperInteriorHall(int room)=>ForFloor(UpperInteriorFloor(room),IsDark(room));
        public Vector2 UpperInteriorDoor(int room)
        {
            var layout=InteriorDefinition(room);
            return Cell(UpperInteriorHall(room),layout.X,layout.Y);
        }
        public Vector2 UpperInteriorExit(int room)=>Cell(room,InteriorDefinition(room).Width/2+2,3);
        public Vector2 CabinExit(int room)=>Cell(room,11,6);
        public Vector2 CabinPanel(int room)=>Cell(room,13,10);

        void PlaceLift(int room)
        {
            var call=Lift(room);
            var boundary=Vector2Int.RoundToInt(call-Cell(room,0,0))+Vector2Int.up;
            rooms[room].Lift=CreateWallDoor(room,call,"Ascensor",DoorMaterial.Metal,false,HallWall(room,boundary));
        }

        void BuildCabin(int room)
        {
            var area=new HashSet<Vector2Int>();
            for(int x=8;x<=14;x++)
                for(int y=6;y<=11;y++)area.Add(new Vector2Int(x,y));
            var exit=CabinExit(room);
            GroundSurface(room,area,"metal","pb-muro-vidrio",openings:WallOpening(room,exit,true));
            var panel=CabinPanel(room);
            Art(room,"bloque-a-panel-horario",panel+Vector2.up*.84f,new Vector2(1.3f,1.45f),
                "Panel del ascensor",CampusLook.Depth(panel.y));
            Art(room,"solid",exit+Vector2.down*.61f,new Vector2(2.65f,.38f),"Umbral bajo la puerta",879)
                .GetComponent<SpriteRenderer>().color=IsDark(room)?new Color(.07f,.09f,.1f):new Color(.2f,.26f,.29f);
            rooms[room].Lift=CreateDoor(room,exit,exit+Vector2.down*.55f,new Vector2(2.65f,1.12f),
                "Puerta de la cabina",CampusLook.Depth(exit.y),DoorMaterial.Metal);
            SetLiftDoors(room,1f);
            FloorLight(room,11,9,new Vector2(6,5),IsDark(room)?new Color(.68f,.4f,.44f):Color.white);
        }

        public void SetLiftDoors(int room,float open)=>rooms[room]?.Lift?.SetOpening(open);

        public IEnumerable<int> UpperInteriorsOn(int room)
        {
            int floor=Floor(room);
            if(floor<0||floor>6||IsInterior(room)||IsStairwell(room)||IsLiftCabin(room)||room==Basement)yield break;
            int start=FirstUpperInterior+(IsDark(room)?upperRooms.Length:0);
            for(int index=0;index<upperRooms.Length;index++)
                if(upperRooms[index].Floor==floor)yield return start+index;
        }

        static Vector2Int UpperDoorBoundary(UpperRoom layout)=>new Vector2Int(layout.X,layout.Y+(layout.South?-1:1));

        void AddUpperDoorOpenings(int room,HashSet<Vector2Int> openings)
        {
            foreach(int interior in UpperInteriorsOn(room))openings.Add(UpperDoorBoundary(InteriorDefinition(interior)));
        }

        void BuildUpperDoors(int room)
        {
            foreach(int interior in UpperInteriorsOn(room)){
                var layout=InteriorDefinition(interior);
                var (material,finish)=UpperDoorStyle(layout);
                PlaceWallDoor(room,UpperInteriorDoor(interior),layout.Name,material,layout.South,
                    HallWall(room,UpperDoorBoundary(layout)),finish);
            }
        }

        static (DoorMaterial material,string finish) UpperDoorStyle(UpperRoom room)=>room.Floor switch
        {
            1 when room.Name=="Dirección de Carrera" => (DoorMaterial.Wood,null),
            1 => (DoorMaterial.Glass,"servicios"),
            2 when room.Name=="Cocina de prácticas" => (DoorMaterial.Metal,"taller"),
            2 when room.Name=="Histología y Fisiología" => (DoorMaterial.Glass,"sistemas"),
            2 => (DoorMaterial.Metal,"clinica"),
            3 when room.Name=="Streaming"||room.Name=="Sonido" => (DoorMaterial.Metal,"acustica"),
            3 when room.Name=="Sala Mac" => (DoorMaterial.Glass,"servicios"),
            3 when room.Name=="Fotografía" => (DoorMaterial.Wood,"madera"),
            3 => (DoorMaterial.Metal,"sistemas"),
            4 when room.Name=="Cámara Gesell" => (DoorMaterial.Glass,"servicios"),
            4 when room.Name=="Sala Jurídica" => (DoorMaterial.Wood,"madera"),
            4 => (DoorMaterial.Wood,"aulas"),
            5 when room.Name=="Taller" => (DoorMaterial.Metal,"taller"),
            5 => (DoorMaterial.Wood,"grupos"),
            6 => (DoorMaterial.Wood,"biblioteca"),
            _ => (DoorMaterial.Wood,null)
        };

        public bool UpperDoorLocked(int room,bool signalReady)=>IsDark(room)&&UpperInteriorName(room)=="Sonido"&&!signalReady;

        public SpriteRenderer MarkFloorPlate(int room,Vector2 point,string label)
        {
            var tile=Art(room,"solid",point,new Vector2(1.7f,.72f),"Placa acústica",-884).GetComponent<SpriteRenderer>();
            string key=CampusLook.AssetKey(label.Replace(" · "," "));
            PlaceLettering(room,label,"letras-"+key,point,1.52f,.18f,-883);
            return tile;
        }

        internal SpriteRenderer MarkInductionPosition(int room,Vector2 point,string label)
        {
            var plate=Art(room,"solid",point,new Vector2(1.45f,.48f),label,-879).GetComponent<SpriteRenderer>();
            plate.color=new Color(.48f,.77f,.71f,.84f);
            Art(room,"solid",point,new Vector2(1.29f,.29f),"Centro de la marca de inducción",-878)
                .GetComponent<SpriteRenderer>().color=new Color(.21f,.37f,.38f,.96f);
            return plate;
        }

        void BuildUpperInterior(int room)
        {
            var layout=InteriorDefinition(room);
            var area=new HashSet<Vector2Int>();
            int center=layout.Width/2+2;
            for(int x=2;x<layout.Width+2;x++)
                for(int y=3;y<layout.Height+3;y++)area.Add(new Vector2Int(x,y));
            if(layout.Floor==1)
                area.RemoveWhere(cell=>layout.Name switch{
                    "Posgrado"=>cell.x<=3&&cell.y<=5,
                    "Caja"=>cell.x>=14&&cell.y<=5,
                    _=>cell.x<=4&&cell.y<=5||cell.x>=14&&cell.y<=4
                });
            if(layout.Floor==2)
                area.RemoveWhere(cell=>layout.Name switch{
                    "Cocina de prácticas"=>cell.x>=18&&cell.y<=5,
                    "Sala de Simulación"=>cell.x<=4&&cell.y<=5,
                    _=>cell.x>=15&&cell.y<=5
                });
            if(layout.Floor==3)
                area.RemoveWhere(cell=>layout.Name switch{
                    "Laboratorio PC"=>cell.x<=3&&cell.y<=5,
                    "Sala Mac"=>cell.x>=14&&cell.y<=5,
                    "Streaming"=>cell.x>=12&&cell.y<=5,
                    "Fotografía"=>cell.x<=4&&cell.y<=5,
                    _=>false
                });
            if(layout.Floor==4)
                area.RemoveWhere(cell=>layout.Name switch{
                    "Sala Jurídica"=>cell.x<=4&&cell.y<=5,
                    "Cámara Gesell"=>cell.x>=12&&cell.y<=5,
                    _=>cell.x>=15&&cell.y<=4
                });
            if(layout.Floor==6)
                area.RemoveWhere(cell=>cell.x>=18&&cell.y<=5);
            if(layout.Floor==5)
                area.RemoveWhere(cell=>layout.Name=="Taller"?cell.x>=14&&cell.y<=5:cell.x<=3&&cell.y<=6);
            var exit=UpperInteriorExit(room);
            GroundSurface(room,area,layout.Surface,layout.Wall,cell=>
                layout.Name=="Sala Jurídica"&&cell.y>=layout.Height-1?"auditorio":
                layout.Name=="Posgrado"&&cell.x<=6&&cell.y>=7?"roble":
                layout.Name=="Caja"&&cell.y>=10?"oficina":
                layout.Name=="Dirección de Carrera"&&cell.x<=6&&cell.y>=6?"oficina":
                layout.Name=="Cocina de prácticas"&&cell.x>=15?"metal":
                layout.Name=="Sala de Simulación"&&cell.x>=8&&cell.x<=10?"linoleo":
                layout.Name=="Histología y Fisiología"&&cell.x>=9&&cell.x<=11?"linoleo":
                layout.Name=="Laboratorio PC"&&cell.x>=9&&cell.x<=11?"linoleo":
                layout.Name=="Sala Mac"&&cell.y>=8?"oficina":
                layout.Name=="Streaming"&&cell.x<=5&&cell.y>=8?"metal":
                layout.Name=="Fotografía"&&cell.y>=10?"auditorio":
                layout.Name=="Aula 4A"&&cell.x>=9&&cell.x<=11?"oficina":
                layout.Name=="Aula de grupos"&&cell.x<=8&&cell.y>=7?"roble":
                layout.Name=="Taller"&&cell.x>=8&&cell.x<=10?"metal":
                layout.Name=="Cámara Gesell"&&cell.y>=layout.Height?"oficina":
                layout.Name=="Sala de lectura"&&cell.x<=9&&cell.y>=7?"auditorio":null,WallOpening(room,exit,true));
            DecorateUpperInterior(room,layout,center);
            var (material,finish)=UpperDoorStyle(layout);
            PlaceWallDoor(room,exit,"Salida",material,true,layout.Wall,finish);
            Color lighting=IsDark(room)?new Color(.8f,.45f,.47f):layout.Floor==3?
                new Color(.72f,.86f,1,.85f):Color.white;
            if(layout.Floor!=6)FloorLight(room,center,9,new Vector2(9,9),lighting);
            if(IsDark(room)&&layout.Name!="Sonido")
                Prop(room,layout.Surface=="roble"||layout.Surface=="laboratorio"?"bloque-a-charco":"bloque-a-cable-caido",
                    layout.Width,6,2.1f,1.2f,false);
            if(!IsDark(room))PopulateUpperInterior(room,layout,center);
        }

        void DecorateUpperInterior(int room,UpperRoom layout,int center)
        {
            int back=layout.Height+1, right=layout.Width;
            void Place(string sprite,int x,int y,float width,float height,bool solid=true)
                =>Prop(room,sprite,x,y,width,height,solid);
            void Back(string sprite,int x,float width,float height,bool solid=true)
                =>Place(sprite,x,back,width,height,solid);
            switch(layout.Name)
            {
                case "Posgrado":
                    Back("pb-servicios",center,4,2);
                    Back("mob-estante-archivo",right,1.8f,2.15f);
                    Place("mob-banca-espera",4,10,2.6f,1.2f);
                    Place("mob-mesa-grupo",5,8,2.75f,1.56f);
                    Place("mob-estante-exhibidor",14,8,1.9f,1.75f);
                    FloorLight(room,5,8,new Vector2(4.5f,5),IsDark(room)?new Color(.45f,.66f,.74f,.64f):new Color(1f,.88f,.63f,.8f));
                    break;
                case "Caja":
                    var lanes=new[]{("Pagos",new Color(.75f,.38f,.32f)),("Consultas",new Color(.42f,.68f,.84f)),("Recibos",new Color(.88f,.70f,.35f))};
                    for(int lane=0;lane<lanes.Length;lane++){
                        int x=center+(lane-1)*4;
                        Back("pb-ventanilla",x,3.15f,1.95f);
                        var desk=Cell(room,x,back);
                        MountPlaque(room,lanes[lane].Item1,desk+Vector2.up*1.2f,.22f,CampusLook.Depth(desk.y)+4);
                        Art(room,"solid",Cell(room,x,9),new Vector2(2.55f,.085f),"Fila de "+lanes[lane].Item1,-870)
                            .GetComponent<SpriteRenderer>().color=lanes[lane].Item2;
                    }
                    Place("bloque-a-maquina-ticket",right,7,1.2f,1.8f);
                    Place("tour-banca-metal",4,7,2.6f,1.2f);
                    if(IsDark(room))FloorLight(room,center,10,new Vector2(4,5),new Color(.47f,.73f,.9f,.72f));
                    break;
                case "Dirección de Carrera":
                    Back("pb-admisiones",center+3,3.8f,2.1f);
                    Back("campus-prop-tablero-avisos",right,2.45f,1.5f,false);
                    Place("mob-banca-espera",4,7,2.65f,1.25f);
                    Place("mob-estante-bajo",4,11,2.4f,1.78f);
                    Place("mob-mesa-profesor",10,10,3.7f,1.62f);
                    Place("mob-silla-visita",8,8,1.2f,1.2f);
                    Place("mob-silla-visita",11,8,1.2f,1.2f);
                    FloorLight(room,11,12,new Vector2(6,5),IsDark(room)?new Color(.79f,.56f,.49f,.68f):new Color(1f,.81f,.59f,.80f));
                    break;
                case "Cocina de prácticas":
                    Back("bloque-a-horno",4,1.65f,2);
                    Back("tour-campana",9,4.25f,2.2f);
                    Back("bloque-a-nevera",right,1.75f,2.25f);
                    Place("bloque-a-mesa-cocina",6,9,4.4f,1.76f);
                    PlaceWashStation(room,right,10,1.9f,1.62f);
                    Place("bloque-a-mesa-cocina",6,6,4.4f,1.76f);
                    Place("bloque-a-mesa-registro",16,6,3.6f,1.6f);
                    Place("mob-estante-bajo",17,11,2.65f,1.6f);
                    FloorLight(room,6,10,new Vector2(7,7),new Color(.99f,.72f,.47f,IsDark(room)?.52f:.82f));
                    break;
                case "Sala de Simulación":
                    Back("bloque-a-armario-medico",4,1.65f,2);
                    Back("bloque-a-maniqui",right,1.65f,1.9f);
                    Place("bloque-a-camilla",5,9,3.25f,2);
                    Place("bloque-a-camilla",right-2,9,3.25f,2);
                    PlaceWashStation(room,right,7,1.55f,1.45f,dispenser:true);
                    Place("mob-banca-espera",4,7,2.6f,1.2f);
                    Prop(room,"campus-prop-botiquin",4,12,.52f,.52f,false);
                    MountPlaque(room,"Botiquín",Cell(room,4,12)+Vector2.up*1.35f,.22f,CampusLook.Depth(Cell(room,4,12).y)+5);
                    FloorLight(room,5,10,new Vector2(5,6),new Color(.44f,.86f,.83f,IsDark(room)?.79f:.62f));
                    break;
                case "Histología y Fisiología":
                    Back("bloque-a-armario-medico",4,1.65f,2);
                    PlaceWashStation(room,right,back,2,1.75f);
                    Place("mob-mesa-auxiliar",8,8,1.35f,1.03f);
                    foreach(int x in new[]{center-5,center+4})
                        foreach(int y in new[]{8,11}){
                            Place("bloque-a-mesa-diseccion",x,y,3.5f,1.7f);
                            Place("campus-prop-microscopio",x,y,1.1f,1.2f,false);
                        }
                    FloorLight(room,center,10,new Vector2(4,7),new Color(.67f,.86f,1f,IsDark(room)?.68f:.85f));
                    break;
                case "Laboratorio PC":
                    Back("bloque-a-rack",4,1.65f,2);
                    Back("campus-prop-pizarra",center,4.2f,1.7f,false);
                    foreach(int y in new[]{8,11})
                        foreach(int x in new[]{5,7,13,15})
                        {
                            Place("bloque-a-pc",x,y,1.7f,1.6f);
                            Place("mob-silla-aula",x,y-1,1.1f,1.05f);
                        }
                    Place("mob-estante-bajo",16,12,2,1.65f);
                    FloorLight(room,center,10,new Vector2(4,8),new Color(.56f,.83f,1f,.75f));
                    break;
                case "Sala Mac":
                    Back("tour-ventanal",center,6,1.7f,false);
                    Place("bloque-a-mesa-registro",6,10,4.12f,1.25f);
                    Place("bloque-a-mesa-registro",12,10,4.12f,1.25f);
                    foreach(int x in new[]{5,7,11,13})
                    {
                        var workstation=Cell(room,x,10);
                        Art(room,"tour-monitor-mac",workstation+Vector2.up*.90f,new Vector2(1.55f,1.36f),
                            "Monitor de la mesa",CampusLook.Depth(workstation.y)+2);
                    }
                    Place("mob-mesa-grupo",5,7,3.2f,1.6f);
                    Place("mob-sofa-modular",right,7,2.65f,1.25f);
                    FloorLight(room,center,10,new Vector2(8,5),new Color(.82f,.94f,1f,.78f));
                    break;
                case "Streaming":
                    Back("tour-mesa-streaming",center+2,4.6f,2.15f);
                    Place("mob-mesa-auxiliar",4,10,1.35f,1.02f);
                    Place("bloque-a-microfono",11,8,1.35f,1.6f);
                    Place("bloque-a-rack",right+1,10,1.35f,1.72f);
                    FloorLight(room,10,9,new Vector2(5,6),new Color(.89f,.47f,.57f,.68f));
                    break;
                case "Fotografía":
                    var backdrop=Cell(room,center+2,back)+Vector2.up*.4f;
                    var curtain=Art(room,"solid",backdrop,new Vector2(6.5f,1.72f),"Fondo de fotografía",CampusLook.Depth(backdrop.y));
                    curtain.GetComponent<SpriteRenderer>().color=IsDark(room)?new Color(.31f,.26f,.38f):new Color(.77f,.74f,.82f);
                    var roller=Art(room,"solid",backdrop+Vector2.up*.85f,new Vector2(6.85f,.17f),"Barra del fondo",CampusLook.Depth(backdrop.y)+1);
                    roller.GetComponent<SpriteRenderer>().color=IsDark(room)?new Color(.21f,.23f,.29f):new Color(.30f,.31f,.39f);
                    Place("bloque-a-mesa-fotografia",5,8,3.1f,1.65f);
                    Place("bloque-a-tripode",8,10,1.45f,1.75f);
                    Place("bloque-a-tripode",right,10,1.45f,1.75f);
                    FloorLight(room,11,11,new Vector2(7,4),new Color(.91f,.82f,1f,.76f));
                    break;
                case "Sonido":
                    if(IsDark(room))
                    {
                        Place("tour-cabina-sonido",3,10,1.5f,1.5f);
                        Place("bloque-a-rack",right+1,10,1.25f,1.75f);
                    }
                    else
                    {
                        Back("tour-cabina-sonido",center,5.3f,2.15f);
                        Place("tour-mesa-streaming",6,8,2.95f,1.65f);
                        Place("bloque-a-microfono",right,9,1.3f,1.6f);
                        Place("mob-silla-aula",11,8,1.1f,1.05f);
                    }
                    FloorLight(room,8,9,new Vector2(9,7),new Color(.53f,.81f,.83f,.67f));
                    break;
                case "Sala Jurídica":
                    Back("mob-mesa-juridica",center,6.1f,1.92f);
                    Place("tour-atril-auditorio",right-2,10,1.7f,1.6f);
                    Place("mob-mesa-profesor",5,10,3.1f,1.56f);
                    foreach(int x in new[]{5,7,13,15})
                        Place("mob-silla-visita",x,8,1.25f,1.3f);
                    FloorLight(room,center,11,new Vector2(8,5),new Color(1f,.80f,.58f,IsDark(room)?.53f:.82f));
                    break;
                case "Cámara Gesell":
                    Back("tour-ventanal",center,8.0f,2.14f,false);
                    Place("mob-mesa-profesor",center+2,back-2,4.1f,1.55f);
                    Place("mob-silla-aula",4,7,1.4f,1.4f);
                    Place("bloque-a-pantalla-gesell",4,10,2.3f,1.5f,false);
                    Place("mob-silla-aula",10,8,1.13f,1.14f);
                    Place("mob-silla-aula",12,8,1.13f,1.14f);
                    FloorLight(room,8,10,new Vector2(8,4),new Color(.65f,.76f,1f,IsDark(room)?.78f:.86f));
                    break;
                case "Aula 4A":
                    Back("campus-prop-pizarra",center,5,1.7f,false);
                    Place("mob-mesa-auxiliar",3,10,1.45f,1.08f);
                    Place("mob-mesa-profesor",5,7,2.35f,1.4f);
                    foreach(int y in new[]{9,11})
                        foreach(int x in new[]{center-4,center+4}){
                            Place("mob-mesa-aula",x,y,2.85f,1.45f);
                            Place("mob-silla-aula",x,y-1,1.12f,1.12f);
                        }
                    Prop(room,"tour-ventanal",right,12,2.85f,1.36f,false);
                    FloorLight(room,14,10,new Vector2(5,7),new Color(.88f,.92f,1f,IsDark(room)?.52f:.9f));
                    break;
                case "Aula de grupos":
                    Back("bloque-a-pizarra-movil",center,3.1f,1.95f,false);
                    Place("mob-sofa-modular",5,8,3.15f,1.65f);
                    Place("mob-mesa-auxiliar",7,9,1.45f,1.06f);
                    Place("mob-estante-exhibidor",4,12,2.12f,1.81f);
                    Place("mob-mesa-grupo",center+4,9,3.75f,1.92f);
                    Place("mob-mesa-grupo",center+4,12,3.75f,1.92f);
                    foreach(int y in new[]{8,11})
                        foreach(int x in new[]{center+3,center+5})
                            Place("mob-silla-visita",x,y,1.13f,1.19f);
                    FloorLight(room,6,10,new Vector2(6,5),new Color(.95f,.79f,.59f,.70f));
                    break;
                case "Taller":
                    Back("escena-panel-herramientas",center,5.1f,1.75f,false);
                    Back("bloque-a-rack",right,1.6f,2);
                    Place("mob-mesa-taller",5,9,3.75f,1.65f);
                    Place("mob-mesa-taller",right-1,10,3.35f,1.7f);
                    Place("utileria-prensa-banco",5,12,2.32f,1.89f);
                    Place("utileria-caja-componentes",12,7,1.65f,1.15f);
                    MountPlaque(room,"Herramientas",Cell(room,5,12)+Vector2.up*1.7f,.22f,CampusLook.Depth(Cell(room,5,12).y)+4);
                    FloorLight(room,5,10,new Vector2(7,6),new Color(.94f,.76f,.47f,IsDark(room)?.53f:.85f));
                    break;
                case "Sala de lectura": DecorateReadingRoom(room);break;
                case "Baños":
                    foreach(int x in new[]{4,7,10})
                    {
                        Back("campus-prop-inodoro",x,1.3f,1.45f);
                        Place("escena-division-bano",x,8,1.68f,1.46f);
                        Vector2 stall=Cell(room,x,8);
                        foreach(int side in new[]{-1,1})
                        {
                            Vector2 partition=stall+new Vector2(side*.79f,1.48f);
                            var rail=Art(room,"solid",partition,new Vector2(.10f,2.63f),
                                "Separador de cabina",CampusLook.Depth(stall.y)-7).GetComponent<SpriteRenderer>();
                            rail.color=IsDark(room)?new Color(.26f,.34f,.40f):new Color(.45f,.60f,.60f);
                            rooms[room].Obstacles.Add(new Rect(partition-new Vector2(.05f,1.31f),new Vector2(.10f,2.62f)));
                        }
                    }
                    PlaceWashStation(room,right,5,1.7f,1.45f,dispenser:true);
                    break;
            }
            void Detail(string asset,int x,int y,float width=.86f,float height=.58f,float lift=.6f)
                =>PlaceRoomDetail(room,asset,x,y,width,height,lift);
            switch(layout.Name)
            {
                case "Posgrado": Detail("carpetas",center+1,back); Detail("horarios",4,back,1.55f,1.05f,1.16f); break;
                case "Caja": Detail("lector",center-4,back); Detail("lector",center,back); Detail("lector",center+4,back); break;
                case "Dirección de Carrera": Detail("carpetas",center+3,back); Detail("horarios",4,back,1.58f,1.05f,1.15f); break;
                case "Cocina de prácticas": Detail("bandeja-gastro",6,6); Detail("bandeja-gastro",6,9); Detail("cuaderno",16,6); break;
                case "Sala de Simulación": Detail("instrumental",5,9); break;
                case "Histología y Fisiología": Detail("muestras",center-6,8); Detail("instrumental",center+5,11); break;
                case "Laboratorio PC": Detail("placa-electronica",5,8,.74f,.48f,.35f); Detail("horarios",right,back,1.45f,.95f,1.15f); break;
                case "Sala Mac": Detail("cuaderno",6,10,.59f,.40f,.61f); Detail("horarios",4,back,1.4f,.97f,1.15f); break;
                case "Streaming": Detail("mezcladora",center+2,back,1.2f,.63f); Detail("luz-estudio",6,back,1.06f,1.47f,.39f); break;
                case "Fotografía": Detail("luz-estudio",right,back,1.18f,1.5f,.4f); Detail("luz-estudio",4,back,1.06f,1.46f,.4f); break;
                case "Sonido": if(!IsDark(room))Detail("mezcladora",6,8,1.16f,.63f); break;
                case "Sala Jurídica": Detail("carpetas",center-1,back); Detail("cuaderno",center+1,back); break;
                case "Cámara Gesell": Detail("cuaderno",center+1,back-2); Detail("instrumental",center+3,back-2); break;
                case "Aula 4A": Detail("horarios",4,back,1.55f,.99f,1.15f); Detail("cuaderno",center-4,9); Detail("cuaderno",center+4,11); break;
                case "Aula de grupos": Detail("cuaderno",7,9); Detail("carpetas",center+5,9); break;
                case "Taller": Detail("placa-electronica",right-2,10,1.03f,.65f); Detail("placa-electronica",5,9,1.03f,.65f); break;
            }
            void Object(string asset,int x,int y,float width=.95f,float height=.65f,float lift=.76f)
                =>PlaceCampusProp(room,asset,x,y,width,height,lift);
            switch(layout.Name)
            {
                case "Posgrado":
                    Object("agenda",center-1,back);
                    Back("utileria-archivador",right-3,1.55f,1.82f);
                    break;
                case "Caja":
                    Object("bandeja-monedas",center-5,back);
                    Object("registro-turno",right+1,back,1.55f,1.16f,1.24f);
                    break;
                case "Dirección de Carrera":
                    Object("bandeja-documentos",center+2,back);
                    Object("organizador",center+4,back,.76f,.75f);
                    break;
                case "Cocina de prácticas":
                    Object("batidora",7,back,1.15f,1.1f,.52f);
                    Object("tabla-ingredientes",7,6);
                    break;
                case "Sala de Simulación":
                    Object("monitor-paciente",right-1,9,1.32f,1.16f,1.09f);
                    break;
                case "Histología y Fisiología":
                    Object("gradilla",center-4,11,.9f,.71f);
                    Object("portaobjetos",center+5,8);
                    break;
                case "Laboratorio PC":
                    Back("utileria-torre-pc",right-2,1.36f,1.5f);
                    Object("teclado-pc",7,8,.9f,.35f,.48f);
                    break;
                case "Sala Mac":
                    Object("tableta-grafica",5,7,.78f,.53f,.70f);
                    Object("disco-externo",12,10,.64f,.43f,.53f);
                    break;
                case "Streaming":
                    Object("auriculares",center+1,back,.69f,.72f,.88f);
                    Object("cartel-aire",right,back,1.55f,.95f,1.28f);
                    break;
                case "Fotografía":
                    Object("camara-reflex",5,8,.89f,.65f,.72f);
                    break;
                case "Sonido":
                    Object("altavoz",right,back,1.02f,1.32f,.92f);
                    Prop(room,"bloque-a-cable-caido",4,IsDark(room)?10:7,1.38f,.51f,false);
                    break;
                case "Sala Jurídica":
                    Object("mazo",center,back,.8f,.59f,.89f);
                    Object("expediente",5,10);
                    break;
                case "Cámara Gesell":
                    Object("portapapeles",center+2,back-2);
                    Detail("mezcladora",center+3,back-2,.82f,.51f,.72f);
                    break;
                case "Aula 4A":
                    Object("lapicero",center+4,9,.6f,.62f);
                    Object("borrador",center,back,.8f,.39f,.67f);
                    break;
                case "Aula de grupos":
                    Object("tubo-planos",center+5,12);
                    Object("panel-notas",right,back,1.9f,1.2f,1.35f);
                    break;
                case "Taller":
                    Object("multimetro",right,10,.81f,.73f,.83f);
                    Object("caja-componentes",4,9,.93f,.63f,.8f);
                    break;
            }
        }

        void PlaceWashStation(int room,int x,int y,float width,float height,bool dispenser=false)
        {
            Vector2 feet=Cell(room,x,y);
            Prop(room,"utileria-base-lavamanos",x,y,width,height*.79f);
            int order=CampusLook.Depth(feet.y)+2;
            Vector2 bowl=feet+new Vector2(dispenser?-.19f:0f,height*.65f);
            Art(room,"campus-prop-lavamanos",bowl,new Vector2(width*.73f,height*.76f),"Lavamanos",order);
            if(dispenser)
            {
                string art=IsDark(room)?"backfranz-aula-dispensador":"aula-dispensador";
                Art(room,art,feet+new Vector2(width*.32f,height*.54f),new Vector2(.32f,.46f),
                    "Dispensador del lavamanos",order+1);
            }
        }

        void PlaceCampusProp(int room,string asset,int x,int y,float width=.95f,float height=.65f,float lift=.76f)
        {
            var position=Cell(room,x,y);
            Art(room,"utileria-"+asset,position+Vector2.up*lift,new Vector2(width,height),asset,CampusLook.Depth(position.y)+3);
        }

        void PlaceRoomDetail(int room,string asset,int x,int y,float width=.86f,float height=.58f,float lift=.6f)
        {
            var position=Cell(room,x,y);
            string prefix=IsDark(room)?"backfranz-aula-":"aula-";
            Art(room,prefix+asset,position+Vector2.up*lift,new Vector2(width,height),asset,CampusLook.Depth(position.y)+2);
        }

        Rect UpperInteriorBounds(int room)
        {
            var layout=InteriorDefinition(room);
            return Rect.MinMaxRect(Cell(room,0,0).x,-9,Cell(room,layout.Width+3,0).x,layout.Height-5);
        }
    }
}
