using System.Collections;
using Backfranz.Playable;
using UnityEngine;
using UnityEngine.UI;

public sealed class BackfranzTransition : MonoBehaviour
{
    // Serialized artwork remains available to the title scene and gallery.
    public RawImage darkBackground;
    public Image flash;
    public Text phaseLabel;
    public BackfranzAudio sound;
    [Min(0)] public float delay = 5f;

    static readonly Vector3 StageOffset = new(900, 900, 0);
    static readonly Color Ink = new(.015f,.028f,.046f);
    RectTransform curtainLeft, curtainRight;
    RawImage display;
    Image tint, resetShade;
    Texture2D titleShade;
    Camera stageCamera;
    RenderTexture stageTexture;
    SpriteRenderer yamil, reflection, windowGlow;
    CampusWorld world;
    GameObject menu;
    Vector2[] route;
    int[] frameDurations;
    float clock, departure;
    bool leaving;

    void Start()
    {
        SetAlpha(darkBackground,0);
        SetAlpha(flash,0);
    }

    internal void BuildTitleStage(RectTransform root,RectTransform menuFrame)
    {
        menu=menuFrame.gameObject;
        var backdrop=Panel(root,"Fondo del título",Ink);
        Spread(backdrop.rectTransform,Vector2.zero,Vector2.one);
        backdrop.transform.SetSiblingIndex(menuFrame.GetSiblingIndex());

        display=Make<RawImage>(backdrop.transform,"Escena animada",Vector2.zero,Vector2.one);
        stageTexture=new RenderTexture(512,288,24,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point};
        display.texture=stageTexture;
        tint=Panel(display.transform,"Hora azul",new Color(.025f,.13f,.18f,0));
        titleShade=DrawTitleShade();
        var shade=Make<RawImage>(display.transform,"Sombra de la fachada",Vector2.zero,Vector2.one);
        shade.texture=titleShade;
        resetShade=Panel(display.transform,"Fundido de escena",new Color(Ink.r,Ink.g,Ink.b,0));

        world=new GameObject("Escena de portada").AddComponent<CampusWorld>();
        world.Show(CampusWorld.Entrance);
        route=new[]{world.Cell(0,7,9),world.Cell(0,10,9),world.StreetFoot,world.StreetLanding,world.StreetDoor+Vector2.left*.3f};
        frameDurations=new[]{62,15,50,49};
        yamil=CampusLook.Art(world.transform,"yamil-08",route[0]+Vector2.up*.53f,
            new Vector2(.93f,1.4f),"Yamil · escena de portada").GetComponent<SpriteRenderer>();
        var door=world.StreetDoor+new Vector2(.70f,.54f);
        reflection=CampusLook.Art(world.transform,"shadow-00",door,
            new Vector2(.53f,1.1f),"Reflejo en el acceso",880).GetComponent<SpriteRenderer>();
        reflection.color=new Color(.43f,.63f,.76f,0);
        windowGlow=CampusLook.Art(world.transform,"lightpool",door+new Vector2(.29f,.45f),
            new Vector2(1.7f,2.15f),"Luz detrás del vidrio",879).GetComponent<SpriteRenderer>();
        windowGlow.color=new Color(.96f,.64f,.36f,0);
        world.transform.position=StageOffset;

        stageCamera=new GameObject("Cámara del título").AddComponent<Camera>();
        stageCamera.enabled=false;
        stageCamera.orthographic=true;
        stageCamera.orthographicSize=6;
        stageCamera.clearFlags=CameraClearFlags.SolidColor;
        stageCamera.backgroundColor=Ink;
        stageCamera.allowHDR=false;
        stageCamera.allowMSAA=false;
        stageCamera.targetTexture=stageTexture;
    }

    static Texture2D DrawTitleShade()
    {
        const int width=128,height=72;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
        var pixels=new Color32[width*height];
        for(int y=0;y<height;y++)for(int x=0;x<width;x++){
            float u=(x+.5f)/width,v=(y+.5f)/height;
            float left=.94f*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.14f,.53f,u)));
            float edge=.24f*Mathf.Pow(Mathf.Abs(v-.5f)*2,2.2f);
            pixels[y*width+x]=new Color(Ink.r,Ink.g,Ink.b,1-(1-left)*(1-edge));
        }
        texture.SetPixels32(pixels);
        texture.Apply(false,true);
        return texture;
    }

    void LateUpdate()
    {
        if(!stageCamera||!menu.activeInHierarchy)return;
        float aspect=display.rectTransform.rect.width/Mathf.Max(1,display.rectTransform.rect.height);
        const float renderAspect=16f/9f;
        float visibleWidth=Mathf.Clamp01(aspect/renderAspect);
        float visibleHeight=Mathf.Clamp01(renderAspect/Mathf.Max(.01f,aspect));
        display.uvRect=new Rect((1-visibleWidth)*.68f,(1-visibleHeight)*.5f,visibleWidth,visibleHeight);
        if(!leaving)clock+=Time.unscaledDeltaTime;
        float cycle=clock%11.5f;
        AnimateWalker(cycle);
        bool reduced=PlayerPrefs.GetInt("Backfranz.ReducedFlashes",0)==1;
        float omen=reduced?0:Mathf.SmoothStep(0,1,Mathf.InverseLerp(5.4f,6.8f,cycle))*
            (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(8.1f,9.7f,cycle)));
        SetAlpha(tint,.24f+.26f*omen);
        var reflectedColor=reflection.color;
        reflectedColor.a=.52f*omen;
        reflection.color=reflectedColor;
        var glow=windowGlow.color;
        float wavering=Mathf.PerlinNoise(cycle*3.5f,2.17f);
        glow.a=reduced?.38f:Mathf.Lerp(.31f,.51f,wavering)*(1-.84f*omen);
        windowGlow.color=glow;
        float loopShade=Mathf.Max(1-Mathf.Clamp01(cycle/.65f),Mathf.InverseLerp(10.65f,11.5f,cycle));
        SetAlpha(resetShade,Mathf.Max(loopShade,departure));
        if(leaving&&departure>=.98f)return;
        stageCamera.Render();
    }

    void AnimateWalker(float cycle)
    {
        float travel=Mathf.Max(0,cycle-1.15f)*32;
        int segment=0;
        while(segment<frameDurations.Length-1&&travel>frameDurations[segment]){
            travel-=frameDurations[segment];
            segment++;
        }
        float progress=Mathf.Clamp01(travel/frameDurations[segment]);
        Vector2 feet=Vector2.Lerp(route[segment],route[segment+1],progress);
        Vector2 direction=route[segment+1]-route[segment];
        bool walking=cycle>1.15f&&progress<1;
        yamil.sprite=walking?CampusLook.YamilWalk(direction,Mathf.FloorToInt(cycle*10)%6):CampusLook.Student(direction,0);
        yamil.transform.position=StageOffset+(Vector3)(feet+Vector2.up*.53f);
        yamil.sortingOrder=CampusLook.Depth(feet.y-.06f);
        Vector2 cameraCenter=Vector2.Lerp(world.Cell(0,8,13),world.Cell(0,10,13),Mathf.SmoothStep(0,1,cycle/8));
        cameraCenter=new Vector2(Mathf.Round(cameraCenter.x*24)/24,Mathf.Round(cameraCenter.y*24)/24);
        stageCamera.transform.position=StageOffset+new Vector3(cameraCenter.x,cameraCenter.y,-10);
    }

    internal IEnumerator PlayDeparture()
    {
        leaving=true;
        sound.Lift();
        var cover=Panel(transform,"Cierre del ascensor",Color.clear);
        cover.raycastTarget=true;
        Spread(cover.rectTransform,Vector2.zero,Vector2.one);
        curtainLeft=Panel(cover.transform,"Hoja izquierda",Ink).rectTransform;
        curtainRight=Panel(cover.transform,"Hoja derecha",Ink).rectTransform;
        sound.LiftDoors(false);
        const float duration=.72f;
        for(float elapsed=0;elapsed<duration;elapsed+=Time.unscaledDeltaTime){
            CloseCurtain(Mathf.SmoothStep(0,1,elapsed/duration));
            yield return null;
        }
        CloseCurtain(1);
    }

    void CloseCurtain(float progress)
    {
        departure=progress;
        Spread(curtainLeft,new Vector2(0,0),new Vector2(.5f*progress,1));
        Spread(curtainRight,new Vector2(1-.5f*progress,0),Vector2.one);
    }

    static T Make<T>(Transform parent,string name,Vector2 min,Vector2 max) where T:Graphic
    {
        var child=new GameObject(name,typeof(RectTransform),typeof(T));
        child.transform.SetParent(parent,false);
        var graphic=child.GetComponent<T>();
        graphic.raycastTarget=false;
        Spread(graphic.rectTransform,min,max);
        return graphic;
    }

    static Image Panel(Transform parent,string name,Color color)
    {
        var panel=Make<Image>(parent,name,Vector2.zero,Vector2.one);
        panel.color=color;
        return panel;
    }

    static void Spread(RectTransform rect,Vector2 min,Vector2 max)
    {
        rect.anchorMin=min;
        rect.anchorMax=max;
        rect.offsetMin=rect.offsetMax=Vector2.zero;
    }

    static void SetAlpha(Graphic graphic,float alpha)
    {
        if(!graphic)return;
        var color=graphic.color;
        color.a=alpha;
        graphic.color=color;
    }

    void OnDestroy()
    {
        if(stageCamera){stageCamera.targetTexture=null;Destroy(stageCamera.gameObject);}
        if(world)Destroy(world.gameObject);
        if(stageTexture){stageTexture.Release();Destroy(stageTexture);}
        if(titleShade)Destroy(titleShade);
    }
}
