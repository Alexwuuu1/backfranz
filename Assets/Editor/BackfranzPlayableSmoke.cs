using System;
using System.IO;
using Backfranz.Playable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BackfranzPlayableSmoke
{
    static int stage,checks;static double started;
    static BackfranzPlayableSmoke(){EditorApplication.playModeStateChanged+=Changed;}
    public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Nivel01.unity");SessionState.SetBool("Backfranz.PlayableSmoke",true);EditorApplication.isPlaying=true;}
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("Backfranz.PlayableSmoke",false))return;
        started=EditorApplication.timeSinceStartup;stage=checks=0;EditorApplication.update+=Tick;
    }
    static void Check(bool value,string label){if(!value)throw new Exception("PLAYABLE FAIL: "+label);checks++;Debug.Log("PLAYABLE PASS: "+label);}
    static void Pickup(SessionFlow s,Vector2 point){s.Player.transform.position=point;s.Interact();}
    static void Tick()
    {
        try{
            if(EditorApplication.timeSinceStartup-started>90)throw new Exception("Playable test timeout");
            if(stage==3&&SceneManager.GetActiveScene().name=="MenuPrincipal"){Check(Time.timeScale==1,"Menu restores time");Finish(0);return;}
            var s=UnityEngine.Object.FindFirstObjectByType<SessionFlow>();if(!s||!s.Player)return;
            if(stage==0){
                Check(s.Room==0&&s.Collected==0,"Fresh reception session");
                Check(UnityEngine.Object.FindObjectsByType<SessionFlow>(FindObjectsSortMode.None).Length==1,"Single session");
                Check(s.World.CanStand(s.Player.Position),"Safe reception spawn");
                Check(!s.World.CanStand(s.World.Cell(0,10,2)),"Internal wall blocks movement");
                var p=s.Player.Position;s.Player.Move(Vector2.right,.1f);Check(s.Player.Position.x>p.x,"Keyboard motor movement path");
                s.Player.transform.position=new Vector2(-15.1f,-8);s.Player.Move(Vector2.left,.1f);Check(s.Player.Position.x>=-15.2f,"No boundary tunnelling");
                Check(s.World.FindRoute(s.World.Entry(0),s.World.Card).Count>0,"Card reachable through office doors");
                Check(s.World.FindRoute(s.World.Card,s.World.Notes[0]).Count>0,"Reception note reachable");
                Check(s.World.FindRoute(s.World.Notes[0],s.World.Door(0)).Count>0,"Reception exit reachable");
                s.Player.transform.position=s.World.Cell(0,15,13);s.SendMessage("LateUpdate");Capture("01-recepcion.png");
                var directions=UnityEngine.Object.FindObjectsByType<TouchDirection>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                Check(directions.Length==4,"Four graphical touch directions wired");
                var touchRoot=directions[0].transform.parent.gameObject;touchRoot.SetActive(true);Capture("06-controles-previa-editor.png");touchRoot.SetActive(false);
                Pickup(s,s.World.Door(0));Check(s.Room==0,"Elevator locked before note");
                s.Player.transform.position=s.World.Notes[0];Time.timeScale=0;s.Interact();Check(s.Collected==0,"Paused collection blocked");Time.timeScale=1;
                s.Interact();s.Interact();Check(s.Collected==1,"Unique note pickup");
                Pickup(s,s.World.Door(0));Check(s.Room==0,"Elevator needs access card");
                Pickup(s,s.World.Card);Check(s.HasCard&&!s.World.CardView.activeSelf,"Card pickup");
                Pickup(s,s.World.Door(0));Check(s.Room==1,"Card and note unlock next zone");
                Check(UnityEngine.Object.FindFirstObjectByType<CampusAtmosphere>().Dark,"Backrooms atmosphere");
                Check(s.World.FindRoute(s.World.Entry(1),s.World.PowerSwitch).Count>0,"Power switch accessible before gate");
                Check(s.World.FindRoute(s.World.Entry(1),s.World.Notes[1]).Count==0,"Gate blocks route before power");
                Check(!s.World.HasLineOfSight(s.World.Cell(1,7,8),s.World.Cell(1,9,8)),"Walls block line of sight");
                Pickup(s,s.World.PowerSwitch);Check(s.HasPower,"Switch restores power");
                Check(s.World.FindRoute(s.World.Entry(1),s.World.Notes[1]).Count>0,"Power opens route to note");
                Check(s.World.FindRoute(s.World.Notes[1],s.World.Door(1)).Count>0,"Hallway exit reachable");
                s.Player.transform.position=s.World.Cell(1,20,8);s.SendMessage("LateUpdate");Capture("02-backfranz.png");
                s.Player.transform.position=s.EnemyPosition;stage=1;
            }else if(stage==1){
                if(!s.Finished)return;Check(!s.Won,"Enemy contact causes defeat");Check(Time.timeScale==0,"Defeat stops time");
                Capture("04-derrota.png");s.Restart();stage=2;
            }else if(stage==2){
                if(s.Finished)return;
                Check(s.Room==0&&s.Collected==0&&!s.HasCard&&!s.HasPower&&Time.timeScale==1,"Retry resets all puzzle state");
                for(int room=0;room<3;room++){
                    Check(s.World.CanStand(s.World.Entry(room)),"Spawn walkable "+room);
                    if(room==0)Pickup(s,s.World.Card);if(room==1)Pickup(s,s.World.PowerSwitch);
                    Check(s.World.FindRoute(s.World.Entry(room),s.World.Notes[room]).Count>0,"Full route to note "+room);
                    Check(s.World.FindRoute(s.World.Notes[room],s.World.Door(room)).Count>0,"Full route to door "+room);
                    Pickup(s,s.World.Notes[room]);Check(s.Collected==room+1,"Collect note "+room);
                    if(room==2){s.Player.transform.position=s.World.Cell(2,16,9);s.SendMessage("LateUpdate");Capture("03-biblioteca.png");}
                    Pickup(s,s.World.Door(room));
                }
                Check(s.Finished&&s.Won&&s.Collected==3,"Victory requires three notes and puzzles");Capture("05-victoria.png");s.Menu();stage=3;
            }
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code){Debug.Log("PLAYABLE RESULT: "+checks+" checks, exit "+code);EditorApplication.update-=Tick;SessionState.SetBool("Backfranz.PlayableSmoke",false);EditorApplication.Exit(code);}
    static void Capture(string file)
    {
        Directory.CreateDirectory("Docs/Hito4/Capturas");var camera=Camera.main;var target=new RenderTexture(1600,900,24);var previous=RenderTexture.active;
        var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder+=3000;}
        camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
        File.WriteAllBytes("Docs/Hito4/Capturas/"+file,texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder-=3000;}
        UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(target);
    }
}
