using System;
using System.IO;
using Backfranz.Playable;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Alex Joel: reproducible smoke checks against the actual saved scenes.
[InitializeOnLoad]
public static class BackfranzSmoke
{
    static int step;
    static double started;
    static Vector3 titleStart;
    static BackfranzSmoke() { EditorApplication.playModeStateChanged+=Changed; }
    public static void Run()
    {
        Hito3Setup.Build();
        SessionState.SetBool("Backfranz.Smoke",true);
        EditorApplication.isPlaying=true;
    }
    // Validate the scenes actually committed by the team without rebuilding them.
    public static void RunSaved()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity");
        SessionState.SetBool("Backfranz.Smoke",true);
        EditorApplication.isPlaying=true;
    }
    static void Changed(PlayModeStateChange change)
    {
        if(!SessionState.GetBool("Backfranz.Smoke",false))return;
        if(change==PlayModeStateChange.EnteredPlayMode) {step=0;started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}
    }
    static void Check(bool ok,string message) { if(!ok)throw new Exception("SMOKE FAIL: "+message);Debug.Log("SMOKE PASS: "+message); }
    static void Tick()
    {
        try {
            double t=EditorApplication.timeSinceStartup-started;
            if(t>50)throw new Exception("Smoke timeout");
            var ui=UnityEngine.Object.FindFirstObjectByType<BackfranzUI>();
            if(!ui)return;
            if(step==0 && t>1) {
                Check(SceneManager.GetActiveScene().name=="MenuPrincipal","Initial menu scene");
                Check(ui.menu.activeSelf,"Main menu visible");
                Check(Array.Exists(ui.sound.GetComponents<AudioSource>(),source=>source.loop&&source.clip&&
                    source.clip.name=="musica-menu"),"The original title theme is assigned to a looping source");
                var walker=GameObject.Find("Yamil · escena de portada");
                Check(walker,"The title builds the animated campus scene");
                var backdrop=GameObject.Find("Escena animada").GetComponent<RawImage>().rectTransform;
                Check(backdrop.anchorMin==Vector2.zero&&backdrop.anchorMax==Vector2.one&&
                    ui.menu.GetComponent<Image>().color.a==0&&ui.firstButton.image.color.a==0,
                    "The title fills the viewport with transparent menu controls");
                titleStart=walker.transform.position;
                Directory.CreateDirectory("Docs/Hito3/Capturas");
                Capture("Docs/Hito3/Capturas/01-menu-dia.png");step++;
            } else if(step==1 && t>7) {
                var transition=UnityEngine.Object.FindFirstObjectByType<BackfranzTransition>();
                Check(transition.darkBackground.color.a==0 && transition.flash.color.a==0,"Main menu stays in daylight");
                Check(Vector3.Distance(GameObject.Find("Yamil · escena de portada").transform.position,titleStart)>1,
                    "Yamil walks across the title scene");
                Capture("Docs/Hito3/Capturas/05-menu-umbral.png");
                ui.Options();Check(ui.options.activeInHierarchy,"Options opens");
                const string volumeKey = "Backfranz.Volume";
                bool hadVolume = PlayerPrefs.HasKey(volumeKey);
                float previousVolume = AudioListener.volume;
                try
                {
                    ui.Volume(.5f);
                    Check(Mathf.Approximately(AudioListener.volume,.5f),"Volume applies");
                }
                finally
                {
                    ui.Volume(previousVolume);
                    if(!hadVolume) PlayerPrefs.DeleteKey(volumeKey);
                }
                ui.ClosePanel();
                ui.Extras();Check(ui.extras.activeInHierarchy,"Extras gallery opens");ui.ClosePanel();
                ui.Credits();Check(ui.credits.activeInHierarchy,"Credits opens");ui.ClosePanel();
                ui.AskQuit();Check(ui.confirmation.activeInHierarchy,"Quit confirmation opens");ui.ClosePanel();step++;
            } else if(step==2 && t>8) {
                ui.Play();
                Check(GameObject.Find("Cierre del ascensor"),"New Game starts the title transition before loading");
                step++;
            }
            else if(step==3 && SceneManager.GetActiveScene().name=="Nivel01" && t>10) {
                Check(ui.gameplay,"Play loads Nivel01");
                var session=UnityEngine.Object.FindFirstObjectByType<SessionFlow>();
                var voice=Array.Find(ui.sound.GetComponents<AudioSource>(),source=>source.clip&&source.clip.name=="dialogo-yamil");
                Check(voice&&voice.clip.length<.061f,"Visible dialogue produces a short synthesized voice pulse");
                session.Player.Interact();
                Check(!voice.isPlaying,"Skipping the dialogue text stops the voice pulse");
                ui.Pause();
                Check(!ui.menu.activeSelf,"Pause does not cover the opening dialogue");
                Check(GameObject.Find("Cartela de apertura"),"The first day has its own opening caption");
                Capture("Docs/Hito3/Capturas/02-apertura.png");
                for(int i=0;i<8&&session.DialogueOpen;i++)session.Player.Interact();
                Check(!session.DialogueOpen,"The opening conversation returns control");
                Check(!GameObject.Find("Cartela de apertura"),"The opening caption clears with the dialogue");
                ui.Pause();Check(Time.timeScale==0,"Pause freezes time");
                Check(Array.Exists(ui.menu.GetComponentsInChildren<Text>(),text=>text.text.Contains("Acceso exterior")),
                    "Pause identifies the current area");
                Capture("Docs/Hito3/Capturas/03-pausa.png");step++;
            } else if(step==4 && t>12) {
                ui.Options();Check(ui.options.activeInHierarchy,"Options during pause");ui.ClosePanel();Check(Time.timeScale==0,"Closing options preserves pause");
                ui.Resume();Check(Time.timeScale==1,"Resume restores time");ui.Pause();ui.MainMenu();
                var cancel=GameObject.Find("Cancelar");
                Check(cancel&&Time.timeScale==0&&!ui.menu.activeSelf,"Returning to menu asks while the game stays frozen");
                Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==cancel,"Return dialog focuses Cancel");
                Capture("Docs/Hito3/Capturas/04-volver-al-menu.png");
                cancel.GetComponent<Button>().onClick.Invoke();
                Check(ui.menu.activeSelf&&Time.timeScale==0,"Cancel returns to pause");
                ui.MainMenu();
                string action=Array.Exists(Environment.GetCommandLineArgs(),arg=>arg=="-backfranzSmokeSave")?
                    "Guardar y salir":"Salir sin guardar";
                GameObject.Find(action).GetComponent<Button>().onClick.Invoke();
                step++;
            } else if(step==5 && SceneManager.GetActiveScene().name=="MenuPrincipal" && t>14) {
                Check(Time.timeScale==1,"Returning to menu resets time");
                if(Array.Exists(Environment.GetCommandLineArgs(),arg=>arg=="-backfranzSmokeSave"))
                    Check(SessionFlow.CanContinue,"Saving and returning leaves a resumable checkpoint");
                File.WriteAllText("Docs/Hito3/VERIFICACION.txt","Unity 6000.3.16f1: smoke checks PASS. Daylight menu, options, extras, credits, quit confirmation, scene load, pause/resume and return verified.\nKeyboard, final Windows quit and visual layout require manual review.\n");
                Finish(0);
            }
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code) { EditorApplication.update-=Tick;SessionState.SetBool("Backfranz.Smoke",false);EditorApplication.Exit(code); }
    static void Capture(string path)
    {
        var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var camera=Camera.main;
        if(!camera)camera=Array.Find(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None),candidate=>candidate.isActiveAndEnabled);
        var target=new RenderTexture(1600,900,24);
        var previous=RenderTexture.active;
        foreach(var canvas in canvases)
        {
            canvas.renderMode=RenderMode.ScreenSpaceCamera;
            canvas.worldCamera=camera;
            canvas.planeDistance=1;
            canvas.sortingOrder+=3000;
        }
        camera.targetTexture=target;
        UnityEngine.Object.FindFirstObjectByType<SessionFlow>()?.SendMessage("LateUpdate");
        Canvas.ForceUpdateCanvases();
        UnityEngine.Object.FindFirstObjectByType<UiPolish>()?.SendMessage("Update");
        Canvas.ForceUpdateCanvases();
        camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
        File.WriteAllBytes(path,texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;
        foreach(var canvas in canvases)
        {
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder-=3000;
        }
        UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(target);
    }
}
