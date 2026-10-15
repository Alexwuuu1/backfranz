using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Hito3Setup
{
    const string Art="Assets/Art/UI/Backfranz/";
    static Font font;
    static Color ink=new Color(.035f,.065f,.09f,.96f);
    static Color cream=new Color(.98f,.93f,.8f);
    static Color amber=new Color(1f,.64f,.15f);

    [MenuItem("Tools/Backfranz/Preparar menú y pausa")]
    public static void Build()
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        font=AssetDatabase.LoadAssetAtPath<Font>(Art+"BackfranzPixel.ttf");
        foreach(string f in new[]{"campus-normal.png","campus-backrooms.png","backfranz-logo.png"}) {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Art+f);
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.filterMode=FilterMode.Point;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>().backgroundColor=Color.black;
        CreateUI(false);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/MenuPrincipal.unity");
        EditorSceneManager.OpenScene("Assets/Scenes/Nivel01.unity");
        var old=GameObject.Find("Backfranz UI"); if(old) UnityEngine.Object.DestroyImmediate(old);
        foreach(var e in UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None)) UnityEngine.Object.DestroyImmediate(e.gameObject);
        if(!UnityEngine.Object.FindFirstObjectByType<AudioListener>()) Camera.main.gameObject.AddComponent<AudioListener>();
        CreateUI(true);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MenuPrincipal.unity",true),new EditorBuildSettingsScene("Assets/Scenes/Nivel01.unity",true)};
        PlayerSettings.productName="Backfranz";PlayerSettings.companyName="Grupo 64";
        PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity");
        Debug.Log("BACKFRANZ_SETUP_OK");
    }
    static void CreateUI(bool game)
    {
        var root=new GameObject("Backfranz UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(BackfranzAudio),typeof(BackfranzUI));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        var ui=root.GetComponent<BackfranzUI>();ui.gameplay=game;ui.sound=root.GetComponent<BackfranzAudio>();
        RawImage dark=null;
        if(!game) { Raw("Campus · día",root.transform,"campus-normal.png",0,0,1600,900);dark=Raw("Campus · backrooms",root.transform,"campus-backrooms.png",0,0,1600,900);dark.color=new Color(1,1,1,0); }
        ui.menu=Box(game?"Pausa":"Menú principal",root.transform,0,0,570,900,ink).gameObject;
        Raw("Título Backfranz",ui.menu.transform,"backfranz-logo.png",25,45,520,173);
        Text phase=Label("Estado del campus",ui.menu.transform,game?"PAUSA":"TU PRIMER DÍA EMPIEZA AQUÍ",40,235,490,40,22,amber);
        Label("Identidad",ui.menu.transform,"EXPLORACIÓN  /  TERROR UNIVERSITARIO",40,278,490,30,15,cream);
        float y=355;
        if(game) {
            ui.firstButton=Button("Continuar",ui.menu.transform,y,ui.Resume); y+=72;
            Button("Opciones",ui.menu.transform,y,ui.Options);y+=72;
            Button("Volver al menú",ui.menu.transform,y,ui.MainMenu);y+=72;
        } else {
            ui.firstButton=Button("Jugar",ui.menu.transform,y,ui.Play);y+=72;
            Button("Opciones",ui.menu.transform,y,ui.Options);y+=72;
            Button("Extras",ui.menu.transform,y,ui.Extras);y+=72;
            Button("Créditos",ui.menu.transform,y,ui.Credits);y+=72;
        }
        Button("Salir",ui.menu.transform,y,ui.AskQuit);
        Label("Controles",ui.menu.transform,"FLECHAS + ENTER  ·  RATÓN\nESC: VOLVER / PAUSA",40,770,490,55,17,cream);
        Label("Autoría",ui.menu.transform,"PROYECTO ACADÉMICO · GRUPO 64",40,850,490,25,15,amber);
        ui.status=Label("Estado",root.transform,"",600,840,900,45,22,cream);
        ui.pauseButton=ButtonAt("Pausa [Esc]",root.transform,1350,25,220,55,ui.Pause).gameObject;
        ui.pauseButton.SetActive(game);
        ui.options=Modal("OPCIONES",root.transform);
        Label("Volumen",ui.options.transform,"VOLUMEN GENERAL",55,110,750,35,24,cream);
        var track=Box("Volumen",ui.options.transform,55,170,780,35,new Color(.2f,.25f,.29f));
        var slider=track.gameObject.AddComponent<Slider>();ui.volume=slider;
        var handle=Box("Control",track.transform,0,-7,25,49,amber);
        var area=new GameObject("Área de control",typeof(RectTransform));area.transform.SetParent(track.transform,false);
        var ar=(RectTransform)area.transform;ar.anchorMin=Vector2.zero;ar.anchorMax=Vector2.one;ar.offsetMin=new Vector2(13,0);ar.offsetMax=new Vector2(-13,0);
        handle.transform.SetParent(area.transform,false);slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.value=.7f;
        UnityEventTools.AddPersistentListener(slider.onValueChanged,ui.Volume);
        ui.fullscreen=ToggleAt("Pantalla completa (en la aplicación)",ui.options.transform,260,ui.Fullscreen);
        ui.reducedFlashes=ToggleAt("Reducir destellos de la transición",ui.options.transform,335,ui.ReducedFlashes);
        Label("Nota opciones",ui.options.transform,"Las preferencias se guardan en este equipo.\nEn el editor, prueba pantalla completa en una compilación.",55,410,790,80,20,cream);
        Back(ui.options,ui);
        ui.credits=Modal("CRÉDITOS",root.transform);
        Label("Equipo",ui.credits.transform,"GRUPO 64\n\nAlejandro Villalpando Rojas\nCristopher Iori Lazcano Gutierrez\nGalilea Alison Llusco Asistiri\nAlex Joel Quispe Ticona\n\nReferencias del menú: Galileya\nAdaptación visual: equipo con asistencia de IA\nMotor: Unity 6000.3.16f1",55,105,790,400,25,cream);Back(ui.credits,ui);
        ui.extras=Modal("EXTRAS · DOS REALIDADES",root.transform);
        Raw("Galería día",ui.extras.transform,"campus-normal.png",40,130,400,225);
        Raw("Galería noche",ui.extras.transform,"campus-backrooms.png",460,130,400,225);
        Label("Descripción",ui.extras.transform,"La misma universidad. Dos realidades.\n\nReferencias de Galileya adaptadas al universo de Backfranz.\nYamil explora un campus que deja de parecer familiar.",45,390,810,130,23,cream);Back(ui.extras,ui);
        ui.confirmation=Modal("¿SALIR DE BACKFRANZ?",root.transform);
        Label("Confirmación",ui.confirmation.transform,"En el editor se detendrá el modo Play.\nEn Windows se cerrará la aplicación.",55,150,790,140,28,cream);
        ButtonAt("Sí, salir",ui.confirmation.transform,55,370,370,70,ui.Quit);
        ButtonAt("Cancelar",ui.confirmation.transform,470,370,370,70,ui.ClosePanel);
        foreach(var panel in new[]{ui.options,ui.credits,ui.extras,ui.confirmation})panel.transform.parent.gameObject.SetActive(false);
        if(game) ui.menu.SetActive(false);
        else {
            var flash=Box("Relámpago · no bloquea botones",root.transform,0,0,1600,900,new Color(1,1,1,0));flash.raycastTarget=false;
            var transition=root.AddComponent<BackfranzTransition>();transition.darkBackground=dark;transition.flash=flash;transition.phaseLabel=phase;transition.sound=ui.sound;
        }
    }
    static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
    {
        var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var r=(RectTransform)g.transform;
        r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
    }
    static Image Box(string name,Transform parent,float x,float y,float w,float h,Color color) {var r=Rect(name,parent,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=color;return image;}
    static RawImage Raw(string name,Transform parent,string file,float x,float y,float w,float h) {var r=Rect(name,parent,x,y,w,h);var image=r.gameObject.AddComponent<RawImage>();image.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+file);image.raycastTarget=false;return image;}
    static Text Label(string name,Transform parent,string value,float x,float y,float w,float h,int size,Color color) {var r=Rect(name,parent,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
    static Button Button(string text,Transform parent,float y,UnityAction action) {return ButtonAt(text,parent,40,y,490,58,action);}
    static Button ButtonAt(string text,Transform parent,float x,float y,float w,float h,UnityAction action)
    {
        var image=Box(text,parent,x,y,w,h,new Color(.12f,.18f,.22f));var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;
        var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=amber;colors.selectedColor=amber;colors.pressedColor=new Color(.7f,.4f,.1f);colors.fadeDuration=.08f;b.colors=colors;
        Label("Texto",image.transform,text.ToUpperInvariant(),20,12,w-35,h-12,25,cream);
        UnityEventTools.AddPersistentListener(b.onClick,action);return b;
    }
    static GameObject Modal(string title,Transform parent)
    {
        var backdrop=Box(title+" · fondo modal",parent,0,0,1600,900,new Color(0,0,0,.75f));
        // The returned content is inside a full screen raycast blocker; toggle both together.
        var content=Box("Panel",backdrop.transform,350,125,900,650,ink);
        Label("Título",content.transform,title,45,35,810,60,34,amber);
        // Modal children use a 900x650 local coordinate frame.
        return content.gameObject;
    }
    static void Back(GameObject panel,BackfranzUI ui) {ButtonAt("Volver",panel.transform,55,550,790,60,ui.ClosePanel);}
    static Toggle ToggleAt(string text,Transform parent,float y,UnityAction<bool> action)
    {
        var r=Rect(text,parent,55,y,790,55);var toggle=r.gameObject.AddComponent<Toggle>();
        var box=Box("Casilla",r,0,0,42,42,new Color(.2f,.25f,.29f));var check=Box("Marcado",box.transform,8,8,26,26,amber);
        toggle.targetGraphic=box;toggle.graphic=check;Label("Texto",r,text,65,4,720,50,24,cream);UnityEventTools.AddPersistentListener(toggle.onValueChanged,action);return toggle;
    }
}
