using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow : MonoBehaviour
    {
        public enum HorrorState { Normal, Pending, Intro, Active }
        public HorrorState Horror {get;private set;}
        [System.Serializable]
        sealed class DescentPreview
        {
            public float holdSeconds = .45f;
            public string notice = "El Bloque A ha cambiado";
        }
        DescentPreview descentPreview = new DescentPreview();
        bool heardUnattendedReader, heardServiceKnock;
        [SerializeField] bool enableLegacyPuzzleRoute;
        public BackfranzAudio Sound=>pause.sound;
        public CampusWorld World {get;private set;}
        public ExplorerMotor Player {get;private set;}
        public int Room {get;private set;}
        public bool Finished {get;private set;}
        public bool Won {get;private set;}
        public bool HasCard {get;private set;}
        public bool HasSchedule {get;private set;}
        public bool HasFuse {get;private set;}
        public bool HasKey {get;private set;}
        public bool HasBadge {get;private set;}
        public bool CircuitSolved {get;private set;}
        public int CircuitProgress {get;private set;}
        public int Visited {get;private set;}
        public bool LiftOpen=>liftPanel && liftPanel.activeSelf;
        public bool Crossing {get;private set;}
        public bool InductionComplete=>HasCard&&HasSchedule&&Visited==7;
        readonly bool[] visits=new bool[7];
        static readonly int[] Sequence={1,0,2};
        BackfranzUI pause;
        CampusAtmosphere atmosphere;
        Text objective,location,hint,endingKicker,endingTitle,endingStory,endingStats,retryLabel,liftTitle,checklist;
        Image vignette,fade,endingCover,endingAccent;
        GameObject ending,touch,hintPanel,liftPanel,statusPanel;
        RectTransform hudControls;
        CanvasGroup controlsVisibility,missionVisibility;
        bool missionPinned;
        float missionUntil;
        static readonly Color HintColor=new Color(.96f,.91f,.76f);
        static readonly Color PickupHintColor=new Color(1f,.81f,.47f);
        Button firstLiftButton;
        int liftOrigin;
        readonly Button[] floorButtons=new Button[7];
        float noticeUntil;
        Font font;
        string notice="";
        public bool HasVisited(int floor)=>visits[floor];
        internal string PauseLocation=>Title(Room)+"\n"+(IsDark(Room)?"Bloque A alterado":"Inducción")+"  ·  "+PlayDuration(playingSeconds);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
        static void Loaded(Scene scene,LoadSceneMode mode)
        {if(scene.name=="Nivel01")new GameObject("Backfranz · partida").AddComponent<SessionFlow>();}
        void Start()
        {
            var settings=Resources.Load<TextAsset>("Backfranz/DescentPreview");
            if(settings){
                try { JsonUtility.FromJsonOverwrite(settings.text,descentPreview); }
                catch(System.ArgumentException){Debug.LogWarning("Configuración inválida del descenso provisional");}
            }
            pause=FindFirstObjectByType<BackfranzUI>();font=pause.status.font;
            foreach(var root in gameObject.scene.GetRootGameObjects())
                if(root!=gameObject&&!root.GetComponentInChildren<Camera>()&&!root.GetComponentInChildren<Canvas>()&&!root.GetComponentInChildren<EventSystem>())root.SetActive(false);
            foreach(var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))renderer.enabled=false;
            foreach(var motor in FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None))motor.enabled=false;
            var camera=Camera.main;
            camera.orthographic=true;
            camera.allowMSAA=false;
            camera.allowHDR=false;
            camera.allowDynamicResolution=false;
            World=gameObject.AddComponent<CampusWorld>();atmosphere=gameObject.AddComponent<CampusAtmosphere>();
            World.ShowLegacyPuzzleProps=enableLegacyPuzzleRoute;
            var actor=CampusLook.Art(transform,"yamil-00",World.Entry(Entrance),new Vector2(.8f,1.2f),"Yamil");
            Player=actor.AddComponent<ExplorerMotor>();Player.world=World;Player.session=this;
            CreateHUD();
            if (!RestoreRequestedSave())
            {
                Enter(Entrance);
                BeginTutorial();
                OpenDialogue(ArrivalDialogue);
            }
        }
        void Enter(int room,bool byLift=false,Vector2? arrival=null)
        {
            int previous=Room;
            Room=room;World.Show(room);ShowEscapeContacts();RefreshAcousticRoom();Player.TouchAxis(Vector2.zero);
            ShowInductionContacts();
            ShowAuditoriumTool();
            ShowCampusRadio();
            EnterClassrooms();
            EnterWorkshop();
            Player.transform.position=arrival??(byLift?World.Lift(room)+Vector2.down*2:World.Entry(room));
            EnterEscortRoom(previous);
            EnterGalleryWatch();
            noticeUntil=0;
            EnterAuditoriumRescue();
            EnterServices();
            AlterReturningHall();
            if(room==DarkGround)
            {
                World.ShowUnattendedReader(heardUnattendedReader);
                World.SetGalleryAccess(GalleryReady);
            }
            atmosphere.SetRoom(room,Sound.AmbientGroup);
            Sound.SetCampusMusic(IsDark(room));
            if(room==ForFloor(3,true))World.ShowBreakers(CircuitProgress,CircuitSolved);
            vignette.color=new Color(1,1,1,IsDark(room)?1:.15f);SnapCamera();RefreshHUD();
        }
        bool Near(Vector2 p,float radius=1.24f)=>Vector2.Distance(Player.Position,p)<radius&&World.HasLineOfSight(Player.Position,p);
        public void Interact()
        {
            if(Finished||Time.timeScale==0||Crossing)return;
            if(DialogueOpen){AdvanceDialogue();return;}
            if(InteractionPrompt().Length>0)Player.Reach(Player.Facing);
            if(TryEscapeInteraction(true,out _))return;
            if(TryInductionInteraction(true,out _))return;
            if(TryAuditoriumTool(true,out _))return;
            if(TryCampusRadio(true,out _))return;
            if(TryClassroomChalk(true,out _))return;
            if(TryWorkshopCable(true,out _))return;
            if(TryTalk())return;
            if(Room==Entrance){
                if(Near(World.StreetDoor,1.45f))UseDoor(World.StreetDoor,Ground,World.Entry(Ground));
                return;
            }
            if(IsLiftCabin(Room)){
                if(Near(World.CabinPanel(Room),1.02f))OpenLift();
                else if(Near(World.CabinExit(Room),1.02f))StartCoroutine(Travel(liftOrigin,false,World.Lift(liftOrigin)+Vector2.down));
                return;
            }
            if(TryUseStairs()||IsStairwell(Room))return;
            if(IsUpperInterior(Room)){
                if(Near(World.UpperInteriorExit(Room),1.05f))
                    UseDoor(World.UpperInteriorExit(Room),World.UpperInteriorHall(Room),World.UpperInteriorDoor(Room));
                return;
            }
            foreach(int interior in World.UpperInteriorsOn(Room))
                if(Near(World.UpperInteriorDoor(interior),.84f)){
                    if(World.UpperDoorLocked(interior,SignalsComplete)){Notify("La puerta está cerrada");return;}
                    UseDoor(World.UpperInteriorDoor(interior),interior,World.Entry(interior));
                    return;
                }
            if(AtAuditoriumServiceDoor())
            {
                bool dark=IsDark(Room);
                int destination=IsGroundHall(Room)?InteriorRoom(AuditoriumIndex,dark):ForFloor(0,dark);
                var arrival=World.AuditoriumServiceDoor(destination)+Vector2.up*(IsGroundHall(destination)?1:-1);
                if(dark&&auditoriumWedged)StartCoroutine(Travel(destination,false,arrival));
                else UseDoor(World.AuditoriumServiceDoor(Room),destination,arrival);
                return;
            }
            if(IsInterior(Room)&&Near(World.InteriorExit(Room),1.3f)){
                int hall=IsDark(Room)?DarkGround:Ground,index=InteriorIndex(Room);
                UseDoor(World.InteriorExit(Room),hall,World.DoorPoint(hall,index)+Vector2.up*(GroundDoors[index].South?1:-1));return;
            }
            if(IsGroundHall(Room)){
                for(int i=FirstGroundDoor;i<GroundDoors.Length;i++)if(Near(World.DoorPoint(Room,i),1.3f)){
                    int interior=InteriorRoom(i,IsDark(Room));
                    UseDoor(World.DoorPoint(Room,i),interior,World.Entry(interior));return;
                }
                int lane=Room==Ground?World.SecurityLane(Player.Position):-1;
                if(lane>=0){BeginSecurityAccess(lane,false);return;}
                if(IsDark(Room)&&(Near(World.Stairs(Room),1.1f)||Near(World.Stairs(Room,1),1.1f))){Notify("Gradas bloqueadas · usa el ascensor");return;}
            }
            if(!IsDark(Room)){
                if(Room==Ground&&Near(World.Exit(Room),1.55f)){
                    UseDoor(World.Exit(Room),Entrance,World.StreetReturn);return;
                }
            }else if(Room==Basement){
                if(!HasFuse&&Near(World.Fuse)){HasFuse=true;World.TakeFuse();Sound.PickUp();Notify("Recogiste el fusible");return;}
                if(Near(World.PowerPanel)){
                    if(!HasFuse){Notify("Falta un fusible en el cuadro");return;}
                    if(!World.Power)
                    {
                        World.RestorePower();
                        RefreshMaintenanceReadings();
                        Sound.MechanismReleased();
                    }
                    Confirm("Energía de emergencia restablecida · la reja ya está abierta");return;
                }
            }else if(enableLegacyPuzzleRoute){
                int floor=Floor(Room);
                if(floor==1&&Near(World.Key)){
                    if(!HasKey){HasKey=true;World.TakeKey();}
                    Confirm("Llave técnica · orden: rojo, azul, verde",5);return;
                }
                if(floor==3)for(int i=0;i<3;i++)if(Near(World.Breaker(i))){UseBreaker(i);return;}
                if(floor==5&&Near(World.Badge)){
                    if(!CircuitSolved){Notify("La vitrina necesita el circuito del piso 3");return;}
                    if(!HasBadge){HasBadge=true;World.TakeBadge();Confirm("Pase de salida · vuelve a planta baja");}
                    return;
                }
                if(Room==DarkGround&&Near(World.Exit(Room),1.55f)){
                    if(!HasBadge){Notify("La puerta requiere el pase de salida");return;}
                    Finish(true);return;
                }
            }
            if(!IsInterior(Room)&&Near(World.Lift(Room),1.55f)){CallLift();return;}
            if(TryTalkToCampusPerson())return;
            foreach(var stop in World.Stops(Room))if(Near(stop.Position)){Confirm(stop.Label,3.4f);return;}
        }
        void UseBreaker(int index)
        {
            if(CircuitSolved){Notify("Circuito estable");return;}
            if(!HasKey){Notify("Falta la llave técnica del piso 1");return;}
            if(Sequence[CircuitProgress]!=index){CircuitProgress=0;World.ShowBreakers(0,false);Confirm("Orden incorrecto. El tablero se reinició");return;}
            CircuitProgress++;
            CircuitSolved=CircuitProgress==Sequence.Length;
            World.ShowBreakers(CircuitProgress,CircuitSolved);
            Confirm(CircuitSolved?"Circuito activo · revisa la vitrina del piso 5":"Interruptor correcto · "+CircuitProgress+"/3");
        }
        void Confirm(string text,float seconds=2.8f){Sound.Confirm();Notify(text,seconds);}
        void Notify(string text,float seconds=2.8f){notice=text;noticeUntil=Time.unscaledTime+seconds;}
        void CallLift()
        {
            if(Room==Basement&&!World.Power){Notify("Ascensor sin energía");return;}
            if(!IsDark(Room)&&(!HasCard||!HasSchedule)){Notify("Primero recoge la credencial y el horario");return;}
            liftOrigin=Room;
            int cabin=LiftCabin(IsDark(Room));
            Sound.Lift();
            Sound.LiftDoors(true);
            Enter(cabin,false,World.CabinExit(cabin)+Vector2.up);
            World.SetLiftDoors(cabin,1f);
        }

        void OpenLift()
        {
            pause.sound.Click();pause.enabled=false;pause.pauseButton.SetActive(false);Player.TouchAxis(Vector2.zero);Time.timeScale=0;liftPanel.SetActive(true);
            bool dark=IsDark(Room);
            liftTitle.text=dark?"Ascensor de emergencia":"Bloque A · Ascensor";
            var progress=new List<string>();
            for(int i=0;i<floorButtons.Length;i++){
                floorButtons[i].interactable=liftOrigin!=ForFloor(i,dark);
                floorButtons[i].GetComponentInChildren<Text>().text=(dark?"":visits[i]?"✓ ":"• ")+Floors[i];
                if(!dark&&!visits[i])progress.Add(i==0?"PB":i==6?"Biblioteca":i.ToString());
            }
            checklist.text=dark?"Bloque A alterado":
                progress.Count==0?"Inducción completa":"Constancias pendientes · "+string.Join("  /  ",progress);
            basementButton.gameObject.SetActive(dark&&InvestigationComplete&&liftOrigin!=Basement);
            var focus=firstLiftButton;foreach(var button in floorButtons)if(button.interactable){focus=button;break;}
            EventSystem.current.SetSelectedGameObject(focus.gameObject);
        }
        public void CloseLift()
        {
            if(!LiftOpen)return;liftPanel.SetActive(false);Time.timeScale=1;pause.enabled=true;pause.pauseButton.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
        }
        public void ChooseFloor(int floor)
        {
            if(!LiftOpen||floor<0||floor>=Floors.Length)return;
            bool shift=liftOrigin==Top&&floor<Floor(Top)&&Horror==HorrorState.Normal&&InductionComplete;
            int next=ForFloor(floor,shift||IsDark(Room));if(next==liftOrigin)return;
            CloseLift();
            if(shift)Horror=HorrorState.Pending;
            StartCoroutine(Travel(next,shift));
        }
        void UseDoor(Vector2 contact,int destination,Vector2 arrival)
        {
            StartCoroutine(Travel(destination,false,arrival,World.DoorAt(Room,contact)));
        }

        IEnumerator Travel(int room,bool shift,Vector2? arrival=null,FramedDoor door=null)
        {
            Crossing=true;pause.enabled=false;pause.pauseButton.SetActive(false);Player.TouchAxis(Vector2.zero);Time.timeScale=0;
            bool fromCabin=IsLiftCabin(Room);
            if(fromCabin){
                Sound.LiftDoors(false);
                for(float time=0;time<.32f;time+=Time.unscaledDeltaTime){
                    World.SetLiftDoors(Room,1f-time/.32f);
                    yield return null;
                }
                World.SetLiftDoors(Room,0f);
                if(shift||room!=liftOrigin)Sound.LiftMotor();
            }
            if(door!=null){
                Player.Reach(door.Facing);
                if(door.Material==DoorMaterial.Metal||IsDark(Room)||IsDark(room)) Sound.Door(true);
                else if(door.Material==DoorMaterial.Wood) Sound.WoodDoor();
                else Sound.Door();
                const float openingSeconds=.32f;
                for(float time=0;time<openingSeconds;time+=Time.unscaledDeltaTime){
                    door.SetOpening(Mathf.SmoothStep(0,1,time/openingSeconds));
                    yield return null;
                }
                door.SetOpening(1);
            }
            fade.gameObject.SetActive(true);float elapsed=0;
            while(elapsed<.22f){elapsed+=Time.unscaledDeltaTime;fade.color=new Color(.015f,.019f,.035f,Mathf.Clamp01(elapsed/.22f));yield return null;}
            fade.color=new Color(.015f,.019f,.035f,1);
            if(shift){
                Horror=HorrorState.Intro;
                Sound.Thunder();
                yield return new WaitForSecondsRealtime(Mathf.Max(0,descentPreview.holdSeconds));
                Horror=HorrorState.Active;
            }
            else if(door==null)pause.sound.Click();
            Enter(room,true,arrival);
            if(fromCabin)World.SetLiftDoors(room,0f);
            door?.SetOpening(0);
            if(shift&&!string.IsNullOrWhiteSpace(descentPreview.notice))Notify(descentPreview.notice,4);
            elapsed=0;
            while(elapsed<.28f){elapsed+=Time.unscaledDeltaTime;fade.color=new Color(.015f,.019f,.035f,1-Mathf.Clamp01(elapsed/.28f));yield return null;}
            if(fromCabin){
                Sound.LiftDoors(true);
                for(float time=0;time<.45f;time+=Time.unscaledDeltaTime){
                    World.SetLiftDoors(room,Mathf.Sin(Mathf.PI*time/.45f));
                    yield return null;
                }
                World.SetLiftDoors(room,0f);
            }
            fade.gameObject.SetActive(false);Crossing=false;Time.timeScale=1;pause.enabled=true;pause.pauseButton.SetActive(true);
            if(shift)OpenDialogue(new[]{
                new DialogueLine("Yamil","yamil-00","El ascensor llegó al piso que marqué. ¿Por qué ya no se oye a nadie?"),
                new DialogueLine("Yamil","yamil-00","Tengo que encontrar una salida. En la biblioteca tenían el plano de evacuación.")
            });
        }

        internal void ObserveFirstNightMovement(Vector2 position)
        {
            if(Room!=DarkGround||Horror!=HorrorState.Active||finaleStage!=0)return;
            if(!heardUnattendedReader&&(position-World.DarkReaderPosition).sqrMagnitude<9f&&
                World.HasLineOfSight(position,World.DarkReaderPosition,false)){
                heardUnattendedReader=true;
                World.ShowUnattendedReader(true);
                Sound.Scanner();
                Notify("Yamil · El lector se activó solo. Por aquí ya no se pasa.",4);
            }
            if(!heardServiceKnock&&World.InDarkServiceCorridor(position)){
                heardServiceKnock=true;
                Sound.DistantKnock();
                Notify("Yamil · Ese golpe vino de debajo de la rejilla.",4);
            }
        }
        void Update()
        {
            if(!Player)return;
            atmosphere.Quiet=Finished||DialogueOpen||(Time.timeScale==0&&!Crossing);
            if(!Finished&&(Time.timeScale>0||DialogueOpen||EscapePanelOpen||JournalOpen||LiftOpen||Crossing)){
                playingSeconds+=Time.unscaledDeltaTime;
                if(Horror==HorrorState.Active)escapeSeconds+=Time.unscaledDeltaTime;
            }
            if(EscapePanelOpen&&Input.GetKeyDown(KeyCode.Escape)){CloseEscapePanel();return;}
            if(JournalOpen&&Input.GetKeyDown(KeyCode.Escape)){CloseJournal();return;}
            if(BackpackOpen&&Input.GetKeyDown(KeyCode.Escape)){StartCoroutine(CloseBackpackAfterEscape());return;}
            if(BackpackOpen&&Input.GetKeyDown(KeyCode.I)){CloseBackpack();return;}
            UpdateDialogue();
            UpdateTutorial();
            UpdateFinale();
            if(LiftOpen&&Input.GetKeyDown(KeyCode.Escape)){CloseLift();return;}
            if(StairsOpen&&Input.GetKeyDown(KeyCode.Escape)){CloseStairMenu();return;}
            if(CanOpenBackpack&&Input.GetKeyDown(KeyCode.I))OpenBackpack();
            if(CanShowMission&&Input.GetKeyDown(KeyCode.M))ToggleMission();
            bool playing=!Finished&&!Crossing&&!LiftOpen&&!DialogueOpen&&!StairsOpen&&Time.timeScale>0;
            UpdateLibraryShadow(playing);
            UpdateGalleryWatch(playing);
            UpdateEscort(playing);
            UpdateAuditoriumRescue(playing);
            UpdateServices(playing);
            touch.SetActive(playing&&Application.isMobilePlatform);RefreshHUD();
            if(playing)World.UpdatePeople(Room,Player.Position,Time.deltaTime,Time.time);
            if(Room==Ground&&Time.timeScale>0)World.UpdateSecurityGates(Player.Position,Time.deltaTime);
        }
        void LateUpdate(){if(Player&&!Finished)SnapCamera();}
        void SnapCamera()
        {
            var camera=Camera.main;
            int height=Mathf.Max(1,camera.pixelHeight);
            float targetHeight=Room==Entrance?14f:IsLiftCabin(Room)?9f:IsInterior(Room)&&!IsUpperInterior(Room)?15.6f:11.6f;
            int scale=Mathf.Max(1,Mathf.RoundToInt(height/(targetHeight*16f)));
            camera.orthographicSize=height/(32f*scale);
            Rect bounds=World.CameraBounds(Room);float halfWidth=camera.orthographicSize*camera.aspect;
            float x=halfWidth*2>=bounds.width?bounds.center.x:Mathf.Clamp(Player.Position.x,bounds.xMin+halfWidth,bounds.xMax-halfWidth);
            float y=IsLiftCabin(Room)||camera.orthographicSize*2>=bounds.height?bounds.center.y:
                Mathf.Clamp(Player.Position.y,bounds.yMin+camera.orthographicSize,bounds.yMax-camera.orthographicSize);
            int pixelsPerUnit=16*scale;
            // On an odd-sized viewport, its center falls half a pixel from the tile edges.
            float phaseX=(camera.pixelWidth%2)*.5f;
            float phaseY=(height%2)*.5f;
            camera.transform.position=new Vector3(
                (Mathf.Round(x*pixelsPerUnit-phaseX)+phaseX)/pixelsPerUnit,
                (Mathf.Round(y*pixelsPerUnit-phaseY)+phaseY)/pixelsPerUnit,-10);
        }
        string Objective()
        {
            if(Room==Entrance)return HasSchedule
                ?"Tengo que volver al Bloque A por las gradas de Landaeta."
                :"Debería subir por las gradas de Landaeta junto a las tiendas y entrar al Bloque A.";
            if(IsStairwell(Room))return ThoughtOnFloor(Floor(stairDestination),"Tengo que cruzar el descansillo para salir");
            if(Room==ServiceGallery)return GalleryObjective();
            if(!IsDark(Room))return InductionObjective();
            if(Room==Basement&&!maintenanceApproved)return MaintenanceThought();
            return AuditoriumRescueThought()??HealthRescueThought()??ClassroomLoopThought()??WorkshopRescueThought()??EscapeObjective();
        }

        string ThoughtOnFloor(int floor,string thought)
        {
            if(!IsLiftCabin(Room)&&!IsStairwell(Room)&&Floor(Room)==floor)return thought+".";
            if(floor==0)return thought+" (planta baja).";
            return thought+" (piso "+floor+").";
        }

        string MaintenanceThought()
        {
            if(!HasFuse)return "Tengo que buscar un fusible en el almacén de repuestos, al fondo del pasillo oeste.";
            if(!World.Power)return "Ya tengo el fusible. Tengo que volver al cuadro eléctrico y abrir la reja.";
            if(maintenanceReadings!=AllPressureReadings)return "Tengo que revisar las tres lecturas de presión, junto a las tuberías de ingreso, retorno y purga.";
            return "Ya anoté las lecturas. Ahora puedo calibrar el regulador de emergencia.";
        }
        void RefreshHUD()
        {
            if(!hudControls)return;
            location.text=Title(Room);
            string thought=Objective();
            if(objective.text!=thought)
            {
                objective.text=thought;
                missionUntil=Time.unscaledTime+5f;
            }
            bool showControls=CanShowMission;
            controlsVisibility.alpha=showControls?1f:0f;
            controlsVisibility.blocksRaycasts=showControls;
            journalButton.SetActive(showControls&&Horror==HorrorState.Active);
            missionVisibility.alpha=showControls&&(missionPinned||Time.unscaledTime<missionUntil)?1f:0f;
            var prompt="";
            bool pickup=false;
            if(!Finished&&!LiftOpen&&!DialogueOpen&&Time.timeScale>0){
                if(Time.unscaledTime<noticeUntil)prompt=notice;
                else if(!Crossing)
                {
                    prompt=InteractionPrompt();
                    pickup=prompt.StartsWith("Recoger · ",System.StringComparison.Ordinal);
                    if(prompt.Length>0&&!Application.isMobilePlatform)prompt="E · "+prompt;
                }
            }
            hint.color=pickup?PickupHintColor:HintColor;
            hint.text=prompt;hintPanel.SetActive(prompt.Length>0);
        }

        bool CanShowMission=>!Finished&&!Crossing&&!LiftOpen&&!DialogueOpen&&!StairsOpen&&Time.timeScale>0;

        void ToggleMission()
        {
            if(!CanShowMission)return;
            if(missionPinned||Time.unscaledTime<missionUntil)
            {
                missionPinned=false;
                missionUntil=0f;
            }
            else missionPinned=true;
            RefreshHUD();
        }
        string InteractionPrompt()
        {
            if(TryEscapeInteraction(false,out string escapePrompt))return escapePrompt;
            if(TryInductionInteraction(false,out string inductionPrompt))return inductionPrompt;
            if(TryAuditoriumTool(false,out string toolPrompt))return toolPrompt;
            if(TryCampusRadio(false,out string radioPrompt))return radioPrompt;
            if(TryClassroomChalk(false,out string chalkPrompt))return chalkPrompt;
            if(TryWorkshopCable(false,out string cablePrompt))return cablePrompt;
            if(Room==Entrance&&Near(World.StudentContact,1.2f))return "Hablar · Estudiante";
            if(Room==Ground&&Near(World.ReceptionContact,1.1f))return "Hablar · Recepcionista";
            if(Room==Ground&&Near(World.GuardContact,.8f))return "Hablar · Guardia";
            if(Room==Entrance)return Near(World.StreetDoor,1.45f)?"Entrar al Bloque A":"";
            if(IsLiftCabin(Room)){
                if(Near(World.CabinPanel(Room),1.02f))return "Usar panel del ascensor";
                return Near(World.CabinExit(Room),1.02f)?"Salir de la cabina":"";
            }
            string stairs=StairPrompt();
            if(IsStairwell(Room)||stairs.Length>0)return stairs;
            if(IsUpperInterior(Room))return Near(World.UpperInteriorExit(Room),1.05f)?"Salir":"";
            foreach(int interior in World.UpperInteriorsOn(Room))
                if(Near(World.UpperInteriorDoor(interior),.84f))return World.UpperDoorLocked(interior,SignalsComplete)?"Revisar cerradura · "+CampusWorld.Title(interior):"Entrar · "+CampusWorld.Title(interior);
            if(AtAuditoriumServiceDoor())return IsGroundHall(Room)?"Entrar por servicio":"Salir al paso de servicio";
            if(IsInterior(Room)&&Near(World.InteriorExit(Room),1.3f))return "Salir";
            if(IsGroundHall(Room)){
                for(int i=FirstGroundDoor;i<GroundDoors.Length;i++)if(Near(World.DoorPoint(Room,i),1.3f))return "Entrar · "+GroundDoors[i].Name;
                if(IsDark(Room)&&(Near(World.Stairs(Room),1.1f)||Near(World.Stairs(Room,1),1.1f)))return "Revisar gradas";
                int lane=Room==Ground?World.SecurityLane(Player.Position):-1;
                if(lane>=0&&!World.SecurityOpen(lane))return lane==AccessibleLane?"Consultar paso accesible":"Escanear huella";
            }
            if(!IsDark(Room)){
                if(Room==Ground&&Near(World.Exit(Room),1.55f))return "Volver al exterior";
            }else if(Room==Basement){
                if(!HasFuse&&Near(World.Fuse))return "Recoger · Fusible";
                if(Near(World.PowerPanel))return World.Power?"Revisar el cuadro":"Reparar el cuadro eléctrico";
            }else if(enableLegacyPuzzleRoute){
                int floor=Floor(Room);
                if(floor==1&&Near(World.Key))return HasKey?"Leer orden del circuito":"Recoger · Llave técnica";
                if(floor==3)for(int i=0;i<3;i++)if(Near(World.Breaker(i)))return "Interruptor "+Colors[i];
                if(floor==5&&!HasBadge&&Near(World.Badge))return "Abrir la vitrina";
                if(Room==DarkGround&&Near(World.Exit(Room),1.55f))return "Abrir salida";
            }
            if(!IsInterior(Room)&&Near(World.Lift(Room),1.55f))return "Llamar al ascensor";
            var person=World.NearbyPerson(Room,Player.Position);
            if(person!=null)return "Hablar · "+person.Name;
            foreach(var stop in World.Stops(Room))if(Near(stop.Position))return stop.Label;
            return "";
        }
        bool AtAuditoriumServiceDoor()=> (IsGroundHall(Room)||Room==DarkAuditorium||Room==InteriorRoom(AuditoriumIndex,false))&&
            Near(World.AuditoriumServiceDoor(Room),1.05f);
        public void Finish(bool won)
        {
            if(Finished)return;Finished=true;Won=won;Time.timeScale=0;touch.SetActive(false);hintPanel.SetActive(false);pause.menu.SetActive(false);pause.pauseButton.SetActive(false);pause.enabled=false;
            var outcome=won?endingSeen:EscapeArchive.Ending.None;
            var accent=outcome switch
            {
                EscapeArchive.Ending.Amanecer=>new Color(1f,.73f,.44f),
                EscapeArchive.Ending.Eco=>new Color(.51f,.84f,.86f),
                EscapeArchive.Ending.Umbral=>new Color(.89f,.58f,.64f),
                _=>new Color(.88f,.64f,.54f)
            };
            Sound.EndScore(outcome);
            endingCover.sprite=CampusLook.Sprite(outcome==EscapeArchive.Ending.None?"final-bloqueado":EscapeArchive.Cover(outcome));
            endingKicker.text=won?"FINAL DESBLOQUEADO":"PUNTO DE CONTROL";
            endingKicker.color=endingAccent.color=accent;
            endingTitle.text=won?EscapeArchive.Title(outcome):"La sombra te encontró";
            endingStory.text=won?EscapeArchive.Story(outcome)+"\n\n"+RescueEpilogue():"Puedes volver al punto de control. Conservas tus objetos y el progreso.";
            endingStats.text=won?(outcome==EscapeArchive.Ending.Amanecer?"Personas a salvo · ":"Personas reunidas · ")+
                RescuedCount()+"/"+RescueTotal+"\nPartida · "+PlayDuration(playingSeconds)+"   Escape · "+PlayDuration(escapeSeconds):"";
            retryLabel.text=won?"Nueva partida":"Volver al ascensor";ending.SetActive(true);
            ending.transform.SetAsLastSibling();
            EventSystem.current.SetSelectedGameObject(retryLabel.transform.parent.gameObject);
        }
        public void Restart()
        {
            if(!Finished)return;
            Time.timeScale=1;if(Won){SceneManager.LoadScene("Nivel01");return;}
            Finished=false;Won=false;ending.SetActive(false);pause.enabled=true;pause.pauseButton.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);Enter(Room,Room!=Basement);Notify("Punto de control · conservaste tus objetos");
        }
        public void Menu(){Time.timeScale=1;SceneManager.LoadScene("MenuPrincipal");}
        void OnDestroy(){Time.timeScale=1;}

        void CreateHUD()
        {
            var root=new GameObject("Partida · HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform);
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=-1;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=1;
            vignette=Icon(root.transform,"vignette",Vector2.zero,Vector2.one);vignette.preserveAspect=false;
            finaleDarkness=Rect(root.transform,"Apagón",Vector2.zero,Vector2.one).gameObject.AddComponent<Image>();
            finaleDarkness.color=new Color(.015f,.024f,.056f,.40f);
            finaleDarkness.raycastTarget=false;
            finaleDarkness.gameObject.SetActive(false);
            var safe=Rect(root.transform,"Área segura",Vector2.zero,Vector2.one);
            var area=Screen.safeArea;safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
            var hud=Rect(safe,"Estado de la partida",Vector2.zero,Vector2.one);
            statusPanel=hud.gameObject;
            bool mobile=Application.isMobilePlatform;
            hudControls=Rect(hud,"Controles inferiores",
                mobile?new Vector2(.25f,.018f):new Vector2(.02f,.018f),
                mobile?new Vector2(.82f,.08f):new Vector2(.52f,.075f));
            controlsVisibility=hudControls.gameObject.AddComponent<CanvasGroup>();
            var areaName=Panel(hudControls,"Zona",Vector2.zero,new Vector2(.34f,1f));
            areaName.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            location=Label(areaName,"Zona",new Vector2(.06f,.10f),new Vector2(.94f,.90f),20);
            location.alignment=TextAnchor.MiddleLeft;
            var missionButton=Button(hudControls,mobile?"Misión":"Misión [M]",
                new Vector2(.36f,0f),new Vector2(.56f,1f),ToggleMission,20);
            missionButton.name="Consultar misión";
            var mission=Panel(hud,"Misión actual",
                new Vector2(mobile?.23f:.02f,.167f),new Vector2(mobile?.82f:.465f,.294f));
            mission.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            missionVisibility=mission.gameObject.AddComponent<CanvasGroup>();
            missionVisibility.interactable=false;
            missionVisibility.blocksRaycasts=false;
            missionVisibility.alpha=0f;
            var missionTitle=Label(mission,"Misión",new Vector2(.055f,.65f),new Vector2(.945f,.93f),18);
            missionTitle.text="YAMIL · PENSAMIENTO";
            missionTitle.alignment=TextAnchor.MiddleLeft;
            missionTitle.color=new Color(1f,.74f,.44f);
            objective=Label(mission,"Qué hacer",new Vector2(.055f,.09f),new Vector2(.945f,.65f),23);
            objective.alignment=TextAnchor.MiddleLeft;
            CreateGalleryWatch(safe);
            hintPanel=Rect(safe,"Interacción",new Vector2(.23f,.093f),new Vector2(.82f,.160f)).gameObject;
            hint=Label(hintPanel.transform,"Acción",Vector2.zero,Vector2.one,23);
            var contrast=hint.gameObject.AddComponent<UnityEngine.UI.Outline>();
            contrast.effectColor=new Color(.025f,.041f,.06f,.94f);
            contrast.effectDistance=new Vector2(2,-2);
            touch=Rect(safe,"Controles táctiles",Vector2.zero,Vector2.one).gameObject;
            Direction(new Vector2(.025f,.095f),Vector2.left,90);Direction(new Vector2(.155f,.095f),Vector2.right,-90);Direction(new Vector2(.09f,.20f),Vector2.up,0);Direction(new Vector2(.09f,.015f),Vector2.down,180);
            var interact=Button(touch.transform,"",new Vector2(.85f,.08f),new Vector2(.97f,.25f),Player.Interact);Icon(interact.transform,"interact",new Vector2(.15f,.15f),new Vector2(.85f,.85f));
            touch.SetActive(Application.isMobilePlatform);
            liftPanel=Panel(safe,"Ascensor",new Vector2(.18f,.12f),new Vector2(.82f,.88f)).gameObject;
            liftTitle=Label(liftPanel.transform,"Título",new Vector2(.05f,.84f),new Vector2(.95f,.96f),34);
            for(int i=0;i<floorButtons.Length;i++){
                int floor=i;int col=i%2,row=i/2;float x=.08f+.44f*col,y=.72f-.145f*row;
                floorButtons[i]=Button(liftPanel.transform,Floors[i],new Vector2(x,y),new Vector2(x+.40f,y+.105f),()=>ChooseFloor(floor),25);
            }
            firstLiftButton=floorButtons[0];
            basementButton=Button(liftPanel.transform,"-1 · Mantenimiento",new Vector2(.52f,.285f),new Vector2(.92f,.39f),ChooseBasement,25);
            checklist=Label(liftPanel.transform,"Progreso",new Vector2(.05f,.135f),new Vector2(.95f,.24f),22);
            Button(liftPanel.transform,"Cerrar",new Vector2(.34f,.035f),new Vector2(.66f,.135f),CloseLift,24);liftPanel.SetActive(false);
            CreateEnding(safe);
            CreateDialogue(safe);
            CreateStairMenu(safe);
            CreateEscapePanel(safe);
            CreateBackpack(safe);
            fade=Rect(root.transform,"Transición de ascensor",Vector2.zero,Vector2.one).gameObject.AddComponent<Image>();fade.gameObject.SetActive(false);
        }
        void CreateEnding(Transform safe)
        {
            var backdrop=Rect(safe,"Resultado",Vector2.zero,Vector2.one);
            backdrop.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.015f,.023f,.039f,.93f);
            ending=backdrop.gameObject;
            var frame=Rect(backdrop,"Tarjeta del final",new Vector2(.06f,.12f),new Vector2(.94f,.88f));
            frame.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.052f,.086f,.12f);
            endingAccent=Rect(frame,"Borde del final",new Vector2(.04f,.90f),new Vector2(.96f,.904f)).gameObject.AddComponent<UnityEngine.UI.Image>();
            endingAccent.raycastTarget=false;
            endingCover=Icon(frame,"final-bloqueado",new Vector2(.04f,.19f),new Vector2(.48f,.86f));
            endingCover.name="Ilustración del final";
            endingKicker=Label(frame,"Estado del final",new Vector2(.52f,.788f),new Vector2(.96f,.868f),23);
            endingTitle=Label(frame,"Título del final",new Vector2(.52f,.66f),new Vector2(.96f,.79f),44);
            endingStory=Label(frame,"Epílogo",new Vector2(.52f,.355f),new Vector2(.96f,.652f),26);
            endingStats=Label(frame,"Balance de partida",new Vector2(.52f,.19f),new Vector2(.96f,.345f),22);
            endingKicker.alignment=endingTitle.alignment=endingStats.alignment=TextAnchor.MiddleLeft;
            endingStory.alignment=TextAnchor.UpperLeft;
            endingStats.color=new Color(.70f,.79f,.81f);
            retryLabel=Button(frame,"Reintentar",new Vector2(.52f,.065f),new Vector2(.73f,.155f),Restart,23).GetComponentInChildren<UnityEngine.UI.Text>();
            Button(frame,"Menú principal",new Vector2(.75f,.065f),new Vector2(.96f,.155f),Menu,23);
            ending.SetActive(false);
        }
        void Direction(Vector2 anchor,Vector2 axis,float angle)
        {
            var button=Button(touch.transform,"",anchor,anchor+new Vector2(.06f,.095f),null);
            var icon=Icon(button.transform,"arrow-up",new Vector2(.18f,.18f),new Vector2(.82f,.82f));icon.rectTransform.localEulerAngles=new Vector3(0,0,angle);
            var direction=button.gameObject.AddComponent<TouchDirection>();direction.motor=Player;direction.direction=axis;
        }
        RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        RectTransform Panel(Transform parent,string name,Vector2 min,Vector2 max)
        {var r=Rect(parent,name,min,max);var image=r.gameObject.AddComponent<Image>();image.sprite=CampusLook.Sprite("panel");image.type=Image.Type.Sliced;return r;}
        Image Icon(Transform parent,string asset,Vector2 min,Vector2 max)
        {var r=Rect(parent,asset,min,max);var image=r.gameObject.AddComponent<Image>();image.sprite=CampusLook.Sprite(asset);image.preserveAspect=true;image.raycastTarget=false;return image;}
        Text Label(Transform parent,string name,Vector2 min,Vector2 max,int size)
        {
            var t=Rect(parent,name,min,max).gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.color=new Color(.96f,.91f,.76f);t.raycastTarget=false;t.alignment=TextAnchor.MiddleCenter;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=15;t.resizeTextMaxSize=size;return t;
        }
        Button Button(Transform parent,string title,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action,int fontSize=28)
        {
            var r=Rect(parent,title,min,max);var image=r.gameObject.AddComponent<Image>();image.sprite=CampusLook.Sprite("button");image.type=Image.Type.Sliced;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=new Color(1,.85f,.55f);colors.pressedColor=new Color(.7f,.8f,.9f);b.colors=colors;
            if(title.Length>0)Label(r,title,new Vector2(.04f,.02f),new Vector2(.96f,.98f),fontSize).text=title;
            if(action!=null)b.onClick.AddListener(action);return b;
        }
    }
}
