using System;
using System.Collections.Generic;
using System.IO;
using Backfranz.Playable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

[InitializeOnLoad]
public static class BackfranzPlayableSmoke
{
    static Queue<Action<SessionFlow>> steps;
    static int checks;
    static double started, normalWalkSeconds, horrorWalkSeconds;
    static bool startedGame,returning,loadingSave;
    static string awaitedEscort;
    static double escortWaitUntil;
    static SpriteRenderer observedDoor;
    static SpriteRenderer observedGate;
    static Vector3 gateScale;
    static Vector3 doorPosition,doorScale;
    static bool filmedDoor;
    static BackfranzPlayableSmoke(){EditorApplication.playModeStateChanged+=Changed;}
    public static void Run()
    {
        Begin(false);
    }
    [MenuItem("Tools/Backfranz/Pruebas/Acceso y rótulos")]
    public static void RunEntrance()
    {
        Begin(false,true);
    }
    public static void RunFinaleSaved()
    {
        if(!TestSave)throw new InvalidOperationException("RunFinaleSaved requires the isolated -backfranzSmokeSave slot.");
        Begin(true);
    }
    static void Begin(bool finaleOnly,bool entranceOnly=false)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity");
        SessionState.SetBool("Backfranz.SmokeFinaleOnly",finaleOnly);
        SessionState.SetBool("Backfranz.SmokeEntranceOnly",entranceOnly);
        SessionState.SetBool("Backfranz.PlayableSmoke",true);EditorApplication.isPlaying=true;
    }
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("Backfranz.PlayableSmoke",false))return;
        started=EditorApplication.timeSinceStartup;checks=0;startedGame=returning=loadingSave=false;
        normalWalkSeconds=horrorWalkSeconds=0;
        awaitedEscort=null;
        observedDoor=null;filmedDoor=false;
        if(TestSave)PlayerPrefs.DeleteKey("Backfranz.Smoke.Endings");
        if(SessionState.GetBool("Backfranz.SmokeFinaleOnly",false)){
            steps=new Queue<Action<SessionFlow>>();
            startedGame=loadingSave=true;
            steps.Enqueue(CheckCompleteJournal);
            QueueFinaleEndings();
        }
        else SetSteps();
        EditorApplication.update+=Tick;
    }
    static void Check(bool value,string label){if(!value)throw new Exception("PLAYABLE FAIL: "+label);checks++;Debug.Log("PLAYABLE PASS: "+label);}
    static void Walk(SessionFlow s,Vector2 goal)
    {
        var path=s.World.FindRoute(s.Player.Position,goal);
        Check(path.Count>0||Vector2.Distance(s.Player.Position,goal)<.2f,"Walkable target in "+Title(s.Room)+" at "+(goal-s.World.Offset(s.Room)));
        int moves=0;
        foreach(var cell in path){
            for(int i=0;i<48&&Vector2.Distance(s.Player.Position,cell)>.045f;i++){
                s.Player.Move((cell-s.Player.Position)/.08f,.025f);
                moves++;
            }
            if(Vector2.Distance(s.Player.Position,cell)>.1f)throw new Exception("Motor blocked at "+cell+" in "+Title(s.Room));
        }
        if(Vector2.Distance(s.Player.Position,goal)>.15f)throw new Exception("Did not reach "+goal);
        if(IsDark(s.Room))horrorWalkSeconds+=moves*.025;
        else normalWalkSeconds+=moves*.025;
    }
    static void Use(SessionFlow s,Vector2 goal){Walk(s,goal);s.Player.Interact();}
    static void OpenLift(SessionFlow s)
    {
        string origin=Title(s.Room);
        Use(s,s.World.Lift(s.Room));
        Check(IsLiftCabin(s.Room)&&!s.LiftOpen&&s.World.CanStand(s.Player.Position),"Call button enters a walkable cabin from "+origin);
        Use(s,s.World.CabinPanel(s.Room));
        Check(s.LiftOpen,"Cabin panel opens from "+origin);
    }
    static void LiftTo(SessionFlow s,int floor){OpenLift(s);s.ChooseFloor(floor);Check(s.Crossing,"Floor button starts transition");}
    static void CaptureHere(SessionFlow s,string file){s.SendMessage("LateUpdate");s.SendMessage("RefreshHUD");Capture(file);}
    static void SetSteps()
    {
        steps=new Queue<Action<SessionFlow>>();
        steps.Enqueue(s=>{
            Check(s.Room==Entrance&&!s.HasCard,"Fresh session starts outside");
            var tile=CampusLook.Sprite("tile16-pb-suelo-adoquin");
            Check(tile.rect.width==16&&tile.rect.height==16&&tile.texture.filterMode==FilterMode.Point,"Street uses native 16x16 nearest-neighbor tiles");
            foreach(string material in new[]{"terrazo","laboratorio","roble","cafe","auditorio","acustico","metal","rejilla"})
            {
                var normal=CampusLook.Sprite("tile16-escena-suelo-"+material);
                var altered=CampusLook.Sprite("tile16-backfranz-escena-suelo-"+material);
                Check(normal.pixelsPerUnit==16&&normal.rect.size==new Vector2(16,16)&&
                    altered.rect.size==normal.rect.size&&altered.texture.filterMode==FilterMode.Point,
                    "Campus scenery uses native tiles in both phases: "+material);
            }
            var camera=Camera.main;
            float pixelsPerTile=camera.pixelHeight/(2f*camera.orthographicSize);
            Check(Mathf.Abs(pixelsPerTile/16f-Mathf.Round(pixelsPerTile/16f))<.001f,"Camera projects base tiles at integer scale");
            Check(!camera.allowMSAA&&!camera.allowHDR&&!camera.allowDynamicResolution,
                "Gameplay camera disables filtering features that blur pixel art");
            var oddViewport=new RenderTexture(1365,767,0);
            camera.targetTexture=oddViewport;
            s.SendMessage("LateUpdate");
            var screen=camera.WorldToScreenPoint(s.World.Cell(Entrance,4,5));
            Check(Mathf.Abs(screen.x-Mathf.Round(screen.x))<.005f&&Mathf.Abs(screen.y-Mathf.Round(screen.y))<.005f,
                "Tile edges align to the pixel grid at an odd viewport resolution");
            camera.targetTexture=null;
            UnityEngine.Object.Destroy(oddViewport);
            s.SendMessage("LateUpdate");
            Check(UnityEngine.Object.FindObjectsByType<SessionFlow>(FindObjectsSortMode.None).Length==1,"Single session");
            var soundSources=s.Sound.GetComponents<AudioSource>();
            foreach(string group in new[]{"Efectos","Pasos","Ambiente","Música"})
                Check(Array.Exists(soundSources,source=>source.outputAudioMixerGroup&&source.outputAudioMixerGroup.name==group),
                    "Runtime audio is routed to "+group);
            Check(s.GetComponent<AudioSource>().outputAudioMixerGroup==s.Sound.AmbientGroup,
                "The campus ambience is assigned to the ambience bus");
            var soundLibrary=Resources.LoadAll<AudioClip>("Backfranz/Audio");
            Check(soundLibrary.Length==84,"All 84 sounds are available in play mode");
            foreach(var clip in soundLibrary)
            {
                bool loop=clip.name.StartsWith("ambiente-",StringComparison.Ordinal)||clip.name=="musica-menu";
                Check(clip.frequency==48000&&clip.samples>0&&(!loop||clip.channels==2&&clip.length>=19.9f),
                    "Imported audio has its expected format: "+clip.name);
            }
            foreach(string surface in new[]{"adoquin","baldosa","madera","metal","alfombra"})
                for(int variant=1;variant<=5;variant++)
                {
                    string name="paso-"+surface+"-"+variant;
                    Check(Array.Exists(soundLibrary,clip=>clip.name==name&&clip.channels==1),
                        "Footstep variation is available: "+name);
                }
            Check(s.World.Footing(Entrance,s.Player.Position)==SurfaceSound.Adoquin,
                "Footsteps use the street cobblestone sample family");
            Check(Array.Exists(s.GetComponents<AudioSource>(),source=>source.loop&&source.clip&&source.clip.name=="ambiente-landaeta"),
                "The exterior starts with its own looping stereo ambience");
            var dirs=UnityEngine.Object.FindObjectsByType<TouchDirection>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            Check(dirs.Length==4,"Four tactile movement directions");
            var touchRoot=dirs[0].transform.parent.gameObject;
            touchRoot.SetActive(true);
            var right=Array.Find(dirs,d=>d.direction==Vector2.right);
            var up=Array.Find(dirs,d=>d.direction==Vector2.up);
            var firstTouch=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=101};
            var secondTouch=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=102};
            right.OnPointerDown(firstTouch);up.OnPointerDown(secondTouch);
            Check(Vector2.Distance(s.Player.TouchInput,new Vector2(1,1).normalized)<.001f,"Integrated multitouch normalizes diagonal movement");
            up.OnPointerUp(secondTouch);
            Check(s.Player.TouchInput==Vector2.right,"Releasing a second finger preserves the first direction");
            up.OnPointerDown(secondTouch);right.OnPointerUp(firstTouch);
            Check(s.Player.TouchInput==Vector2.up,"Reverse finger release preserves the remaining direction");
            touchRoot.SetActive(false);
            Check(s.Player.TouchInput==Vector2.zero,"Hiding integrated controls releases all touch directions");
            Check(s.DialogueOpen,"Yamil opens the first-day dialogue");
            var spawn=s.Player.Position;s.Player.Move(Vector2.right,.1f);
            Check(s.Player.Position==spawn,"Dialogue locks movement");
            var continueButton=System.Array.Find(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None),
                button=>button.transform.parent.name=="Diálogo");
            Check(continueButton,"Dialogue has a visible continue button");
            continueButton.onClick.Invoke();Check(s.DialogueOpen,"Dialogue button reveals the current line");
            CaptureHere(s,"00-intro-yamil.png");
            continueButton.onClick.Invoke();Check(s.DialogueOpen,"Dialogue button advances to the next line");
            FinishDialogue(s);Check(!s.DialogueOpen,"Interact closes the final line");
            s.SendMessage("UpdateTutorial");
            var tutorial=GameObject.Find("Tutorial de primera partida");
            Check(tutorial&&Array.Exists(tutorial.GetComponentsInChildren<Text>(),t=>t.text.Contains("1/4")),"New game reveals the movement tutorial after introduction");
            for(int move=0;move<16;move++){
                s.Player.Move(Vector2.right,.05f);
                s.SendMessage("UpdateTutorial");
            }
            Check(Array.Exists(tutorial.GetComponentsInChildren<Text>(),t=>t.text.Contains("2/4")),"Tutorial advances after actual walking");
            var mission=GameObject.Find("Misión actual");
            Check(mission&&Array.Exists(mission.GetComponentsInChildren<Text>(),label=>label.text.Contains("gradas de Landaeta")),
                "Mission panel tells the player how to reach Block A");
            Use(s,s.World.StudentContact);Check(s.DialogueOpen,"Student can be approached and spoken to");
            Check(Array.Exists(GameObject.Find("Diálogo").GetComponentsInChildren<Image>(),
                portrait=>portrait.sprite&&portrait.sprite.name=="npc-estudiante"),
                "Landaeta student uses the same unique appearance in dialogue");
            FinishDialogue(s);
            Check(!s.DialogueOpen&&s.Room==Entrance,"Student conversation returns control outdoors");
            s.SendMessage("UpdateTutorial");
            Check(Array.Exists(tutorial.GetComponentsInChildren<Text>(),t=>t.text.Contains("3/4")),"Talking advances the tutorial to pause and saving");
            var pauseUi=UnityEngine.Object.FindFirstObjectByType<BackfranzUI>();
            pauseUi.Pause();s.SendMessage("UpdateTutorial");
            Check(!tutorial.activeSelf&&Time.timeScale==0,"Pause hides the tutorial and freezes gameplay");
            pauseUi.Resume();s.SendMessage("UpdateTutorial");
            Check(tutorial.activeSelf&&Array.Exists(tutorial.GetComponentsInChildren<Text>(),t=>t.text.Contains("4/4")),"Resume restores the final reception tutorial step");
            Walk(s,s.World.Cell(Entrance,9,8));CaptureHere(s,"01-landaeta.png");
            Walk(s,s.World.StreetFoot);
            Check(!s.World.HasLineOfSight(s.World.Cell(Entrance,7,11),s.World.StreetFoot+Vector2.up),
                "The stair boundary stops the player from entering laterally");
            Vector2 before=s.Player.Position;
            for(int i=0;i<44;i++)s.Player.Move(Vector2.up,.025f);
            Check(s.Player.Position.y>before.y+3.2f&&s.World.CanStand(s.Player.Position),
                "Yamil climbs the visible treads to the raised landing");
            Walk(s,s.World.StreetLanding);CaptureHere(s,"01-acceso.png");
            Check(!s.World.CanStand(s.World.Cell(Entrance,15,17)),"Street facade blocks the player");
            Use(s,s.World.StreetDoor);
            Check(s.Crossing,"Street entrance opens with a transition");
        });
        steps.Enqueue(s=>{
            Check(s.Room==Ground,"Side entrance reaches planta baja");
            Use(s,s.World.Exit(Ground));
        });
        steps.Enqueue(s=>{
            Check(s.Room==Entrance&&Vector2.Distance(s.Player.Position,s.World.StreetReturn)<.05f,
                "Returning from planta baja puts Yamil in front of the same door");
            CaptureHere(s,"01-puerta-lateral.png");
            Use(s,s.World.StreetDoor);
        });
        steps.Enqueue(s=>{
            Check(s.Room==Ground,"Entrance leads to reception");
            if(SessionState.GetBool("Backfranz.SmokeEntranceOnly",false))
            {
                steps.Clear();
                var contact=s.World.DoorPoint(Ground,1);
                Walk(s,contact);
                observedDoor=FindFramedDoor("Financiero");
                Check(observedDoor,"Financiero has an integrated frame");
                Check(Array.Exists(s.World.GetComponentsInChildren<Text>(),
                    text=>text.text=="Financiero" && text.fontSize>=24 && !text.raycastTarget),
                    "Financiero bears a readable wall plaque in the game font");
                doorPosition=observedDoor.transform.position;
                doorScale=observedDoor.transform.localScale;
                CaptureHere(s,"02-financiero-placa.png");
                s.Player.Interact();
                steps.Enqueue(active=>{
                    Check(filmedDoor,"The leaves open while the frame stays fixed");
                    Check(active.Room==InteriorRoom(1,false),"The framed doorway leads into Financiero");
                    Walk(active,active.World.InteriorExit(active.Room));
                    CaptureHere(active,"02-financiero-salida.png");
                    active.Player.Interact();
                });
                steps.Enqueue(active=>{
                    Check(active.Room==Ground,"The exit leads back to the hall");
                    Finish(0);
                });
                return;
            }
            Check(Array.Exists(s.GetComponents<AudioSource>(),source=>source.clip&&source.clip.name=="ambiente-campus"),
                "Entering reception selects the campus ambience");
            Check(!s.World.CanStand(s.World.Cell(Ground,44,5)),"Space beside the entrance corridor is outside the map");
            Walk(s,s.World.Cell(Ground,57,14));CaptureHere(s,"02-pb-giro.png");
            Check(s.World.FindRoute(s.Player.Position,s.World.Lift(Ground)).Count==0,"Closed security blocks the route to the elevator");
            Check(!s.World.CanStand(s.World.ReceptionContact+Vector2.up),"Reception counter prevents entering the booth");
            Check(!FindFramedDoor("Recepción"),"Reception has no separate room door");
            Use(s,s.World.SecurityPoint(2,true));Check(s.Crossing,"Biometric scan animates before deciding access");
        });
        steps.Enqueue(s=>{
            Check(!s.World.SecurityOpen(2)&&s.World.FindRoute(s.Player.Position,s.World.Lift(Ground)).Count==0,"Unregistered scan denies access without opening a bypass");
            Use(s,s.World.GuardContact);Check(s.DialogueOpen,"Guard is available before security");FinishDialogue(s);
            Check(!s.World.SecurityOpen(AccessibleLane),"Guard requires registration for accessible door");
            Use(s,s.World.ReceptionContact);Check(s.DialogueOpen,"Receptionist explains induction");
            s.Player.Interact();
            CaptureHere(s,"02-pb-dialogo.png");
            Time.timeScale=0;s.Player.Interact();Check(s.DialogueOpen&&!s.HasCard,"Pause blocks dialogue actions");Time.timeScale=1;
            FinishDialogue(s);
            Check(s.Room==Ground&&!s.DialogueOpen&&s.HasCard&&s.HasSchedule&&s.Visited==1,"Reception registers the player from the hall");
            CaptureHere(s,"02-pb-recepcion.png");
            var lane=s.World.SecurityPoint(2,true);
            observedGate=Array.Find(s.World.GetComponentsInChildren<SpriteRenderer>(),renderer=>
                renderer.sprite&&renderer.sprite.name=="control-molinete-00"&&Mathf.Abs(renderer.transform.position.y-lane.y)<.1f);
            Check(observedGate,"Biometric lane has a fixed assembly with closed glass leaves");
            gateScale=observedGate.transform.localScale;
            Use(s,s.World.SecurityPoint(2,true));Check(s.Crossing&&!s.World.SecurityOpen(2),"Registered scan starts with the gate closed");
        });
        steps.Enqueue(s=>{
            Check(s.World.SecurityOpen(2)&&!s.World.SecurityOpen(3),"Authorized scan opens only its lane");
            Check(observedGate.transform.localScale==gateScale&&observedGate.sprite.name=="control-molinete-06",
                "The glass leaves open without moving or flattening the gate housing");
            CaptureHere(s,"02-pb-control-abierto.png");
            Walk(s,s.World.SecurityPoint(2,false));
            Use(s,s.World.GuardContact);
            FinishDialogue(s);
            DialogueChoice("Ahora no").onClick.Invoke();
            Check(!s.Crossing&&!s.World.SecurityOpen(AccessibleLane),"Guard can leave the gate closed when asked");
            Use(s,s.World.GuardContact);
            FinishDialogue(s);
            DialogueChoice("Abrir el paso").onClick.Invoke();
            Check(s.Crossing,"Authorized guard operates accessible door");
        });
        steps.Enqueue(s=>{
            Check(s.World.SecurityOpen(AccessibleLane),"Accessible door opens on guard authorization");
            Walk(s,s.World.SecurityPoint(AccessibleLane,true));Walk(s,s.World.SecurityPoint(AccessibleLane,false));
            Check(!s.World.CanStand(s.World.Cell(Ground,22,14)),"Seated NPC has a collision");
            Use(s,s.World.Cell(Ground,21,14));
            Check(s.DialogueOpen,"Seated student responds in the common hall");
            FinishDialogue(s);
            CheckUpperInteriors(s,active=>CheckGroundDoors(active,Ground,true,()=>{
                Check(observedGate.sprite.name=="control-molinete-00"&&active.World.SecurityOpen(2),
                    "After the walk the leaves close and retain authorization");
                Walk(active,active.World.Cell(Ground,7,17));CaptureHere(active,"02-pb-auditorio-banos-gradas.png");
                Walk(active,active.World.Cell(Ground,32,15));CaptureHere(active,"02-pb-pasillo.png");
                Use(active,active.World.Stairs(Ground));Check(active.Crossing,"Central stair opens from the hall");
            }));
        });
        steps.Enqueue(s=>{
            Check(s.Room==FirstStairwell,"Central stairs load a separate walkable stairwell");
            Check(Array.Exists(s.GetComponents<AudioSource>(),source=>source.clip&&source.clip.name=="ambiente-gradas"),
                "Stairwell selects its own ambience");
            Check(!s.World.CanStand(s.World.Cell(s.Room,8,5)),"Stairwell walls prevent skipping the turn");
            Walk(s,s.World.Cell(s.Room,8,11));CaptureHere(s,"02-pb-descansillo.png");
            if (TestSave) {
                var ui=UnityEngine.Object.FindFirstObjectByType<BackfranzUI>();
                ui.Pause();
                var save=Array.Find(ui.menu.GetComponentsInChildren<Button>(),button=>button.name=="Guardar partida");
                Check(save,"Pause offers a save button");
                save.onClick.Invoke();
                Check(SessionFlow.CanContinue&&ui.status.text=="Partida guardada","Save button writes the intermediate landing");
                loadingSave=true;s.Menu();
            }
        });
        steps.Enqueue(s=>{
            Check(s.Room==FirstStairwell && Vector2.Distance(s.Player.Position,s.World.Cell(s.Room,8,11))<.1f,
                "Stairwell position survives continuing");
            Check(s.HasCard&&s.World.SecurityOpen(AccessibleLane)&&s.World.SecurityOpen(2),"Registration and both gate unlocks survive continuing");
            Use(s,s.World.StairLanding(s.Room,true));Check(s.Crossing,"Upper landing connects to floor 1");
        });
        steps.Enqueue(s=>{
            Check(s.Room==ForFloor(1,false)&&Vector2.Distance(s.Player.Position,s.World.Stairs(s.Room))<.1f,
                "Central stairs arrive at the matching stair on floor 1");
            Use(s,s.World.Stairs(s.Room,1));s.ChooseStairDirection(-1);
            Check(s.Crossing,"Auditorium stair accepts descent from floor 1");
        });
        steps.Enqueue(s=>{
            Check(s.Room==FirstStairwell+1,"Auditorium stairs use their own stairwell");
            Walk(s,s.World.Cell(s.Room,8,11));
            Use(s,s.World.StairLanding(s.Room,false));
        });
        steps.Enqueue(s=>{
            Check(s.Room==Ground&&Vector2.Distance(s.Player.Position,s.World.Stairs(s.Room,1))<.1f,
                "Auditorium stair returns through the correct ground-floor opening");
            LiftTo(s,6);
        });
        steps.Enqueue(s=>{
            Check(s.Room==Top,"Elevator connects the hall to the library");
            Use(s,s.World.MainStop(Top));
            Use(s,s.World.Lift(Top));
            Check(IsLiftCabin(s.Room),"The library elevator has a separate cabin");
            Check(Array.Exists(s.GetComponents<AudioSource>(),source=>source.clip&&source.clip.name=="ambiente-cabina"),
                "The elevator cabin has its own ambience");
            var panelPosition=s.World.CabinPanel(s.Room)+Vector2.up*.84f;
            Vector3 panelViewport=Camera.main.WorldToViewportPoint(panelPosition);
            Check(panelViewport.y>.30f&&panelViewport.y<.88f,"Cabin controls are in view and below the HUD");
            CaptureHere(s,"07-cabina.png");
            Use(s,s.World.CabinPanel(s.Room));
            Check(s.LiftOpen&&s.Horror==SessionFlow.HorrorState.Normal,"Opening the floor 6 panel keeps campus normal");
            Check(!s.SaveGame(),"No partial save in a cabin panel");
            s.CloseLift();
            Use(s,s.World.CabinExit(s.Room));
        });
        steps.Enqueue(s=>{
            Check(s.Room==Top&&s.Horror==SessionFlow.HorrorState.Normal,"Leaving the cabin without selecting a floor keeps the campus normal");
            Use(s,s.World.Stairs(s.Room,0));
            Check(s.Crossing,"Floor 6 stair descent stays available");
        });
        steps.Enqueue(s=>{
            Use(s,s.World.StairLanding(s.Room,false));
        });
        steps.Enqueue(s=>{
            Check(s.Room==ForFloor(5,false)&&s.Horror==SessionFlow.HorrorState.Normal,"Descending from the library on foot keeps campus normal");
            LiftTo(s,1);
        });
        for(int i=1;i<=5;i++){
            int floor=i;
            steps.Enqueue(s=>{
                Check(s.Room==ForFloor(floor,false),"Normal floor "+floor+" selected");
                var mission=GameObject.Find("Misión actual");
                Check(mission&&Array.Exists(mission.GetComponentsInChildren<Text>(),label=>label.text.Contains("Piso "+floor+" · Habla con")),
                    "Mission panel updates with the guide on floor "+floor);
                Check(s.World.CanStand(s.Player.Position),"Safe elevator arrival on "+floor);
                for(int core=0;core<StairwellCount;core++)
                    Check(s.World.FindRoute(s.Player.Position,s.World.Stairs(s.Room,core)).Count>0,"Walkable stair opening "+core+" on floor "+floor);
                Use(s,s.World.MainStop(s.Room));
                Check(!s.HasVisited(floor),"The hall's sign directs the first-day activity on floor "+floor);
                foreach(var stop in s.World.Stops(s.Room)){
                    Check(s.World.FindRoute(s.Player.Position,stop.Position).Count>0||Vector2.Distance(s.Player.Position,stop.Position)<.2f,"Accessible landmark: "+stop.Label);
                }
                if(floor==1)CaptureHere(s,"03-servicios.png");
                if(floor==2)CaptureHere(s,"03-salud-y-cocina.png");
                if(floor==3)CaptureHere(s,"04-computacion.png");
                if(floor==5)CaptureHere(s,"05-taller.png");
                if(floor==4){
                    CaptureHere(s,"05-aulas.png");
                    Check(GameObject.Find("Estudiante de Derecho"),"Scheduled student starts in the hall");
                    float clock=Time.time+5.5f;
                    for(int step=0;step<30;step++)s.World.UpdatePeople(s.Room,s.Player.Position,.1f,clock+step*.1f);
                    Check(!GameObject.Find("Estudiante de Derecho"),"Scheduled student goes into class");
                }
                CheckUpperInteriors(s, active => {
                    Check(active.HasVisited(floor),"The activity records a certificate on floor "+floor);
                    if(floor==4){
                        active.World.UpdatePeople(active.Room,active.Player.Position,.1f,Time.time+30f);
                        Check(GameObject.Find("Estudiante de Derecho"),"Scheduled student returns after class");
                    }
                    LiftTo(active,floor==5?6:floor+1);
                });
            });
        }
        steps.Enqueue(s=>{
            Check(s.Room==Top&&!s.InductionComplete&&s.Visited==6,"The library remains accessible before the last certificate");
            OpenLift(s);
            s.ChooseFloor(5);
            Check(s.Crossing&&s.Horror==SessionFlow.HorrorState.Normal,"An incomplete induction keeps a library elevator descent normal");
        });
        steps.Enqueue(s=>{
            Check(s.Room==ForFloor(5,false)&&!s.InductionComplete,"A pending certificate allows a normal return to the other floors");
            LiftTo(s,6);
        });
        steps.Enqueue(s=>{
            Use(s,s.World.MainStop(Top));
            Check(!GameObject.Find("Mecanismo de escape"),"The library first asks Yamil to speak with Alma");
            CheckUpperInteriors(s, active => {
                SubmitInductionActivity(active,active.World.MainStop(Top),"Mostrador de biblioteca",0);
                Check(active.InductionComplete&&active.Visited==7,"Six activities and the reception registration finish induction");
                CaptureHere(active,"06-biblioteca.png");
                OpenLift(active);CaptureHere(active,"07-ascensor.png");
                active.ChooseFloor(2);
                Check(active.Crossing&&active.Horror==SessionFlow.HorrorState.Pending,"Selecting a lower floor after finishing induction arms the event");
                Check(!active.SaveGame(),"A moving elevator cannot save a partial transition");
            });
        });
        steps.Enqueue(s=>{
            Check(s.Room==ForFloor(2,true)&&s.Horror==SessionFlow.HorrorState.Active&&Time.timeScale==1,"Descent loads the provisional altered destination");
            Check(s.DialogueOpen,"First altered arrival starts the escape introduction");
            FinishDialogue(s);
            Check(!GameObject.Find("La sombra"),"Provisional horror has no encounter");
            LiftTo(s,3);
        });
        steps.Enqueue(s=>{
            Check(s.Room==ForFloor(3,true),"Altered computer floor is accessible");
            foreach(int interior in s.World.UpperInteriorsOn(s.Room)){
                if(UpperInteriorName(interior)!="Sonido")continue;
                Use(s,s.World.UpperInteriorDoor(interior));
                Check(!s.Crossing&&s.Room==ForFloor(3,true),"Locked altered sound room keeps the player in the hall");
            }
            LiftTo(s,6);
        });
        steps.Enqueue(s=>{
            Check(s.Room==DarkTop&&s.Horror==SessionFlow.HorrorState.Active,"Altered library remains accessible");
            if (TestSave) {
                Check(s.SaveGame(),"Active horror is saved");
                loadingSave=true;s.Menu();
            }
        });
        steps.Enqueue(s=>{
            Check(s.Room==DarkTop&&s.Horror==SessionFlow.HorrorState.Active,"Continuing retains altered library and active horror");
            OpenLift(s);s.ChooseFloor(0);
            Check(s.Crossing&&s.Horror==SessionFlow.HorrorState.Active,"Subsequent library descent does not replay the event");
        });
        steps.Enqueue(s=>{
            Check(s.Room==DarkGround&&s.Horror==SessionFlow.HorrorState.Active,"Altered ground floor loads without retriggering");
            Use(s,s.World.Exit(DarkGround));
            Check(!s.Finished,"The escape gate refuses entry before the mechanisms are solved");
            Use(s,s.World.GalleryEntrance);
            Check(s.Room==DarkGround&&!s.Crossing,"The service gallery stays sealed before the evidence and acoustic channel");
            CheckGroundDoors(s,DarkGround,false,()=>CheckEscapeCampaign(s));
        });
    }
    static bool TestSave => Application.isBatchMode && Array.Exists(Environment.GetCommandLineArgs(), arg=>arg=="-backfranzSmokeSave");

    static void CheckEscapeCampaign(SessionFlow session)
    {
        var tasks=new[]{
            (6,17,8,6,15,15,2), (4,5,15,2,12,15,1), (5,26,15,3,14,7,0),
            (3,26,15,1,12,15,2), (1,5,14,4,26,15,1), (2,5,15,5,15,15,0), (6,16,15,0,32,15,2)
        };
        int current=Floor(session.Room);
        foreach(var (clueFloor,clueX,clueY,floor,x,y,answer) in tasks){
            if(floor==0){
                QueueInvestigation(current);
                QueueMaintenance();
                QueueSignalNetwork();
                QueueBlackoutTimeline();
                QueueAcousticRoute();
                QueueServiceGallery();
                QueueRadioCrew();
                current=6;
            }
            if(floor==1){
                foreach(var (partFloor,roomName,item) in new[]{
                    (1,"Posgrado","Puente Norte"),(2,"Histología y Fisiología","Puente Este"),(3,"Laboratorio PC","Puente Sur")
                }){
                    if(current!=partFloor)steps.Enqueue(s=>LiftTo(s,partFloor));
                    steps.Enqueue(s=>{
                        foreach(int interior in s.World.UpperInteriorsOn(s.Room))
                            if(UpperInteriorName(interior)==roomName){Use(s,s.World.UpperInteriorDoor(interior));return;}
                        Check(false,"Relay part room exists: "+roomName);
                    });
                    steps.Enqueue(s=>{
                        Check(Title(s.Room)==roomName&&GameObject.Find(item),"Physical relay part is visible in "+roomName);
                        Use(s,s.World.Entry(s.Room)+Vector2.right*2+Vector2.up);
                        Check(!GameObject.Find(item),"Relay part can be collected: "+item);
                        Use(s,s.World.UpperInteriorExit(s.Room));
                    });
                    current=partFloor;
                }
            }
            if(current!=clueFloor)steps.Enqueue(s=>LiftTo(s,clueFloor));
            if(floor==6)steps.Enqueue(s=>{
                Use(s,s.World.Cell(s.Room,x,y));
                var panel=GameObject.Find("Mecanismo de escape");
                var buttons=panel.GetComponentsInChildren<Button>();
                Check(!Array.Find(buttons,button=>button.name=="Opción 1").interactable,
                    "The first mechanism requires discovering its clue");
                Array.Find(buttons,button=>button.name=="Cerrar").onClick.Invoke();
            });
            steps.Enqueue(s=>{
                Use(s,s.World.Cell(s.Room,clueX,clueY));
                Check(s.DialogueOpen,"Escape witness can be reached on floor "+clueFloor);
                FinishDialogue(s);
            });
            if(clueFloor!=floor)steps.Enqueue(s=>LiftTo(s,floor));
            steps.Enqueue(s=>{
                Use(s,s.World.Cell(s.Room,x,y));
                var buttons=GameObject.Find("Mecanismo de escape").GetComponentsInChildren<Button>();
                if(floor==4){
                    EnterEscapeCode(buttons,"1924");
                    Check(Time.timeScale==0,"Incorrect closing time is rejected by the keypad");
                    EnterEscapeCode(buttons,"1930");
                    Check(Time.timeScale==1,"Correct closing time opens the Gesell archive");
                    return;
                }
                Check(buttons.Length==4&&buttons[answer].interactable,"Mechanism accepts evidence on floor "+floor);
                buttons[(answer+1)%3].onClick.Invoke();
                Check(Time.timeScale==0,"Wrong solution keeps the mechanism open for retry");
                int count=floor==3||floor==0?3:1;
                for(int i=0;i<count;i++)buttons[(answer+i)%3].onClick.Invoke();
                Check(Time.timeScale==1,"Correct solution closes the mechanism");
                if(floor==6&&TestSave){
                    Check(s.SaveGame(),"Saving after solving a mechanism succeeds");
                    loadingSave=true;s.Menu();
                }
            });
            if(floor==6&&TestSave)steps.Enqueue(s=>{
                var page=OpenEscapeJournal();
                Check(Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text=="Diario · 1/7"),
                    "Journal retains the solved mechanism after continuing");
                CaptureHere(s,"11-escape-diario.png");
                JournalButton(page,"Pistas").onClick.Invoke();
                Check(Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text.Contains("B7 termina en número")),
                    "Journal restores the discovered clue after continuing");
                Check(!Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text.Contains("El círculo con tapa blanca")),
                    "Journal omits undiscovered clues");
                JournalButton(page,"Pedir orientación").onClick.Invoke();
                Check(Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text.Contains("busca a Profesora")),
                    "Optional guidance identifies the next witness");
                JournalButton(page,"Cerrar").onClick.Invoke();
            });
            current=floor;
        }
        QueueRescues();
        QueueEscortRescues();
        steps.Enqueue(s=>{
            Check(GameObject.Find("Dalia")&&GameObject.Find("Lázaro")&&GameObject.Find("Nuria"),
                "Rescued NPCs move to the evacuation area");
            Check(GameObject.Find("Inés")&&GameObject.Find("Marcos")&&GameObject.Find("Elena"),
                "Escorted survivors also arrive at the evacuation area");
            Use(s,s.World.Exit(DarkGround));
            Check(s.DialogueOpen&&!s.Finished,"Opening the gate begins the blackout finale");
            FinishDialogue(s);
            Use(s,s.World.Exit(DarkGround));
            Check(!s.Finished,"The shutter blocks the ending until the emergency lever is used");
            Use(s,s.World.Cell(DarkGround,58,11));
            Check(s.DialogueOpen,"The emergency lever opens the three escape choices");
            FinishDialogue(s);
            if(TestSave)Check(s.SaveGame(),"The escape decision can be saved");
        });
        steps.Enqueue(CheckCompleteJournal);
        QueueFinaleEndings();
    }

    static GameObject OpenEscapeJournal()
    {
        var journal=Array.Find(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None),
            button=>button.name=="Diario");
        Check(journal,"The altered campus offers a journal");
        journal.onClick.Invoke();
        var page=GameObject.Find("Diario de escape");
        Check(page&&Time.timeScale==0,"Journal stops movement while open");
        return page;
    }

    static Button JournalButton(GameObject page,string name)
        =>Array.Find(page.GetComponentsInChildren<Button>(),button=>button.name==name);

    static void CheckCompleteJournal(SessionFlow s)
    {
        var page=OpenEscapeJournal();
        bool legacyFinale=SessionState.GetBool("Backfranz.SmokeFinaleOnly",false);
        JournalButton(page,"Rescates").onClick.Invoke();
        Check(Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text=="Rescates · 6/6"||
            legacyFinale&&label.text=="Rescates · 3/6"),"The journal reports the rescue total");
        CaptureHere(s,"21-diario-rescates.png");
        JournalButton(page,"Pistas").onClick.Invoke();
        Check(Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text=="Pistas anotadas · 33"||
            legacyFinale&&(label.text=="Pistas anotadas · 30"||label.text=="Pistas anotadas · 22")),
            "Journal lists collected clues including the blackout archive and gallery witnesses");
        var next=JournalButton(page,"Siguiente");
        while(next.interactable)next.onClick.Invoke();
        Check(Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text.Contains("Paula · Aula de grupos")),
            "Clue pages reach the last witness");
        JournalButton(page,"Anterior").onClick.Invoke();
        Check(next.interactable,"Clue pages can be browsed in both directions");
        CaptureHere(s,"17-escape-pistas.png");
        JournalButton(page,"Cerrar").onClick.Invoke();
        Check(Time.timeScale==1,"Closing the journal resumes play");
    }

    static void QueueFinaleEndings()
    {
        var endings=TestSave?new[]{
            (EscapeArchive.Ending.Eco,57,5),(EscapeArchive.Ending.Umbral,60,4),(EscapeArchive.Ending.Amanecer,58,2)
        }:new[]{(EscapeArchive.Ending.Amanecer,58,2)};
        foreach(var (ending,x,y) in endings)steps.Enqueue(s=>{
            if(TestSave)Check(!EscapeArchive.Unlocked(ending),"An unchosen route remains locked in Extras");
            Use(s,s.World.Cell(DarkGround,x,y));
            Check(s.DialogueOpen&&!s.Finished,"Approaching the exit prompts for confirmation");
            if(ending==endings[0].Item1)
            {
                FinishDialogue(s);
                DialogueChoice("Ahora no").onClick.Invoke();
                Check(!s.DialogueOpen&&!s.Finished,"Declining an exit leaves the player in the hall");
                if(TestSave)Check(!EscapeArchive.Unlocked(ending),"Declining an exit does not unlock the route in Extras");
                Use(s,s.World.Cell(DarkGround,x,y));
            }
            FinishDialogue(s);
            DialogueChoice("Cruzar").onClick.Invoke();
            Check(s.Finished&&s.Won&&EscapeArchive.Unlocked(ending),"Playable route unlocks "+EscapeArchive.Title(ending));
            if(ending==endings[0].Item1&&!SessionState.GetBool("Backfranz.SmokeFinaleOnly",false))
                Debug.Log($"PLAYABLE ROUTE WALK: normal {normalWalkSeconds:F1}s, horror {horrorWalkSeconds:F1}s (scripted path and regression detours)");
            bool legacyFinale=SessionState.GetBool("Backfranz.SmokeFinaleOnly",false);
            string countLabel=ending==EscapeArchive.Ending.Amanecer?"Personas a salvo · ":"Personas reunidas · ";
            Check(Array.Exists(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None),
                label=>label.text.Contains(countLabel+"6/6")||legacyFinale&&label.text.Contains(countLabel+"3/6")),
                "Ending distinguishes the assembled group from a successful group evacuation");
            var epilogue=Array.Find(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None),
                label=>label.text.StartsWith(EscapeArchive.Story(ending)+"\n\n",StringComparison.Ordinal));
            Check(epilogue,"Ending shows its own epilogue and the rescue outcome");
            var cover=GameObject.Find("Ilustración del final");
            Check(cover&&cover.GetComponent<Image>().sprite.name==EscapeArchive.Cover(ending),
                "The ending displays the illustration for "+EscapeArchive.Title(ending));
            Capture("final-"+ending.ToString().ToLowerInvariant()+".png");
            if(!legacyFinale&&ending==EscapeArchive.Ending.Amanecer)
            {
                Check(epilogue.text.Contains("La lista está completa"),"A full evacuation changes the dawn epilogue");
                Capture("22-epilogo-amanecer.png");
            }
            Check(Array.Exists(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None),
                label=>label.text.Contains("Partida · ")&&label.text.Contains("Escape · ")),
                "Ending reports total and escape times separately");
            s.Menu();
            if(ending==EscapeArchive.Ending.Amanecer)returning=true;
            else loadingSave=true;
        });
    }

    static void QueueRescues()
    {
        int current=0;
        foreach(var (floor,room,sourceFloor,source,person,item) in new[]{
            (0,"Café",2,"Cocina de prácticas","Dalia","Botella sellada"),
            (3,"Fotografía",5,"Taller","Lázaro","Linterna de trabajo"),
            (0,"Auditorio",2,"Sala de Simulación","Nuria","Botiquín portátil")
        }){
            if(current!=floor)steps.Enqueue(s=>LiftTo(s,floor));
            steps.Enqueue(s=>UseCampaignDoor(s,room));
            steps.Enqueue(s=>{
                Use(s,s.World.Entry(s.Room)+(room=="Auditorio"?new Vector2(-1,1):new Vector2(-2,2)));
                Check(s.DialogueOpen,"Rescue request is playable: "+person);
                FinishDialogue(s);
                LeaveCampaignRoom(s);
            });
            steps.Enqueue(s=>LiftTo(s,sourceFloor));
            steps.Enqueue(s=>UseCampaignDoor(s,source));
            steps.Enqueue(s=>{
                Check(GameObject.Find(item),"Rescue item is visible: "+item);
                Use(s,s.World.Entry(s.Room)+new Vector2(2,3));
                Check(!GameObject.Find(item),"Rescue item can be collected");
                LeaveCampaignRoom(s);
            });
            steps.Enqueue(s=>LiftTo(s,floor));
            steps.Enqueue(s=>UseCampaignDoor(s,room));
            steps.Enqueue(s=>{
                Use(s,s.World.Entry(s.Room)+(room=="Auditorio"?new Vector2(-1,1):new Vector2(-2,2)));
                Check(s.DialogueOpen,"Aid can be delivered to "+person);
                FinishDialogue(s);
                LeaveCampaignRoom(s);
            });
            current=floor;
        }
    }

    static void QueueEscortRescues()
    {
        foreach(var (floor,room,offsetX,refugeX,refugeY,person) in new[]{
            (1,"Posgrado",-2,26,5,"Inés"),(4,"Cámara Gesell",-2,4,5,"Marcos"),(5,"Aula de grupos",2,26,6,"Elena")
        })
        {
            steps.Enqueue(s=>LiftTo(s,floor));
            steps.Enqueue(s=>UseCampaignDoor(s,room));
            steps.Enqueue(s=>{
                Check(GameObject.Find(person),"Escort survivor can be found: "+person);
                Use(s,s.World.Entry(s.Room)+new Vector2(offsetX,2));
                Check(s.DialogueOpen,"Escort survivor describes their refuge");
                FinishDialogue(s);
                Use(s,s.World.UpperInteriorExit(s.Room));
                Check(!s.Crossing,"Player has to wait for "+person+" at the door");
                WaitForEscort(person);
            });
            steps.Enqueue(s=>{
                s.Player.Interact();
                Check(s.Crossing,"Escort can follow Yamil through an interior door");
            });
            steps.Enqueue(s=>{
                Check(GameObject.Find(person),"Escort is present in the hall: "+person);
                Walk(s,s.World.Cell(s.Room,refugeX,refugeY));
                if(TestSave&&floor==1)
                {
                    Check(s.SaveGame(),"An escort can be saved in the hall before reaching the refuge");
                    loadingSave=true;
                    s.Menu();
                }
            });
            steps.Enqueue(s=>{
                Check(s.Room==ForFloor(floor,true)&&GameObject.Find(person),"Escort is still active in the same hall");
                if(floor==1&&TestSave)
                {
                    var page=OpenEscapeJournal();
                    JournalButton(page,"Rescates").onClick.Invoke();
                    Check(Array.Exists(page.GetComponentsInChildren<Text>(),label=>label.text.Contains("Inés · Piso 1")&&
                        label.text.Contains("pasando las bancas")),"Continuing restores the rescue directions in the journal");
                    JournalButton(page,"Cerrar").onClick.Invoke();
                }
                s.Player.Interact();
                Check(!s.DialogueOpen,"The rescue waits for the student to reach the refuge");
                WaitForEscort(person);
            });
            steps.Enqueue(s=>{
                if(floor==1)CaptureHere(s,"20-rescate-al-refugio.png");
                s.Player.Interact();
                Check(s.DialogueOpen,"Yamil can radio the brigade once "+person+" reaches safety");
                FinishDialogue(s);
            });
        }
        steps.Enqueue(s=>LiftTo(s,0));
    }

    static void WaitForEscort(string person)
    {
        awaitedEscort=person;
        escortWaitUntil=EditorApplication.timeSinceStartup+22;
    }

    static void UseCampaignDoor(SessionFlow s,string room)
    {
        if(s.Room==DarkGround){
            for(int index=FirstGroundDoor;index<GroundDoors.Length;index++)
                if(GroundDoors[index].Name==room){Use(s,s.World.DoorPoint(s.Room,index));return;}
        }
        foreach(int interior in s.World.UpperInteriorsOn(s.Room))
            if(UpperInteriorName(interior)==room){Use(s,s.World.UpperInteriorDoor(interior));return;}
        Check(false,"Campaign room exists: "+room);
    }

    static void LeaveCampaignRoom(SessionFlow s)
        =>Use(s,IsUpperInterior(s.Room)?s.World.UpperInteriorExit(s.Room):s.World.InteriorExit(s.Room));

    static void EnterEscapeCode(Button[] buttons,string code)
    {
        foreach(char digit in code)Array.Find(buttons,button=>button.name==digit.ToString()).onClick.Invoke();
        Array.Find(buttons,button=>button.name=="Validar").onClick.Invoke();
    }

    static void QueueInvestigation(int current)
    {
        if(current!=6)steps.Enqueue(s=>LiftTo(s,6));
        steps.Enqueue(s=>{
            Use(s,s.World.Cell(DarkTop,15,9));
            Check(!GameObject.Find("Mecanismo de escape"),"The investigation desk requires testimony before accepting a verdict");
        });
        current=6;
        foreach(var (floor,room,speaker) in new[]{
            (1,"Caja","Iris"),(4,"Sala Jurídica","Renata"),(2,"Cocina de prácticas","Andrés"),
            (3,"Sala Mac","Mateo"),(5,"Taller","Eva"),(6,"Sala de lectura","Alma")
        }){
            if(current!=floor)steps.Enqueue(s=>LiftTo(s,floor));
            steps.Enqueue(s=>{
                foreach(int interior in s.World.UpperInteriorsOn(s.Room))
                    if(UpperInteriorName(interior)==room){Use(s,s.World.UpperInteriorDoor(interior));return;}
                Check(false,"Investigation room exists: "+room);
            });
            steps.Enqueue(s=>{
                Check(Title(s.Room)==room&&GameObject.Find(speaker),"Witness is present in "+room);
                Use(s,s.World.Entry(s.Room)+Vector2.left*2+Vector2.up);
                Check(s.DialogueOpen,"Witness can be questioned: "+speaker);
                FinishDialogue(s);
                Use(s,s.World.UpperInteriorExit(s.Room));
            });
            current=floor;
        }
        if(TestSave){
            steps.Enqueue(s=>{
                Check(s.SaveGame(),"Investigation testimony can be saved");
                loadingSave=true;s.Menu();
            });
        }
        foreach(int answer in new[]{1,2,0})steps.Enqueue(s=>{
            Use(s,s.World.Cell(DarkTop,15,9));
            var panel=GameObject.Find("Mecanismo de escape");
            Check(panel,"The desk accepts the evidence for a verdict");
            var buttons=panel.GetComponentsInChildren<Button>();
            Array.Find(buttons,b=>b.name=="Opción "+((answer+1)%3+1)).onClick.Invoke();
            Check(Time.timeScale==0,"A wrong deduction leaves the case open");
            Array.Find(buttons,b=>b.name=="Opción "+(answer+1)).onClick.Invoke();
            Check(Time.timeScale==1,"A supported deduction closes the case");
        });
    }

    static void QueueMaintenance()
    {
        steps.Enqueue(s=>{
            OpenLift(s);
            var button=Array.Find(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None),
                item=>item.name=="-1 · Mantenimiento");
            Check(button,"Evacuation clearance enables the maintenance floor");
            button.onClick.Invoke();
        });
        steps.Enqueue(s=>{
            Check(s.Room==Basement,"Maintenance elevator arrives in the west basement");
            Check(s.World.FindRoute(s.Player.Position,s.World.Lift(Basement)).Count==0,
                "Basement grate prevents leaving before restoring power");
            Use(s,s.World.Cell(Basement,6,11));
            Use(s,s.World.PowerPanel);
            Check(!s.World.Power,"Emergency panel requires the fuse");
            Use(s,s.World.Fuse);
            Check(s.HasFuse,"Fuse can be recovered in the basement");
            CaptureHere(s,"08-subsuelo-backfranz.png");
            Use(s,s.World.PowerPanel);
            Check(s.World.Power,"Fuse powers the basement gate");
            Use(s,s.World.Cell(Basement,24,10));
            Check(!GameObject.Find("Mecanismo de escape"),"Regulator requires all three physical readings");
            Use(s,s.World.Cell(Basement,20,12));
            Use(s,s.World.Cell(Basement,27,10));
            Use(s,s.World.Cell(Basement,24,10));
            var panel=GameObject.Find("Mecanismo de escape");
            Check(panel,"Regulator can be calibrated after visiting the gauges");
            var buttons=panel.GetComponentsInChildren<Button>();
            Array.Find(buttons,b=>b.name=="Opción 2").onClick.Invoke();
            Check(Time.timeScale==0,"Incorrect pressure is rejected");
            Array.Find(buttons,b=>b.name=="Opción 1").onClick.Invoke();
            Check(Time.timeScale==1,"Correct pressure enables the gate");
            LiftTo(s,6);
        });
    }

    static void QueueSignalNetwork()
    {
        foreach(var (floor,selectors) in new[]{(2,new[]{2,3,4}),(4,new[]{3,5}),(5,new[]{3,4})})
        {
            steps.Enqueue(s=>LiftTo(s,floor));
            if(floor==2&&TestSave)steps.Enqueue(s=>{
                Use(s,s.World.Cell(s.Room,17,7));
                var controls=GameObject.Find("Mecanismo de escape").GetComponentsInChildren<Button>();
                Array.Find(controls,button=>button.name=="1").onClick.Invoke();
                Array.Find(controls,button=>button.name=="Cerrar").onClick.Invoke();
                Check(s.SaveGame(),"A partially configured signal can be saved");
                loadingSave=true;s.Menu();
            });
            steps.Enqueue(s=>{
                Use(s,s.World.Cell(s.Room,17,7));
                var panel=GameObject.Find("Mecanismo de escape");
                Check(panel,"Signal cabinet is reachable on floor "+floor);
                var controls=panel.GetComponentsInChildren<Button>();
                if(floor==2&&TestSave)
                {
                    Check(Array.Find(controls,button=>button.name=="1").GetComponentInChildren<Text>().text=="1 · Luz",
                        "Continuing restores individual lamp states");
                    CaptureHere(s,"12-red-senales.png");
                }
                Array.Find(controls,button=>button.name=="Restablecer").onClick.Invoke();
                Array.Find(controls,button=>button.name=="1").onClick.Invoke();
                Check(Time.timeScale==0,"An incomplete signal keeps the cabinet open");
                Array.Find(controls,button=>button.name=="Restablecer").onClick.Invoke();
                foreach(int number in selectors)
                    Array.Find(controls,button=>button.name==number.ToString()).onClick.Invoke();
                Check(Time.timeScale==1,"Five lit lamps synchronize floor "+floor);
            });
        }
        steps.Enqueue(s=>LiftTo(s,3));
    }

    static void QueueAcousticRoute()
    {
        foreach(var (room,speaker) in new[]{("Streaming","Lina"),("Fotografía","Diego"),("Sala Mac","Hugo")})
        {
            steps.Enqueue(s=>UseCampaignDoor(s,room));
            steps.Enqueue(s=>{
                Check(Title(s.Room)==room&&GameObject.Find(speaker),"Acoustic witness is visible: "+speaker);
                Use(s,s.World.Entry(s.Room)+new Vector2(2,1));
                Check(s.DialogueOpen,"Acoustic witness explains a track");
                FinishDialogue(s);
                LeaveCampaignRoom(s);
            });
        }
        steps.Enqueue(s=>UseCampaignDoor(s,"Sonido"));
        steps.Enqueue(s=>{
            Check(Title(s.Room)=="Sonido","Synchronized stations unlock the acoustics room");
            var console=s.World.Cell(s.Room,8,4);
            Use(s,console);
            Walk(s,s.World.Cell(s.Room,7,5));
            CaptureHere(s,"13-placas-sonido.png");
            Check(Array.Exists(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None),
                label=>label.text.Contains("Interferencia")),"Wrong plate disarms the route");
            Use(s,console);
            foreach(var (x,y) in new[]{(9,4),(9,5),(9,6),(7,6),(7,7),(7,8),(11,8),(11,9)})
                Walk(s,s.World.Cell(s.Room,x,y));
            CaptureHere(s,"14-canal-validado.png");
            Check(Array.Exists(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None),
                label=>label.text.Contains("Canal acústico validado")),"Walking the correct physical path validates the acoustic channel");
            LeaveCampaignRoom(s);
        });
        steps.Enqueue(s=>LiftTo(s,6));
    }

    static void QueueBlackoutTimeline()
    {
        int current=3;
        var sources=new[]{
            (3,"Fotografía","Clip · Destello"), (5,"Taller","Clip · Prensa"),
            (6,"Sala de lectura","Raúl"), (4,"Aula 4A","Sara"),
            (2,"Sala de Simulación","Clip · Monitor de la camilla"), (1,"Dirección de Carrera","Mónica"),
            (0,"Financiero","Clip · Molinetes"), (0,"Admisiones","Vera")
        };
        foreach(var (floor,room,source) in sources)
        {
            if(current!=floor)steps.Enqueue(s=>LiftTo(s,floor));
            if(room=="Admisiones")steps.Enqueue(s=>{
                Use(s,s.World.Cell(DarkGround,47,14));
                Check(!GameObject.Find("Cronología del apagón"),"Camera archive requires the last witness");
                if(TestSave)
                {
                    Check(s.SaveGame(),"Found footage and testimony can be saved before the chronology");
                    loadingSave=true;
                    s.Menu();
                }
            });
            steps.Enqueue(s=>UseCampaignDoor(s,room));
            steps.Enqueue(s=>{
                Check(Title(s.Room)==room&&GameObject.Find(source),"Blackout source appears in "+room+": "+source);
                Use(s,s.World.Entry(s.Room)+new Vector2(2,2));
                Check(s.DialogueOpen,"Blackout source can be examined");
                FinishDialogue(s);
                LeaveCampaignRoom(s);
            });
            current=floor;
        }
        steps.Enqueue(s=>{
            Use(s,s.World.Cell(DarkGround,47,14));
            var panel=GameObject.Find("Cronología del apagón");
            Check(panel,"Archive opens after all eight sources, including footage recovered from the saved game");
            var canvas=panel.GetComponentInParent<Canvas>();
            Check(canvas&&canvas.GetComponent<GraphicRaycaster>()&&
                UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,
                "The chronology uses the existing interactive Canvas and EventSystem");
            CaptureHere(s,"18-archivo-apagon.png");
            var submit=JournalButton(panel,"Reproducir");
            var dials=new Button[4];
            for(int i=0;i<dials.Length;i++)dials[i]=JournalButton(panel,"Hora "+(i+1));
            submit.onClick.Invoke();
            Check(Array.Exists(panel.GetComponentsInChildren<Text>(),label=>label.text.Contains("hora distinta")),
                "Incomplete or duplicated times are rejected");
            int[] correct={4,1,3,2};
            for(int i=0;i<dials.Length;i++)
                for(int step=0;step<=i;step++)dials[i].onClick.Invoke();
            submit.onClick.Invoke();
            Check(Time.timeScale==0&&Array.Exists(panel.GetComponentsInChildren<Text>(),
                label=>label.text.Contains("no coincide")),"A wrong chronology leaves the controls open");
            for(int i=0;i<dials.Length;i++)
                for(int step=0;step<(correct[i]-(i+1)+5)%5;step++)dials[i].onClick.Invoke();
            submit.onClick.Invoke();
            Check(!panel.activeInHierarchy&&s.DialogueOpen&&GameObject.Find("Reflejo en el vidrio"),
                "Synchronizing the clips triggers the archive's horror reveal");
            FinishDialogue(s);
            Check(!s.DialogueOpen&&Time.timeScale==1,"Dialogue returns control after the reveal");
        });
        steps.Enqueue(s=>LiftTo(s,3));
    }

    static void QueueRadioCrew()
    {
        foreach(var (floor,room,name) in new[]{
            (1,"Dirección de Carrera","Camila"),(2,"Histología y Fisiología","Iván"),(5,"Aula de grupos","Paula")
        })
        {
            steps.Enqueue(s=>LiftTo(s,floor));
            steps.Enqueue(s=>UseCampaignDoor(s,room));
            steps.Enqueue(s=>{
                Check(Title(s.Room)==room&&GameObject.Find(name),"Evacuation crew member is present: "+name);
                Use(s,s.World.Entry(s.Room)+new Vector2(-2,2));
                Check(s.DialogueOpen,"Crew member explains their duties");
                FinishDialogue(s);
                LeaveCampaignRoom(s);
            });
        }
        steps.Enqueue(s=>LiftTo(s,0));
        steps.Enqueue(s=>{
            Use(s,s.World.Cell(DarkGround,30,14));
            var panel=GameObject.Find("Mecanismo de escape");
            Check(panel,"Coordination desk accepts the three crew briefings");
            var controls=panel.GetComponentsInChildren<Button>();
            var options=new[]{Array.Find(controls,b=>b.name=="Opción 1"),Array.Find(controls,b=>b.name=="Opción 2"),
                Array.Find(controls,b=>b.name=="Opción 3")};
            options[0].onClick.Invoke();
            Check(!options[0].interactable,"One person cannot be assigned to two posts");
            options[1].onClick.Invoke();
            options[2].onClick.Invoke();
            Check(Time.timeScale==0,"Invalid crew allocation can be retried");
            foreach(int i in new[]{1,2,0})options[i].onClick.Invoke();
            Check(Time.timeScale==1&&GameObject.Find("Camila")&&GameObject.Find("Iván")&&GameObject.Find("Paula"),
                "Correct assignments move the full crew to the exit");
            LiftTo(s,6);
        });
    }

    static void QueueServiceGallery()
    {
        steps.Enqueue(s=>LiftTo(s,0));
        steps.Enqueue(s=>{
            Use(s,s.World.GalleryEntrance);
            Check(s.Crossing,"The acoustic channel and camera archive open the gallery door");
        });
        steps.Enqueue(s=>{
            Check(s.Room==ServiceGallery&&s.World.CanStand(s.Player.Position)&&GameObject.Find("Sombra de la galería"),
                "The service gallery loads a walkable map and its roaming shadow");
            Check(s.World.Footing(ServiceGallery,s.Player.Position)==SurfaceSound.Metal,
                "The gallery starts on a metal foothold");
            Check(Array.Exists(s.GetComponents<AudioSource>(),source=>source.clip&&source.clip.name=="ambiente-galeria"),
                "The service gallery uses its own looping stereo ambience");
        });
        steps.Enqueue(s=>{
            Check(GameObject.Find("Atención de la sombra"),"Gallery displays a separate attention meter");
            Use(s,s.World.Cell(s.Room,5,6));
            Check(s.DialogueOpen,"Mara explains how the refuge lights work");
            FinishDialogue(s);
            CaptureHere(s,"19-galeria-servicio.png");
            Walk(s,s.World.Cell(s.Room,4,7));
            var refuge=s.World.GalleryLight(0);
            for(int i=0;i<160&&Vector2.Distance(s.Player.Position,refuge)>.2f;i++)
                s.Player.Move(Vector2.right,.025f);
            Check(Vector2.Distance(s.Player.Position,refuge)<.2f,"Moving too long in the dark returns Yamil to the last light");
            foreach(int light in new[]{1,2,3,4})Walk(s,s.World.GalleryLight(light));
            Use(s,s.World.Cell(s.Room,27,7));
            Check(s.DialogueOpen,"Felipe can be reached using the refuge lights");
            FinishDialogue(s);
            Walk(s,s.World.GalleryLight(5));
            Use(s,s.World.Cell(s.Room,27,13));
            s.SendMessage("RefreshHUD");
            Check(Array.Exists(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None),
                label=>label.text.Contains("antes de mover el mecanismo")),
                "The mechanism requires the remaining witness");
            Walk(s,s.World.GalleryLight(6));
            Use(s,s.World.Cell(s.Room,22,16));
            Check(s.DialogueOpen,"Celia is reachable at the far end");
            FinishDialogue(s);
            Use(s,s.World.GalleryLight(7));
            s.SendMessage("RefreshHUD");
            Check(Array.Exists(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None),
                label=>label.text.Contains("El mecanismo se traba")),
                "The gallery rejects selecting a mechanism out of order");
            Walk(s,s.World.GalleryLight(6));
            Walk(s,s.World.GalleryLight(5));
            Use(s,s.World.Cell(s.Room,27,13));
            if(TestSave)
            {
                Check(s.SaveGame(),"The gallery saves the first completed mechanism");
                loadingSave=true;
                s.Menu();
            }
        });
        steps.Enqueue(s=>{
            Check(s.Room==ServiceGallery&&GameObject.Find("Ventilación").GetComponent<SpriteRenderer>().color.g>.9f,
                "The first mechanism remains active when back in the gallery");
            Walk(s,s.World.GalleryLight(6));
            Use(s,s.World.GalleryLight(7));
            Walk(s,s.World.GalleryLight(8));
            Walk(s,s.World.GalleryLight(9));
            Walk(s,s.World.GalleryLight(0));
            Use(s,s.World.Cell(s.Room,8,5));
            Check(!GameObject.Find("Sombra de la galería").GetComponent<SpriteRenderer>().enabled,
                "Completing the ordered ventilation, return and latch removes the shadow");
            Use(s,s.World.GalleryExit);
            Check(s.Crossing,"The gallery exit starts a return to the hall");
        });
        steps.Enqueue(s=>Check(s.Room==DarkGround&&Vector2.Distance(s.Player.Position,s.World.GalleryEntrance)<1.1f,
            "The exit returns Yamil to the correct part of the hall"));
    }

    static void CheckUpperInteriors(SessionFlow session,Action<SessionFlow> next)
    {
        int hall=session.Room;
        var remaining=steps;
        steps=new Queue<Action<SessionFlow>>();
        foreach(int interior in session.World.UpperInteriorsOn(hall)){
            var point=session.World.UpperInteriorDoor(interior);
            steps.Enqueue(s=>{
                Use(s,point);
                Check(s.Crossing,"Upper door transitions to "+UpperInteriorName(interior));
            });
            steps.Enqueue(s=>{
                Check(s.Room==interior&&s.World.CanStand(s.Player.Position),"Upper interior is explorable: "+Title(s.Room));
                string name=UpperInteriorName(interior);
                if(name=="Sala Jurídica"&&!IsDark(interior))
                    Check(GameObject.Find("Estudiante de Derecho"),"Scheduled student is visible inside the correct classroom");
                if(!IsDark(interior))
                {
                    bool captureRoom=name=="Cocina de prácticas"||name=="Laboratorio PC"||name=="Sala de lectura";
                    Walk(s,s.World.Entry(interior)+Vector2.up*(captureRoom?4:2));
                    if(captureRoom||name=="Baños")
                        CaptureHere(s,"escenografia-"+interior+".png");
                    CheckInductionInRoom(s,name);
                }
                Use(s,s.World.UpperInteriorExit(s.Room));
            });
            steps.Enqueue(s=>{
                Check(s.Room==hall&&Vector2.Distance(s.Player.Position,point)<.15f,
                    "Upper interior returns to its matching door");
                if(TestSave&&UpperInteriorName(interior)=="Streaming"&&!IsDark(interior))
                {
                    Check(s.SaveGame(),"First-day instructions can be saved before finishing the activity");
                    loadingSave=true;
                    s.Menu();
                }
            });
        }
        steps.Enqueue(next);
        foreach(var step in remaining)steps.Enqueue(step);
    }

    static void CheckInductionInRoom(SessionFlow s,string name)
    {
        var point=s.World.Entry(s.Room)+Vector2.up*3;
        string guide=name switch{
            "Posgrado"=>"Irene","Cocina de prácticas"=>"Andrés","Streaming"=>"Lina",
            "Cámara Gesell"=>"Celia","Aula de grupos"=>"Valeria","Sala de lectura"=>"Alma",_=>null
        };
        if(guide!=null)
        {
            Check(GameObject.Find(guide),"First-day guide is visible: "+guide);
            Use(s,point);
            Check(s.DialogueOpen,"First-day guide gives instructions in "+name);
            FinishDialogue(s);
            return;
        }
        switch(name)
        {
            case "Caja":SubmitInductionActivity(s,point,"Ventanilla de turnos",1);break;
            case "Histología y Fisiología":SubmitInductionActivity(s,point,"Registro de bandejas",2);break;
            case "Sonido":SubmitInductionActivity(s,point,"Prueba de micrófono",0);break;
            case "Aula 4A":SubmitInductionActivity(s,point,"Lista de práctica",1);break;
            case "Taller":SubmitInductionActivity(s,point,"Casillero de herramientas",2);break;
        }
    }

    static void SubmitInductionActivity(SessionFlow s,Vector2 point,string station,int answer)
    {
        int before=s.Visited;
        Use(s,point);
        var panel=GameObject.Find("Mecanismo de escape");
        Check(panel&&Array.Exists(panel.GetComponentsInChildren<Text>(),label=>label.text==station),
            "The induction station opens after reading its instructions: "+station);
        var buttons=panel.GetComponentsInChildren<Button>();
        Array.Find(buttons,b=>b.name=="Opción "+((answer+1)%3+1)).onClick.Invoke();
        Check(Time.timeScale==0&&s.Visited==before,"An incorrect induction answer allows a retry");
        if(station=="Prueba de micrófono")CaptureHere(s,"23-induccion-sonido.png");
        Array.Find(buttons,b=>b.name=="Opción "+(answer+1)).onClick.Invoke();
        Check(Time.timeScale==1&&s.Visited==before+1,"The correct induction answer records one certificate");
        s.Player.Interact();
        Check(s.Visited==before+1&&!GameObject.Find("Mecanismo de escape"),"Repeating an accredited activity does not duplicate it");
    }

    static void CheckGroundDoors(SessionFlow session,int hall,bool capture,Action next)
    {
        var remaining=steps;
        steps=new Queue<Action<SessionFlow>>();
        for(int i=FirstGroundDoor;i<GroundDoors.Length;i++){
            int index=i;
            var point=session.World.DoorPoint(hall,index);
            steps.Enqueue(s=>{
                Use(s,point);
                Check(s.Crossing,"Ground floor door starts a transition: "+GroundDoors[index].Name);
            });
            steps.Enqueue(s=>{
                Check(s.Room==InteriorRoom(index,IsDark(hall))&&s.World.CanStand(s.Player.Position),"Separate playable interior: "+Title(s.Room));
                if(IsDark(hall)&&GroundDoors[index].Name=="Game Room"){
                    Use(s,s.World.Entry(s.Room)+Vector2.up*2);
                    Check(s.DialogueOpen,"Optional witness Nico can be spoken to in the altered Game Room");
                    FinishDialogue(s);
                }
                if(s.World.Stops(s.Room).Count>0)Use(s,s.World.Stops(s.Room)[0].Position);
                if(capture)CaptureHere(s,"02-pb-interior-"+index+".png");
                Use(s,s.World.InteriorExit(s.Room));
            });
            steps.Enqueue(s=>Check(s.Room==hall&&s.World.CanStand(s.Player.Position)&&Vector2.Distance(point,s.Player.Position)<1.2f,
                "Return to matching door: "+GroundDoors[index].Name));
        }
        steps.Enqueue(_=>next());
        foreach(var step in remaining)steps.Enqueue(step);
    }
    static void FinishDialogue(SessionFlow session)
    {
        for(int i=0;i<12&&session.DialogueOpen;i++)session.Player.Interact();
    }
    static Button DialogueChoice(string title)
    {
        var choice=Array.Find(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None),
            button=>button.name==title&&button.transform.parent.name=="Respuesta");
        Check(choice,"Dialogue exposes choice "+title);
        return choice;
    }
    static SpriteRenderer FindFramedDoor(string name)
    {
        return Array.Find(UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None),
            renderer=>renderer.name==name&&renderer.sprite&&renderer.sprite.name.StartsWith("puerta-marco-"));
    }
    static void Tick()
    {
        try{
            if(EditorApplication.timeSinceStartup-started>350)throw new Exception("Playable test timeout");
            var scene=SceneManager.GetActiveScene().name;
            if(returning&&scene=="MenuPrincipal"){
                Check(Time.timeScale==1,"Menu restores time");
                if(TestSave){
                    var ui=UnityEngine.Object.FindFirstObjectByType<BackfranzUI>();
                    ui.Extras();
                    var images=ui.extras.GetComponentsInChildren<Image>();
                    foreach(string cover in new[]{"final-amanecer","final-eco","final-umbral"})
                        Check(Array.Exists(images,i=>i.sprite==CampusLook.Sprite(cover)),"Extras shows unlocked cover: "+cover);
                    Capture("15-extras-finales.png");
                    var buttons=ui.extras.GetComponentsInChildren<Button>();
                    Array.Find(buttons,b=>b.name=="Personajes").onClick.Invoke();
                    Check(GameObject.Find("Retrato de Yamil"),"Character tab shows a portrait and profile");
                    Array.Find(ui.extras.GetComponentsInChildren<Button>(),b=>b.name=="Camila").onClick.Invoke();
                    Check(GameObject.Find("Retrato de Camila"),"Character selection updates the profile");
                    Capture("16-extras-personajes.png");
                    Array.Find(ui.extras.GetComponentsInChildren<Button>(),b=>b.name=="Más").onClick.Invoke();
                    Check(GameObject.Find("Retrato de Nuria"),"Character index advances to the next page");
                    Array.Find(buttons,b=>b.name=="Arte").onClick.Invoke();
                    Check(GameObject.Find("Arte del campus"),"Extras retains the original art gallery");
                }
                Finish(0);return;
            }
            if(loadingSave&&scene=="MenuPrincipal"){
                var ui=UnityEngine.Object.FindFirstObjectByType<BackfranzUI>();if(!ui)return;
                Check(SessionFlow.CanContinue,"Saved game is available from the main menu");
                var resume=Array.Find(ui.menu.GetComponentsInChildren<Button>(),button=>button.name=="Continuar partida");
                if(resume==null)return;
                Check(resume.interactable,"Continue is enabled for a valid save");
                resume.onClick.Invoke();loadingSave=false;return;
            }
            if(!startedGame&&scene=="MenuPrincipal"){
                var ui=UnityEngine.Object.FindFirstObjectByType<BackfranzUI>();if(!ui)return;
                Check(!ui.gameplay,"Main menu starts first");ui.Play();startedGame=true;return;
            }
            if(scene!="Nivel01"||steps.Count==0)return;
            var session=UnityEngine.Object.FindFirstObjectByType<SessionFlow>();if(!session||!session.Player)return;
            if(session.Crossing)
            {
                if(observedDoor&&observedDoor.gameObject.activeInHierarchy&&
                    observedDoor.sprite.name.EndsWith("-03",StringComparison.Ordinal))
                {
                    Check(doorPosition==observedDoor.transform.position&&doorScale==observedDoor.transform.localScale,
                        "Door animation keeps the frame in place and at full size");
                    CaptureHere(session,"02-financiero-abriendo.png");
                    observedDoor=null;filmedDoor=true;
                }
                return;
            }
            if(awaitedEscort!=null)
            {
                var actor=GameObject.Find(awaitedEscort);
                var feet=actor?(Vector2)actor.transform.position-Vector2.up*.62f:Vector2.zero;
                if(!actor||Vector2.Distance(feet,session.Player.Position)>1.60f||
                    !session.World.HasLineOfSight(feet,session.Player.Position,false))
                {
                    if(EditorApplication.timeSinceStartup>=escortWaitUntil)
                        throw new Exception("Escort did not catch up: "+awaitedEscort+" in "+Title(session.Room)+
                            " at "+(feet-session.World.Offset(session.Room))+" to "+(session.Player.Position-session.World.Offset(session.Room)));
                    return;
                }
                awaitedEscort=null;
            }
            steps.Dequeue()(session);
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code){Debug.Log("PLAYABLE RESULT: "+checks+" checks, exit "+code);EditorApplication.update-=Tick;SessionState.SetBool("Backfranz.PlayableSmoke",false);EditorApplication.Exit(code);}
    static void Capture(string file)
    {
        Directory.CreateDirectory("Docs/BloqueA/Capturas-PB");
        var camera=Camera.main;
        bool temporaryCamera=!camera;
        if(temporaryCamera){
            camera=new GameObject("Cámara de captura").AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.04f,.06f,.08f);
        }
        var target=new RenderTexture(1600,900,24);var previous=RenderTexture.active;
        var canvases=Array.FindAll(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None),
            canvas=>canvas.renderMode==RenderMode.ScreenSpaceOverlay);
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder+=3000;}
        camera.targetTexture=target;
        UnityEngine.Object.FindFirstObjectByType<SessionFlow>()?.SendMessage("LateUpdate");
        Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
        File.WriteAllBytes("Docs/BloqueA/Capturas-PB/"+file,texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder-=3000;}
        UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(target);
        if(temporaryCamera)UnityEngine.Object.Destroy(camera.gameObject);
    }
}


