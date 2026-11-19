using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        public const int FirstInterior=16, FirstDarkInterior=23;
        public const int FirstGroundDoor=1; // Keep the old reception room IDs reserved for saved games.
        public readonly struct GroundDoor
        {
            public readonly string Name;
            public readonly int X,Y;
            public readonly bool South;
            public GroundDoor(string name,int x,int y,bool south=false){Name=name;X=x;Y=y;South=south;}
        }
        // Coordinates are interaction points; room geometry is loaded separately.
        public static readonly GroundDoor[] GroundDoors={
            new GroundDoor("Recepción",54,18),new GroundDoor("Financiero",44,18),
            new GroundDoor("Servicios Estudiantiles",23,18),new GroundDoor("Auditorio",7,13,true),
            new GroundDoor("Game Room",14,13,true),new GroundDoor("Café",27,13,true),
            new GroundDoor("Admisiones",43,13,true)
        };
        public static bool IsGroundHall(int room)=>room==Ground||room==DarkGround;
        public static bool IsInterior(int room)=>IsUpperInterior(room)||room>=FirstInterior&&room<FirstStairwell;
        public static int InteriorIndex(int room)=>room>=FirstDarkInterior?room-FirstDarkInterior:room-FirstInterior;
        public static int InteriorRoom(int index,bool dark)=>(dark?FirstDarkInterior:FirstInterior)+index;
        public static bool IsFormerReception(int room)=>room==FirstInterior||room==FirstDarkInterior;
        public Vector2 DoorPoint(int room,int index)=>Cell(room,GroundDoors[index].X,GroundDoors[index].Y);
        public Vector2 Stairs(int room,int core=0)
        {
            if(IsGroundHall(room))return core==0?Cell(room,36,21):Cell(room,5,20);
            return core==0?Cell(room,27,18):Cell(room,2,18);
        }

        public Vector2 StairLanding(int room,bool upper)
        {
            bool central=room==FirstStairwell;
            return Cell(room,upper==central?5:12,upper?17:3);
        }

        void BuildStairwell(int room)
        {
            var area=new HashSet<Vector2Int>();
            bool central=room==FirstStairwell;
            int lower=central?12:5, upper=central?5:12;
            void Fill(int x0,int x1,int y0,int y1)
            {
                for(int x=x0;x<=x1;x++)
                    for(int y=y0;y<=y1;y++)area.Add(new Vector2Int(x,y));
            }
            Fill(lower-1,lower+1,3,11);
            Fill(4,13,10,12);
            Fill(upper-1,upper+1,11,17);
            GroundSurface(room,area,"terrazo",central?"pb-muro-yeso":"campus-muro-panel");
            Art(room,"pb-gradas",Cell(room,lower,7),new Vector2(3,7),"Primer tramo",-950);
            Art(room,"pb-gradas",Cell(room,upper,14),new Vector2(3,5),"Segundo tramo",-950);
            FloorLight(room,8,11,new Vector2(8,4),Color.white);
        }
        public Vector2 ReceptionContact=>Cell(Ground,54,17);
        public Vector2 GuardContact=>Cell(Ground,41,17);
        const int SecurityColumn=39, FirstSecurityLane=13;
        public const int AccessLaneCount=6;
        public const int AccessibleLane=0;
        readonly bool[] securityOpen=new bool[AccessLaneCount];
        readonly SpriteRenderer[] securityGates=new SpriteRenderer[AccessLaneCount];
        readonly SpriteRenderer[] securityReaders=new SpriteRenderer[AccessLaneCount];
        readonly float[] gateProgress=new float[AccessLaneCount];
        readonly float[] gateHold=new float[AccessLaneCount];

        public bool SecurityOpen(int lane)=>securityOpen[lane];
        public Vector2 SecurityPoint(int lane,bool fromEntrance)=>Cell(Ground,fromEntrance?40:38,FirstSecurityLane+lane);

        public int SecurityLane(Vector2 position)
        {
            int lane=Mathf.RoundToInt(position.y)+10-FirstSecurityLane;
            if(lane<0||lane>=AccessLaneCount)return -1;
            return Vector2.Distance(position,SecurityPoint(lane,true))<.66f||
                Vector2.Distance(position,SecurityPoint(lane,false))<.66f?lane:-1;
        }

        bool SecurityBlocks(Vector2 position)
        {
            if(Mathf.Abs(position.x-Cell(Ground,SecurityColumn,0).x)>.62f)return false;
            int lane=Mathf.RoundToInt(position.y)+10-FirstSecurityLane;
            return lane<0||lane>=AccessLaneCount||!securityOpen[lane]||
                Mathf.Abs(position.y-SecurityPoint(lane,true).y)>(lane==AccessibleLane?.47f:.35f);
        }

        public void ShowSecuritySignal(int lane,Color color)=>securityReaders[lane].color=color;

        static string SecurityArt(int lane)=>lane==AccessibleLane?"control-accesible":"control-molinete";

        void PoseSecurityGate(int lane,float fraction)
        {
            gateProgress[lane]=fraction;
            int frame=Mathf.RoundToInt(fraction*6);
            securityGates[lane].sprite=CampusLook.Sprite(SecurityArt(lane)+"-"+frame.ToString("00"));
        }

        public void ShowSecurityOpening(int lane,float fraction)
        {
            PoseSecurityGate(lane,fraction);
            if(fraction<1f)return;
            securityOpen[lane]=true;
            gateHold[lane]=lane==AccessibleLane?3.5f:.38f;
            ShowSecuritySignal(lane,new Color(.53f,.86f,.66f));
        }

        internal void UpdateSecurityGates(Vector2 player,float delta)
        {
            for(int lane=0;lane<AccessLaneCount;lane++){
                if(!securityOpen[lane])continue;
                var center=Cell(Ground,SecurityColumn,FirstSecurityLane+lane);
                var relative=player-center;
                bool approaching=Mathf.Abs(relative.x)<1.44f&&Mathf.Abs(relative.y)<.46f;
                if(approaching)gateHold[lane]=.38f;
                else gateHold[lane]=Mathf.Max(0,gateHold[lane]-delta);
                float pose=Mathf.MoveTowards(gateProgress[lane],gateHold[lane]>0?1:0,delta*4.6f);
                if(pose!=gateProgress[lane])PoseSecurityGate(lane,pose);
            }
        }

        void BuildSecurity()
        {
            for(int lane=0;lane<AccessLaneCount;lane++){
                var point=Cell(Ground,SecurityColumn,FirstSecurityLane+lane);
                int order=CampusLook.Depth(point.y)-3;
                securityGates[lane]=Art(Ground,SecurityArt(lane)+"-00",point,new Vector2(lane==AccessibleLane?1.45f:1.3f,.98f),
                    lane==AccessibleLane?"Paso accesible":"Molinete",order).GetComponent<SpriteRenderer>();
                securityReaders[lane]=Art(Ground,"solid",point+new Vector2(.32f,.39f),new Vector2(.14f,.057f),
                    "Indicador del lector",order+1).GetComponent<SpriteRenderer>();
                ShowSecuritySignal(lane,new Color(.63f,.74f,.71f));
            }
            var seat=GuardContact+Vector2.up;
            Art(Ground,"pb-silla-blanca",seat+Vector2.up*.31f,new Vector2(1.05f,1.14f),"Silla del guardia",CampusLook.Depth(seat.y)-2);
            PlaceSpeaker(Ground,"pb-guardia","Guardia",GuardContact,true);
            Prop(Ground,"mob-banca-pasillo",16,16,3.5f,1f);
            Prop(Ground,"mob-silla-visita",22,14,1.35f,1.1f,false);
            Prop(Ground,"mob-banca-espera",28,16,3.5f,1f);
        }

        void PlaceSpeaker(int room,string sprite,string name,Vector2 contact,bool seated=false)
        {
            var standing = contact + Vector2.up;
            string portrait=seated&&name=="Guardia"?"npc-guardia-sentado":CampusLook.Person(name,sprite);
            Art(room,portrait,standing + Vector2.up*(seated?.26f:.35f),new Vector2(.85f,seated?.94f:1.18f),name,CampusLook.Depth(standing.y));
            rooms[room].Obstacles.Add(new Rect(standing - new Vector2(.24f,.2f),new Vector2(.48f,.4f)));
        }

        void FloorLight(int room,int x,int y,Vector2 size,Color color)
        {
            var reflection = Art(room,"lightpool",Cell(room,x,y),size,"Reflejo de luz",-900);
            reflection.GetComponent<SpriteRenderer>().color=color;
        }

        public Rect CameraBounds(int room)
        {
            if(room==Entrance)return Rect.MinMaxRect(-16.5f,-10.5f,StreetWidth-16.5f,10.5f);
            if(IsGroundHall(room))return Rect.MinMaxRect(Cell(room,2,0).x,-10,Cell(room,62,0).x,13);
            if(IsStairwell(room))return Rect.MinMaxRect(Cell(room,2,0).x,-9,Cell(room,15,0).x,9);
            if(IsLiftCabin(room))return Rect.MinMaxRect(Cell(room,4,0).x,-8,Cell(room,18,0).x,6);
            if(IsUpperInterior(room))return UpperInteriorBounds(room);
            if(IsInterior(room))return Rect.MinMaxRect(Cell(room,1,0).x,-9,Cell(room,21,0).x,5);
            var o=Offset(room);return Rect.MinMaxRect(o.x-16,-10,o.x+15,9);
        }
        void BuildGroundHall(int room)
        {
            var area=new HashSet<Vector2Int>();
            void Fill(int x0,int x1,int y0,int y1){for(int x=x0;x<=x1;x++)for(int y=y0;y<=y1;y++)area.Add(new Vector2Int(x,y));}
            Fill(5,61,13,18);Fill(55,61,2,12);Fill(53,54,11,12);
            Fill(28,32,18,21);Fill(34,38,18,21);Fill(3,9,16,20);
            if(room==Ground)Fill(53,55,19,19);
            else AddDarkGroundDetour(area);
            var openings=new HashSet<Vector2Int>();
            void Opening(Vector2 contact, int direction)
            {
                openings.Add(Vector2Int.RoundToInt(contact-Cell(room,0,0))+new Vector2Int(0,direction));
            }
            for(int i=FirstGroundDoor;i<GroundDoors.Length;i++)Opening(DoorPoint(room,i),GroundDoors[i].South?-1:1);
            AddUpperDoorOpenings(room,openings);
            Opening(Lift(room),1);Opening(Exit(room),-1);
            if(IsDark(room))Opening(GalleryEntrance,1);
            foreach(int core in new[]{0,1})
                for(int x=-1;x<=1;x++)Opening(Stairs(room,core)+Vector2.right*x,1);
            GroundSurface(room,area,"terrazo","pb-muro-yeso",cell=>
                IsDark(room)&&cell.x>=32&&cell.x<=41&&cell.y<=12?"rejilla":
                cell.x>=55&&cell.y<=6?"oficina":
                cell.x>=38&&cell.x<=40?"metal":null,openings);
            PlaceGroundBend(room);
            for(int i=FirstGroundDoor;i<GroundDoors.Length;i++){
                var door=GroundDoors[i];var p=DoorPoint(room,i);
                var boundary=new Vector2Int(door.X,door.Y+(door.South?-1:1));
                PlaceWallDoor(room,p,door.Name,DoorMaterial.Glass,door.South,HallWall(room,boundary));
            }
            if(room==Ground){
                var window=ReceptionContact+Vector2.up;
                PlaceSpeaker(room,"pb-recepcionista","Recepcionista",window);
                rooms[room].Obstacles.Add(new Rect(Cell(room,53,18)-Vector2.one*.5f,new Vector2(3,2)));
                Art(room,"pb-ventanilla",window+Vector2.up*.32f,new Vector2(3.1f,1.65f),"Ventanilla de recepción",CampusLook.Depth(window.y)+3);
                MountPlaque(room,"Recepción",window+Vector2.up*.98f,.22f,CampusLook.Depth(window.y)+4);
            }
            PlaceLift(room);
            for(int core=0;core<StairwellCount;core++)PlaceWallStair(room,Stairs(room,core),StairName(core));
            if(room==Ground){BuildSecurity();PopulateHall();}
            else for(int i=0;i<3;i++)Prop(room,"pb-molinete",37,14+i,1.65f,1.02f);
            Prop(room,"pb-listones",49,18,3.5f,1.55f,false);
            Prop(room,"tour-jardinera",60,10,1.6f,2.1f);
            Prop(room,"tour-jardinera",59,17,1.55f,1.53f);
            Prop(room,"campus-prop-tablero-avisos",46,18,2.6f,1.25f,false);
            Prop(room,"campus-prop-extintor",33,18,.6f,1.1f,false);
            Color hallLight = IsDark(room)?new Color(1,.42f,.38f):Color.white;
            for(int x=10;x<=56;x+=7)
                FloorLight(room,x,16,new Vector2(10,5),hallLight);
            FloorLight(room,58,10,new Vector2(5,11),hallLight);
            for(int y=4;y<=12;y+=3)Prop(room,"tour-marca-piso",57,y,.65f,.65f,false);
            PlaceWallDoor(room,Exit(room),"Salida",DoorMaterial.Glass,true,"pb-muro-vidrio");
            if(IsDark(room)){
                Prop(room,"bloque-a-tuberia",36,9,5.1f,1.14f,false);
                Prop(room,"bloque-a-cable-caido",39,10,2.25f,.78f,false);
                FloorLight(room,34,10,new Vector2(4.7f,3.4f),new Color(.35f,.79f,.72f,.58f));
                FloorLight(room,40,11,new Vector2(4.7f,3.4f),new Color(.77f,.29f,.39f,.58f));
                Prop(room,"bloque-a-cable-caido",33,17,1.8f,1.2f,false);
                Prop(room,"bloque-a-alarma",47,17,1,1.2f,false);
                PlaceWallDoor(room,GalleryEntrance,"Galería de servicio",DoorMaterial.Metal);
            }
            BuildUpperDoors(room);
        }
        void GroundSurface(int room,HashSet<Vector2Int> area,string material,string wall,
            System.Func<Vector2Int,string> selectAccent=null,HashSet<Vector2Int> openings=null)
        {
            rooms[room].Cells=area;
            var map=CreateSurfaceMap(room);
            var tile=TileFor(room,"escena-suelo-"+material);
            var north=new HashSet<Vector2Int>();var south=new HashSet<Vector2Int>();var side=new HashSet<Vector2Int>();
            foreach(var cell in area){
                var accent=selectAccent?.Invoke(cell);
                PaintSurface(map,cell.x,cell.y,accent==null?tile:TileFor(room,"escena-suelo-"+accent));
                if(wall==null)continue;
                if(!area.Contains(cell+Vector2Int.up))north.Add(cell+Vector2Int.up);
                if(!area.Contains(cell+Vector2Int.down))south.Add(cell+Vector2Int.down);
                var left=cell+Vector2Int.left;var right=cell+Vector2Int.right;
                if(!area.Contains(left))side.Add(left);
                if(!area.Contains(right))side.Add(right);
            }
            if(wall==null)return;
            if(!IsGroundHall(room)){
                var perimeter=new HashSet<Vector2Int>(north);
                perimeter.UnionWith(south);
                perimeter.UnionWith(side);
                foreach(var cell in area){
                    for(int x=-1;x<=1;x+=2)
                        for(int y=-1;y<=1;y+=2){
                            var corner=cell+new Vector2Int(x,y);
                            if(!area.Contains(cell+new Vector2Int(x,0))&&!area.Contains(cell+new Vector2Int(0,y))&&
                                !area.Contains(corner))perimeter.Add(corner);
                        }
                }
                foreach(var cell in perimeter){
                    if(openings!=null&&openings.Contains(cell)&&!(IsUpperInterior(room)&&Floor(room)>0))continue;
                    bool low=south.Contains(cell)||!side.Contains(cell)&&!north.Contains(cell)&&
                        (south.Contains(cell+Vector2Int.left)||south.Contains(cell+Vector2Int.right));
                    PlaceJoinedWall(room,cell,perimeter,wall,low);
                }
                return;
            }
            void Wall(Vector2Int cell,bool low)
            {
                if(openings!=null&&openings.Contains(cell))return;
                if(IsGroundHall(room)&&BendReplacesWall(cell))return;
                var p=Cell(room,cell.x,cell.y);
                Art(room,HallWall(room,cell),
                    p+Vector2.up*(low?.02f:.25f),new Vector2(1,low?.45f:1.42f),"Borde",CampusLook.Depth(p.y)+(low?-14:0));
            }
            foreach(var cell in side)if(!north.Contains(cell)&&!south.Contains(cell))Wall(cell,false);
            foreach(var cell in north)Wall(cell,false);
            foreach(var cell in south)Wall(cell,true);
        }

        static bool BendReplacesWall(Vector2Int cell)=>
            cell.y==19&&cell.x>=58||cell.x==62&&cell.y>=15||
            cell.x==52&&cell.y>=11&&cell.y<=12||cell.y==10&&cell.x>=53&&cell.x<=54;

        void PlaceGroundBend(int room)
        {
            string prefix=IsDark(room)?"backfranz-":"";
            Art(room,prefix+"giro-pb-vidrio",Cell(room,60,17)+Vector2.up*.5f,new Vector2(5,6),"Vidrio curvo de entrada",-860);
            Art(room,prefix+"giro-pb-zocalo",Cell(room,53,11)+new Vector2(.5f,.5f),new Vector2(4,4),"Zócalo curvo de entrada",-860);
        }

        static bool GroundBendContains(Vector2 point)
        {
            if(point.x>57.5f&&point.y>14.5f&&
                (point-new Vector2(57.5f,14.5f)).sqrMagnitude>16f)return false;
            if(point.x<=52.5f)return true;
            if(point.x>=54.5f||point.y>=12.5f)return true;
            if(point.y<=10.5f)return false;
            return (point-new Vector2(52.5f,10.5f)).sqrMagnitude>=4f;
        }
        void BuildGroundInterior(int room)
        {
            int index=InteriorIndex(room);
            var area=new HashSet<Vector2Int>();
            for(int x=2;x<=20;x++)for(int y=3;y<=13;y++){
                if(index==1&&(x==2||x>=19&&y>=10))continue;
                if(index==2&&x>=17&&y<=6)continue;
                if(index==3&&x>=18&&y<=5&&x-y>13)continue;
                if(index==4&&x<=5&&y>=12)continue;
                if(index==6&&(x<=4&&y>=12||x>=19&&y<=5))continue;
                area.Add(new Vector2Int(x,y));
            }
            string floor=groundMaterials[index];
            string wall=index==3?"pb-muro-acustico":index==4?"tour-muro-estudio":"pb-muro-yeso";
            var exit=InteriorExit(room);
            GroundSurface(room,area,floor,wall,cell=>index switch{
                1 when cell.x<=10&&cell.y>=11=>"oficina",
                2 when cell.x>=14&&cell.y>=8=>"roble",
                3 when cell.y>=11=>"roble",
                4 when cell.x<=8&&cell.y<=8=>"acustico",
                5 when cell.y>=11=>"cocina",
                6 when cell.x>=13&&cell.y>=6&&cell.y<=10=>"roble",
                _=>null
            },WallOpening(room,exit,true));
            Color light = IsDark(room)?new Color(.9f,.35f,.4f):index==4?new Color(.8f,.48f,1):Color.white;
            foreach(int x in new[]{6,11,17})
                FloorLight(room,x,10,new Vector2(7,8),light);
            PlaceWallDoor(room,exit,"Salida",DoorMaterial.Glass,true,wall);
            void P(string asset,int x,int y,float width=1.8f,float height=1.6f,bool solid=true)=>Prop(room,asset,x,y,width,height,solid);
            switch(index){
                case 1:
                    P("pb-caja-corrida",8,11,7.3f,2.25f);
                    PlaceRoomDetail(room,"lector",7,11);PlaceRoomDetail(room,"lector",9,11);
                    PlaceCampusProp(room,"sello-caja",10,11,.68f,.64f,.86f);
                    P("mob-estante-archivo",4,12,1.9f,2.1f);
                    P("bloque-a-maquina-ticket",16,10,1.45f,1.9f);
                    P("tour-banca-metal",16,6,3.4f,1.35f);
                    P("campus-prop-tablero-avisos",15,12,2.75f,1.35f,false);
                    var line=Art(room,"solid",Cell(room,7,8),new Vector2(5.2f,.1f),"Línea de espera",-875).GetComponent<SpriteRenderer>();
                    line.color=IsDark(room)?new Color(.58f,.44f,.49f):new Color(.82f,.60f,.29f);
                    break;
                case 2:
                    P("mob-mesa-grupo",7,8,4.1f,2.1f);
                    PlaceRoomDetail(room,"cuaderno",7,8);
                    P("mob-silla-visita",6,6,1.25f,1.35f);P("mob-silla-visita",8,6,1.25f,1.35f);
                    P("pb-servicios",16,10,4.2f,2.05f);P("mob-silla-visita",16,8,1.35f,1.4f);
                    P("mob-estante-exhibidor",5,12,2.8f,2.05f);P("campus-prop-tablero-avisos",8,12,2.4f,1.6f,false);
                    P("utileria-revistero",10,12,1.35f,1.5f);
                    P("tour-jardinera",18,12,2.1f,1.55f);break;
                case 3:
                    P("pb-pantalla",11,13,8.4f,2.4f,false);P("tour-atril-auditorio",16,12,2,1.7f);
                    PlaceRoomDetail(room,"carpetas",16,12,.66f,.44f,.75f);
                    P("utileria-consola-auditorio",5,12,1.5f,1.11f);
                    foreach(int y in new[]{6,8,10})foreach(int x in new[]{5,7,9,13,15,17})
                        P(x==5||x==17?"mob-butaca-lateral":"mob-butaca-auditorio",x,y,1.25f,1.3f);
                    P("campus-prop-extintor",3,11,.7f,1.22f,false);
                    break;
                case 4:
                    P("bloque-a-arcade",7,12,1.8f,2.3f);P("bloque-a-arcade",10,12,1.8f,2.3f);
                    P("mob-sofa-modular",5,6,3.7f,2.1f);P("mob-mesa-auxiliar",5,8,1.45f,1.25f);
                    PlaceRoomDetail(room,"taza",5,8,.66f,.42f,.41f);
                    PlaceCampusProp(room,"control-juegos",6,8,.7f,.45f,.57f);
                    P("pb-futbolin",15,8,3.7f,2.3f);P("mob-banca-pasillo",16,12,3.5f,1.2f);
                    P("pb-neon",8,13,3.3f,1.7f,false);
                    if(!IsDark(room))FloorLight(room,5,11,new Vector2(5,5),new Color(.63f,.38f,1,.75f));
                    break;
                case 5:
                    P("campus-prop-barra-cafeteria",11,12,5.5f,2);P("campus-prop-vitrina-cafeteria",5,12,2.7f,1.8f);
                    foreach(var pt in new[]{new Vector2Int(6,6),new Vector2Int(8,9),new Vector2Int(16,7),new Vector2Int(17,10)})
                        P(pt.x<10?"mob-mesa-cafe-cuadrada":"mob-mesa-cafe-redonda",pt.x,pt.y,3.25f,2.0f);
                    PlaceRoomDetail(room,"taza",6,6,.61f,.41f,.72f);PlaceRoomDetail(room,"taza",16,7,.61f,.41f,.72f);
                    PlaceRoomDetail(room,"bandeja-gastro",17,10,1.05f,.54f,.72f);
                    PlaceCampusProp(room,"servilletero",8,9,.65f,.62f,.88f);
                    break;
                case 6:
                    P("campus-prop-mostrador-orientacion",16,8,5.2f,2.35f);
                    PlaceRoomDetail(room,"carpetas",15,8,.94f,.58f,.88f);
                    PlaceCampusProp(room,"folletos-entrada",17,8,.83f,.76f,.93f);
                    P("mob-sofa-modular",5,7,3.8f,2.05f);P("mob-mesa-auxiliar",7,5,1.5f,1.2f);
                    P("mob-estante-exhibidor",6,11,3.15f,2.3f);P("campus-prop-tablero-avisos",9,12,2.15f,1.65f,false);
                    P("tour-ventanal",15,13,6.2f,1.6f,false);P("tour-jardinera",18,11,2.15f,1.65f);break;
            }
            var descriptions=new[]{"Orientación del primer día","Caja y turnos","Apoyo, orientación y vida universitaria","Charlas y proyecciones","Consolas y juegos del campus","Barra y mesas para reunirse","Información de ingreso y trámites"};
            var stop=InteriorExit(room)+Vector2.up*2;
            rooms[room].Stops.Add(new Stop(stop,IsDark(room)?"Backfranz · "+GroundDoors[index].Name:descriptions[index]));
            if(IsDark(room)){P("bloque-a-charco",13,6,3,1.35f,false);P("bloque-a-alarma",4,12,1,1.2f,false);}
            else PopulateGroundInterior(room,index);
        }
        static readonly string[] groundMaterials={"terrazo","terrazo","linoleo","auditorio","juegos","cafe","oficina"};
        public Vector2 InteriorExit(int room)=>Cell(room,11,3);
    }
}
