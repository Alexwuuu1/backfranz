using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Backfranz.Playable
{
    public sealed class SessionFlow : MonoBehaviour
    {
        public CampusWorld World {get;private set;}
        public ExplorerMotor Player {get;private set;}
        public int Room {get;private set;}
        public int Collected {get;private set;}
        public bool Finished {get;private set;}
        public bool Won {get;private set;}
        public bool HasCard {get;private set;}
        public bool HasPower=>World && World.GateOpen;
        readonly bool[] collected=new bool[3];
        BackfranzUI pause;CampusAtmosphere atmosphere;
        Text objective,location,hint;Image cardIcon,powerIcon,vignette;
        GameObject ending,touch,hintPanel;
        Transform enemy;
        float grace,routeAt,alertUntil,noticeUntil;
        readonly Queue<Vector2> route=new Queue<Vector2>();
        Vector2 patrolTarget;
        Font font;
        string notice="";
        public Vector2 EnemyPosition=>(Vector2)enemy.position;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
        static void Loaded(Scene scene,LoadSceneMode mode)
        {if(scene.name=="Nivel01")new GameObject("Backfranz · partida").AddComponent<SessionFlow>();}
        void Start()
        {
            pause=FindFirstObjectByType<BackfranzUI>();font=pause.status.font;
            foreach(var root in gameObject.scene.GetRootGameObjects())
                if(root!=gameObject&&!root.GetComponentInChildren<Camera>()&&!root.GetComponentInChildren<Canvas>()&&!root.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>())root.SetActive(false);
            foreach(var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))renderer.enabled=false;
            foreach(var motor in FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None))motor.enabled=false;
            World=gameObject.AddComponent<CampusWorld>();World.Build();atmosphere=gameObject.AddComponent<CampusAtmosphere>();
            var actor=CampusLook.Art(transform,"yamil-00",World.Entry(0),new Vector2(.8f,1.2f),"Yamil");
            Player=actor.AddComponent<ExplorerMotor>();Player.world=World;Player.session=this;
            enemy=CampusLook.Art(transform,"shadow-00",Vector2.zero,new Vector2(.9f,1.35f),"La sombra").transform;
            CreateHUD();Enter(0);
        }
        public void Enter(int room)
        {
            Room=room;Player.transform.position=World.Entry(room);grace=3;alertUntil=0;route.Clear();routeAt=0;
            var camera=Camera.main;camera.orthographic=true;camera.orthographicSize=5.8f;
            atmosphere.SetRoom(room);enemy.gameObject.SetActive(room>0);enemy.position=World.EnemySpawn(room);patrolTarget=World.Door(room);
            vignette.color=new Color(1,1,1,room==0?.35f:1);SnapCamera();RefreshHUD();
        }
        bool Near(Vector2 p,float radius=1.2f)=>Vector2.Distance(Player.Position,p)<radius&&World.HasLineOfSight(Player.Position,p);
        public void Interact()
        {
            if(Finished||Time.timeScale==0)return;
            if(Room==0&&!HasCard&&Near(World.Card)){
                HasCard=true;World.CardView.SetActive(false);pause.sound.Click();Notify("Tarjeta de acceso");RefreshHUD();return;
            }
            if(Room==1&&!HasPower&&Near(World.PowerSwitch)){
                World.RestorePower();pause.sound.Click();Notify("Energía restablecida");RefreshHUD();return;
            }
            if(!collected[Room]&&Near(World.Notes[Room])){
                collected[Room]=true;Collected++;World.NoteViews[Room].SetActive(false);pause.sound.Click();Notify("Apunte recuperado");RefreshHUD();return;
            }
            if(Near(World.Door(Room),1.5f)){
                if(!collected[Room]){Notify("Falta un apunte");return;}
                if(Room==0&&!HasCard){Notify("Necesitas la tarjeta");return;}
                if(Room==1&&!HasPower){Notify("Activa la energía");return;}
                if(Room==2){Finish(true);return;}
                Enter(Room+1);pause.sound.Thunder();
            }
        }
        void Notify(string text){notice=text;noticeUntil=Time.unscaledTime+2.3f;}
        void Update()
        {
            if(!Player)return;
            bool playing=!Finished&&Time.timeScale>0;touch.SetActive(playing&&Application.isMobilePlatform);RefreshHUD();
            if(!playing||Room==0)return;
            grace-=Time.deltaTime;if(grace>0)return;
            if(Vector2.Distance(enemy.position,Player.Position)<7&&World.HasLineOfSight(enemy.position,Player.Position))alertUntil=Time.time+4;
            if(Time.time>=routeAt){
                route.Clear();Vector2 goal=Time.time<alertUntil?Player.Position:patrolTarget;
                foreach(var p in World.FindRoute(enemy.position,goal))route.Enqueue(p);
                if(Time.time>=alertUntil&&Vector2.Distance(enemy.position,patrolTarget)<.5f)patrolTarget=Vector2.Distance(patrolTarget,World.Door(Room))<1?World.EnemySpawn(Room):World.Door(Room);
                routeAt=Time.time+.65f;
            }
            if(route.Count>0){Vector2 target=route.Peek();enemy.position=Vector2.MoveTowards(enemy.position,target,(Time.time<alertUntil?2.5f:1.3f)*Time.deltaTime);if(Vector2.Distance(enemy.position,target)<.05f)route.Dequeue();}
            var renderer=enemy.GetComponent<SpriteRenderer>();renderer.sortingOrder=CampusLook.Depth(enemy.position.y);renderer.sprite=CampusLook.Sprite("shadow-"+((int)(Time.time*6)%4).ToString("00"));
            if(Vector2.Distance(enemy.position,Player.Position)<.55f)Finish(false);
        }
        void LateUpdate(){if(Player&&!Finished)SnapCamera();}
        void SnapCamera()
        {
            var camera=Camera.main;Vector2 o=World.Offset(Room);float halfWidth=camera.orthographicSize*camera.aspect;
            float x=halfWidth>=16?o.x-.5f:Mathf.Clamp(Player.Position.x,o.x-16+halfWidth,o.x+15-halfWidth);
            float y=Mathf.Clamp(Player.Position.y,-10+camera.orthographicSize,9-camera.orthographicSize);
            camera.transform.position=new Vector3(Mathf.Round(x*32)/32,Mathf.Round(y*32)/32,-10);
        }
        void RefreshHUD()
        {
            objective.text=Collected+" / 3";location.text=CampusWorld.Names[Room];cardIcon.color=HasCard?Color.white:new Color(1,1,1,.24f);powerIcon.color=HasPower?Color.white:new Color(1,1,1,.24f);
            string prompt="";
            if(!Finished&&Time.timeScale>0){
                if(Time.unscaledTime<noticeUntil)prompt=notice;
                else if(Room==0&&!HasCard&&Near(World.Card))prompt=Application.isMobilePlatform?"Recoger tarjeta":"E · Recoger tarjeta";
                else if(Room==1&&!HasPower&&Near(World.PowerSwitch))prompt=Application.isMobilePlatform?"Activar energía":"E · Activar energía";
                else if(!collected[Room]&&Near(World.Notes[Room]))prompt=Application.isMobilePlatform?"Recoger apunte":"E · Recoger apunte";
                else if(Near(World.Door(Room),1.5f))prompt=Room==2?"Salir":"Usar ascensor";
            }
            hint.text=prompt;hintPanel.SetActive(prompt.Length>0);
        }
        public void Finish(bool won)
        {
            if(Finished)return;Finished=true;Won=won;Time.timeScale=0;touch.SetActive(false);hintPanel.SetActive(false);pause.menu.SetActive(false);pause.pauseButton.SetActive(false);pause.enabled=false;
            ending.SetActive(true);ending.GetComponentInChildren<Text>().text=won?"Lograste escapar":"La sombra te encontró";
        }
        public void Restart(){Time.timeScale=1;SceneManager.LoadScene("Nivel01");}
        public void Menu(){Time.timeScale=1;SceneManager.LoadScene("MenuPrincipal");}
        void OnDestroy(){Time.timeScale=1;}
        void CreateHUD()
        {
            var root=new GameObject("Partida · HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform);
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=-1;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=1;
            vignette=Icon(root.transform,"vignette",Vector2.zero,Vector2.one);vignette.preserveAspect=false;
            var safe=Rect(root.transform,"Área segura",Vector2.zero,Vector2.one);
            var area=Screen.safeArea;safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
            var top=Panel(safe,"Estado",new Vector2(.02f,.9f),new Vector2(.42f,.98f));
            location=Label(top,"Zona",new Vector2(.03f,.1f),new Vector2(.45f,.9f),24);
            Icon(top,"note",new Vector2(.47f,.12f),new Vector2(.56f,.88f));
            objective=Label(top,"Apuntes",new Vector2(.57f,.1f),new Vector2(.72f,.9f),24);
            cardIcon=Icon(top,"card",new Vector2(.76f,.15f),new Vector2(.85f,.85f));powerIcon=Icon(top,"switch-on",new Vector2(.88f,.12f),new Vector2(.97f,.88f));
            hintPanel=Panel(safe,"Interacción",new Vector2(.34f,.06f),new Vector2(.66f,.14f)).gameObject;
            hint=Label(hintPanel.transform,"Acción",new Vector2(.03f,.05f),new Vector2(.97f,.95f),25);
            touch=Rect(safe,"Controles táctiles",Vector2.zero,Vector2.one).gameObject;
            Direction(new Vector2(.025f,.095f),Vector2.left,90);Direction(new Vector2(.155f,.095f),Vector2.right,-90);Direction(new Vector2(.09f,.20f),Vector2.up,0);Direction(new Vector2(.09f,.015f),Vector2.down,180);
            var interact=Button(touch.transform,"",new Vector2(.85f,.08f),new Vector2(.97f,.25f),Player.Interact);Icon(interact.transform,"interact",new Vector2(.15f,.15f),new Vector2(.85f,.85f));
            touch.SetActive(Application.isMobilePlatform);
            ending=Panel(safe,"Resultado",new Vector2(.25f,.25f),new Vector2(.75f,.75f)).gameObject;
            Label(ending.transform,"Resultado",new Vector2(.06f,.64f),new Vector2(.94f,.92f),34);
            Button(ending.transform,"Reintentar",new Vector2(.12f,.35f),new Vector2(.88f,.53f),Restart);
            Button(ending.transform,"Menú principal",new Vector2(.12f,.1f),new Vector2(.88f,.28f),Menu);ending.SetActive(false);
        }
        void Direction(Vector2 anchor,Vector2 axis,float angle)
        {
            var button=Button(touch.transform,"",anchor,anchor+new Vector2(.06f,.095f),null);
            var image=Icon(button.transform,"arrow-up",new Vector2(.18f,.18f),new Vector2(.82f,.82f));image.rectTransform.localEulerAngles=new Vector3(0,0,angle);
            var direction=button.gameObject.AddComponent<TouchDirection>();direction.motor=Player;direction.direction=axis;
        }
        RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        RectTransform Panel(Transform parent,string name,Vector2 min,Vector2 max)
        {var r=Rect(parent,name,min,max);var image=r.gameObject.AddComponent<Image>();image.sprite=CampusLook.Sprite("panel");image.type=Image.Type.Sliced;return r;}
        Image Icon(Transform parent,string asset,Vector2 min,Vector2 max)
        {var r=Rect(parent,asset,min,max);var image=r.gameObject.AddComponent<Image>();image.sprite=CampusLook.Sprite(asset);image.preserveAspect=true;image.raycastTarget=false;return image;}
        Text Label(Transform parent,string name,Vector2 min,Vector2 max,int size)
        {var t=Rect(parent,name,min,max).gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.color=new Color(.96f,.91f,.76f);t.raycastTarget=false;t.alignment=TextAnchor.MiddleCenter;return t;}
        Button Button(Transform parent,string title,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
        {
            var r=Rect(parent,title,min,max);var image=r.gameObject.AddComponent<Image>();image.sprite=CampusLook.Sprite("button");image.type=Image.Type.Sliced;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=new Color(1,.85f,.55f);colors.pressedColor=new Color(.7f,.8f,.8f);b.colors=colors;
            if(title.Length>0)Label(r,title,new Vector2(.04f,.02f),new Vector2(.96f,.98f),28).text=title;
            if(action!=null)b.onClick.AddListener(action);return b;
        }
    }
}
