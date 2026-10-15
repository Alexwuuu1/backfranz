using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Alex Joel: reproducible smoke checks against the actual saved scenes.
[InitializeOnLoad]
public static class BackfranzSmoke
{
    static int step;
    static double started;
    static BackfranzSmoke() { EditorApplication.playModeStateChanged+=Changed; }
    public static void Run()
    {
        Hito3Setup.Build();
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
                Directory.CreateDirectory("Docs/Hito3/Capturas");
                Capture("Docs/Hito3/Capturas/01-menu-dia.png");step++;
            } else if(step==1 && t>7) {
                Check(UnityEngine.Object.FindFirstObjectByType<BackfranzTransition>().Completed,"Five second transformation completes");
                Capture("Docs/Hito3/Capturas/02-menu-backrooms.png");
                ui.Options();Check(ui.options.activeInHierarchy,"Options opens");ui.Volume(.5f);Check(Mathf.Approximately(AudioListener.volume,.5f),"Volume applies");ui.ClosePanel();
                ui.Extras();Check(ui.extras.activeInHierarchy,"Extras gallery opens");ui.ClosePanel();
                ui.Credits();Check(ui.credits.activeInHierarchy,"Credits opens");ui.ClosePanel();
                ui.AskQuit();Check(ui.confirmation.activeInHierarchy,"Quit confirmation opens");ui.ClosePanel();step++;
            } else if(step==2 && t>8) {ui.Play();step++;}
            else if(step==3 && SceneManager.GetActiveScene().name=="Nivel01" && t>10) {
                Check(ui.gameplay,"Play loads Nivel01");ui.Pause();Check(Time.timeScale==0,"Pause freezes time");
                Capture("Docs/Hito3/Capturas/03-pausa.png");step++;
            } else if(step==4 && t>12) {
                ui.Options();Check(ui.options.activeInHierarchy,"Options during pause");ui.ClosePanel();Check(Time.timeScale==0,"Closing options preserves pause");
                ui.Resume();Check(Time.timeScale==1,"Resume restores time");ui.Pause();ui.MainMenu();step++;
            } else if(step==5 && SceneManager.GetActiveScene().name=="MenuPrincipal" && t>14) {
                Check(Time.timeScale==1,"Returning to menu resets time");
                File.WriteAllText("Docs/Hito3/VERIFICACION.txt","Unity 6000.3.16f1: smoke checks PASS. Menu, transformation, options, extras, credits, quit confirmation, scene load, pause/resume and return verified.\nKeyboard, final Windows quit and visual layout require manual review.\n");
                Finish(0);
            }
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code) { EditorApplication.update-=Tick;SessionState.SetBool("Backfranz.Smoke",false);EditorApplication.Exit(code); }
    static void Capture(string path)
    {
        var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
        var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
        var target=new RenderTexture(1600,900,24);
        var previous=RenderTexture.active;
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder=1000;
        camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
        File.WriteAllBytes(path,texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(target);
    }
}
