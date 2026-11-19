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
            new UpperRoom(1,7,10,"Posgrado",13,9,"oficina","campus-muro-panel"),
            new UpperRoom(1,14,10,"Caja",12,8,"terrazo"),
            new UpperRoom(1,24,10,"Dirección de Carrera",12,9,"roble","campus-muro-panel"),
            new UpperRoom(2,6,9,"Cocina de prácticas",19,12,"cocina","campus-muro-azulejo"),
            new UpperRoom(2,13,9,"Sala de Simulación",15,10,"laboratorio","tour-muro-clinica"),
            new UpperRoom(2,23,9,"Histología y Fisiología",16,11,"laboratorio","campus-muro-azulejo"),
            new UpperRoom(3,6,9,"Laboratorio PC",17,11,"computo"),
            new UpperRoom(3,26,9,"Sala Mac",14,10,"linoleo"),
            new UpperRoom(3,10,5,"Streaming",12,8,"acustico","tour-muro-estudio",south:true),
            new UpperRoom(3,15,5,"Fotografía",14,9,"taller","tour-muro-estudio",south:true),
            new UpperRoom(3,23,5,"Sonido",12,8,"acustico","pb-muro-acustico",south:true),
            new UpperRoom(4,7,10,"Sala Jurídica",17,11,"roble","campus-muro-panel"),
            new UpperRoom(4,24,10,"Cámara Gesell",12,9,"acustico","pb-muro-acustico"),
            new UpperRoom(4,14,5,"Aula 4A",16,11,"linoleo",south:true),
            new UpperRoom(5,6,9,"Aula de grupos",19,12,"linoleo"),
            new UpperRoom(5,23,7,"Taller",15,10,"taller","escena-muro-servicio",south:true),
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
            1 => (DoorMaterial.Glass,"servicios"),
            2 => (DoorMaterial.Metal,"clinica"),
            3 when room.Name=="Streaming"||room.Name=="Sonido" => (DoorMaterial.Metal,"acustica"),
            3 => (DoorMaterial.Metal,"sistemas"),
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

        void BuildUpperInterior(int room)
        {
            var layout=InteriorDefinition(room);
            var area=new HashSet<Vector2Int>();
            int center=layout.Width/2+2;
            for(int x=2;x<layout.Width+2;x++)
                for(int y=3;y<layout.Height+3;y++)area.Add(new Vector2Int(x,y));
            var exit=UpperInteriorExit(room);
            GroundSurface(room,area,layout.Surface,layout.Wall,cell=>
                layout.Name=="Sala Jurídica"&&cell.y>=layout.Height-1?"auditorio":
                layout.Name=="Cámara Gesell"&&cell.y>=layout.Height?"oficina":null,WallOpening(room,exit,true));
            DecorateUpperInterior(room,layout,center);
            var (material,finish)=UpperDoorStyle(layout);
            PlaceWallDoor(room,exit,"Salida",material,true,layout.Wall,finish);
            Color lighting=IsDark(room)?new Color(.8f,.45f,.47f):layout.Floor==3?
                new Color(.72f,.86f,1,.85f):Color.white;
            FloorLight(room,center,9,new Vector2(9,9),lighting);
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
                    Place("mob-banca-espera",4,7,2.8f,1.2f);
                    break;
                case "Caja":
                    Back("pb-ventanilla",center-2,3.15f,1.95f);
                    Back("pb-ventanilla",center+2,3.15f,1.95f);
                    Place("bloque-a-maquina-ticket",right,6,1.2f,1.8f);
                    Place("tour-banca-metal",4,7,2.6f,1.2f);
                    break;
                case "Dirección de Carrera":
                    Back("pb-admisiones",center,3.9f,2.1f);
                    Back("campus-prop-tablero-avisos",right,2.45f,1.5f,false);
                    Place("tour-jardinera",4,7,2.4f,1.5f);
                    break;
                case "Cocina de prácticas":
                    Back("bloque-a-horno",4,1.65f,2);
                    Back("tour-campana",9,4.25f,2.2f);
                    Back("bloque-a-nevera",right,1.75f,2.25f);
                    Place("bloque-a-mesa-cocina",6,9,3.8f,1.65f);
                    Place("campus-prop-lavamanos",right,9,1.65f,1.5f);
                    Place("bloque-a-mesa-cocina",6,6,3.8f,1.65f);
                    Place("bloque-a-mesa-registro",16,6,3.2f,1.5f);
                    break;
                case "Sala de Simulación":
                    Back("bloque-a-armario-medico",4,1.65f,2);
                    Back("bloque-a-maniqui",right,1.65f,1.9f);
                    Place("bloque-a-camilla",6,8,3.2f,2);
                    Place("tour-sillon-odontologia",right-1,8,2.1f,2.1f);
                    break;
                case "Histología y Fisiología":
                    Back("bloque-a-armario-medico",4,1.65f,2);
                    Back("campus-prop-lavamanos",right,2,1.75f);
                    foreach(int x in new[]{center-4,center+4})
                    {
                        Place("bloque-a-mesa-diseccion",x,9,3.5f,1.7f);
                        Place("campus-prop-microscopio",x,10,1.1f,1.2f,false);
                    }
                    break;
                case "Laboratorio PC":
                    Back("bloque-a-rack",4,1.65f,2);
                    Back("campus-prop-pizarra",center,4.2f,1.7f,false);
                    foreach(int y in new[]{8,10})
                        foreach(int x in new[]{5,7,13,15})
                        {
                            Place("bloque-a-pc",x,y,1.7f,1.6f);
                            Place("mob-silla-aula",x,y-1,1.1f,1.05f);
                        }
                    break;
                case "Sala Mac":
                    Back("tour-ventanal",center,6,1.7f,false);
                    foreach(int x in new[]{4,6,10,12})
                        Place("tour-monitor-mac",x,9,1.65f,1.7f);
                    Place("tour-banca-metal",right,6,2.65f,1.25f);
                    break;
                case "Streaming":
                    Back("tour-mesa-streaming",center,4.2f,2.15f);
                    Place("bloque-a-microfono",4,8,1.35f,1.6f);
                    Place("bloque-a-rack",right,8,1.4f,1.8f);
                    break;
                case "Fotografía":
                    Back("bloque-a-pantalla-gesell",center,6.1f,2,false);
                    Place("bloque-a-mesa-fotografia",right-1,8,2.8f,1.65f);
                    Place("bloque-a-tripode",4,9,1.45f,1.75f);
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
                        Place("tour-mesa-streaming",4,7,2.45f,1.65f);
                        Place("bloque-a-microfono",right,7,1.3f,1.6f);
                    }
                    break;
                case "Sala Jurídica":
                    Back("mob-mesa-juridica",center,5.2f,1.75f);
                    Place("tour-atril-auditorio",right-1,10,1.7f,1.6f);
                    foreach(int x in new[]{5,7,13,15})
                        Place("mob-silla-visita",x,8,1.25f,1.3f);
                    break;
                case "Cámara Gesell":
                    Back("bloque-a-pantalla-gesell",center,6.7f,2,false);
                    Place("mob-mesa-profesor",center,back-2,5,1.55f);
                    Place("mob-silla-aula",4,7,1.4f,1.4f);
                    break;
                case "Aula 4A":
                    Back("campus-prop-pizarra",center,5,1.7f,false);
                    foreach(int y in new[]{8,10})
                        foreach(int x in new[]{center-4,center+4})
                            Place("mob-mesa-aula",x,y,2.85f,1.45f);
                    break;
                case "Aula de grupos":
                    Back("bloque-a-pizarra-movil",center,3.1f,1.95f,false);
                    foreach(int y in new[]{8,11})
                        foreach(int x in new[]{center-5,center+5})
                            Place("mob-mesa-grupo",x,y,3.7f,2);
                    break;
                case "Taller":
                    Back("escena-panel-herramientas",center,5.1f,1.75f,false);
                    Back("bloque-a-rack",right,1.6f,2);
                    Place("bloque-a-mesa-cocina",5,9,3.75f,1.65f);
                    Place("mob-mesa-taller",right-1,8,3,1.65f);
                    break;
                case "Sala de lectura":
                    Back("mob-estante-roble",4,1.9f,2.15f);
                    Back("mob-estante-bajo",6,1.9f,2.15f);
                    Back("mob-estante-acero",right-2,1.9f,2.15f);
                    Back("mob-estante-exhibidor",right,1.9f,2.15f);
                    Back("tour-ventanal",center,5,1.7f,false);
                    foreach(int y in new[]{8,11})
                        foreach(int x in new[]{center-5,center+5})
                            Place(x<center?"mob-mesa-lectura-lampara":"mob-mesa-lectura-doble",x,y,3.1f,1.7f);
                    break;
                case "Baños":
                    foreach(int x in new[]{4,7,10})
                    {
                        Back("campus-prop-inodoro",x,1.3f,1.45f);
                        Place("escena-division-bano",x,8,1.45f,1.7f);
                    }
                    Place("campus-prop-lavamanos",right,5,1.7f,1.45f);
                    break;
            }
            void Detail(string asset,int x,int y,float width=.86f,float height=.58f,float lift=.6f)
                =>PlaceRoomDetail(room,asset,x,y,width,height,lift);
            switch(layout.Name)
            {
                case "Posgrado": Detail("carpetas",center+1,back); Detail("horarios",4,back,1.55f,1.05f,1.16f); break;
                case "Caja": Detail("lector",center-2,back); Detail("lector",center+2,back); break;
                case "Dirección de Carrera": Detail("carpetas",center-1,back); Detail("horarios",4,back,1.58f,1.05f,1.15f); break;
                case "Cocina de prácticas": Detail("bandeja-gastro",6,6); Detail("bandeja-gastro",6,9); Detail("cuaderno",16,6); break;
                case "Sala de Simulación": Detail("instrumental",5,8); Detail("dispensador",right+1,back,.62f,.9f,.85f); break;
                case "Histología y Fisiología": Detail("muestras",center-4,9); Detail("instrumental",center+4,9); break;
                case "Laboratorio PC": Detail("placa-electronica",5,8,.74f,.48f,.35f); Detail("horarios",right,back,1.45f,.95f,1.15f); break;
                case "Sala Mac": Detail("cuaderno",5,9); Detail("horarios",4,back,1.4f,.97f,1.15f); break;
                case "Streaming": Detail("mezcladora",center,back,1.2f,.63f); Detail("luz-estudio",4,back,1.06f,1.47f,.39f); break;
                case "Fotografía": Detail("luz-estudio",right,back,1.18f,1.5f,.4f); Detail("luz-estudio",4,back,1.06f,1.46f,.4f); break;
                case "Sonido": if(!IsDark(room))Detail("mezcladora",4,7,1.16f,.63f); break;
                case "Sala Jurídica": Detail("carpetas",center-1,back); Detail("cuaderno",center+1,back); break;
                case "Cámara Gesell": Detail("cuaderno",center-1,back-2); Detail("instrumental",center+1,back-2); break;
                case "Aula 4A": Detail("horarios",4,back,1.55f,.99f,1.15f); Detail("cuaderno",center-4,8); Detail("cuaderno",center+4,10); break;
                case "Aula de grupos": Detail("cuaderno",center-5,11); Detail("carpetas",center+5,8); break;
                case "Taller": Detail("placa-electronica",right-1,8,1.03f,.65f); Detail("placa-electronica",5,9,1.03f,.65f); break;
                case "Sala de lectura": Detail("cuaderno",center-5,8); Detail("cuaderno",center+5,11); Detail("lector",center+5,8); break;
                case "Baños": Detail("dispensador",right+1,5,.54f,.71f,.72f); break;
            }
            void Object(string asset,int x,int y,float width=.95f,float height=.65f,float lift=.76f)
                =>PlaceCampusProp(room,asset,x,y,width,height,lift);
            switch(layout.Name)
            {
                case "Posgrado":
                    Object("agenda",center-1,back);
                    Back("utileria-archivador",right-2,1.55f,1.82f);
                    break;
                case "Caja":
                    Object("bandeja-monedas",center-3,back);
                    Object("registro-turno",right+1,back,1.55f,1.16f,1.24f);
                    break;
                case "Dirección de Carrera":
                    Object("bandeja-documentos",center+1,back);
                    Object("organizador",center+2,back,.76f,.75f);
                    break;
                case "Cocina de prácticas":
                    Object("batidora",7,back,1.15f,1.1f,.52f);
                    Object("tabla-ingredientes",7,6);
                    break;
                case "Sala de Simulación":
                    Object("monitor-paciente",7,8,1.32f,1.16f,1.09f);
                    Object("maletin-clinico",right-1,8,.78f,.67f,.86f);
                    break;
                case "Histología y Fisiología":
                    Object("gradilla",center-3,9,.9f,.71f);
                    Object("portaobjetos",center+3,9);
                    break;
                case "Laboratorio PC":
                    Back("utileria-torre-pc",right-2,1.36f,1.5f);
                    Object("teclado-pc",7,8,.9f,.35f,.48f);
                    break;
                case "Sala Mac":
                    Object("tableta-grafica",4,9,.8f,.55f,.44f);
                    Object("disco-externo",12,9,.67f,.45f,.4f);
                    break;
                case "Streaming":
                    Object("auriculares",center+1,back,.69f,.72f,.88f);
                    Object("cartel-aire",right,back,1.55f,.95f,1.28f);
                    break;
                case "Fotografía":
                    Object("rollo-fondo",center,back,2.4f,1.55f,1.36f);
                    Object("camara-reflex",right-1,8,.89f,.65f,.72f);
                    break;
                case "Sonido":
                    Object("altavoz",right,back,1.02f,1.32f,.92f);
                    Object("bobina-cable",4,IsDark(room)?10:7,.74f,.61f,.74f);
                    break;
                case "Sala Jurídica":
                    Object("mazo",center,back,.8f,.59f,.89f);
                    Object("expediente",right-1,10);
                    break;
                case "Cámara Gesell":
                    Object("portapapeles",center,back-2);
                    Object("grabadora",center+2,back-2,.66f,.64f);
                    break;
                case "Aula 4A":
                    Object("lapicero",center+4,8,.6f,.62f);
                    Object("borrador",center,back,.8f,.39f,.67f);
                    break;
                case "Aula de grupos":
                    Object("tubo-planos",center+5,11);
                    Object("panel-notas",right,back,1.9f,1.2f,1.35f);
                    break;
                case "Taller":
                    Object("multimetro",right,8,.81f,.73f,.83f);
                    Object("caja-componentes",4,9,.93f,.63f,.8f);
                    break;
                case "Sala de lectura":
                    Object("separalibros",right-2,back,.92f,.63f,1.38f);
                    Object("lupa-lectura",center+5,11,.7f,.52f,.68f);
                    break;
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
