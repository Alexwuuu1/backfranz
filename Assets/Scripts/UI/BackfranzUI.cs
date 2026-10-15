using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Alejandro: navigation and modal focus. Each scene owns its UI (no duplicates).
public sealed class BackfranzUI : MonoBehaviour
{
    public bool gameplay;
    public GameObject menu, options, extras, credits, confirmation, pauseButton;
    public Button firstButton;
    public Slider volume;
    public Toggle fullscreen, reducedFlashes;
    public BackfranzAudio sound;
    public Text status;
    private GameObject modal;
    private bool paused, loading;
    void Awake() { Time.timeScale=1; AudioListener.volume=PlayerPrefs.GetFloat("Backfranz.Volume",.7f); }
    void Start()
    {
        volume.SetValueWithoutNotify(AudioListener.volume);
        fullscreen.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Backfranz.Fullscreen",Screen.fullScreen?1:0)==1);
        reducedFlashes.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Backfranz.ReducedFlashes",0)==1);
        if(!Application.isEditor) Screen.fullScreen=fullscreen.isOn;
        menu.SetActive(!gameplay); if(pauseButton) pauseButton.SetActive(gameplay);
        if(!gameplay) Focus(firstButton.gameObject);
    }
    void Update()
    {
        if(loading || !Input.GetKeyDown(KeyCode.Escape)) return;
        if(modal) { ClosePanel(); return; }
        if(gameplay) { if(paused) Resume(); else Pause(); }
    }
    void Focus(GameObject target) { EventSystem.current.SetSelectedGameObject(null); EventSystem.current.SetSelectedGameObject(target); }
    void Panel(GameObject panel)
    {
        sound.Click(); if(modal) modal.transform.parent.gameObject.SetActive(false); menu.SetActive(false);
        modal=panel;modal.transform.parent.gameObject.SetActive(true);
        var selectable=panel.GetComponentInChildren<Selectable>(); if(selectable) Focus(selectable.gameObject);
    }
    public void Options() { Panel(options); }
    public void Extras() { Panel(extras); }
    public void Credits() { Panel(credits); }
    public void AskQuit() { Panel(confirmation); }
    public void ClosePanel() { sound.Click();if(modal) modal.transform.parent.gameObject.SetActive(false);modal=null;menu.SetActive(true);Focus(firstButton.gameObject); }
    public void Play() { Load("Nivel01"); }
    public void MainMenu() { Load("MenuPrincipal"); }
    void Load(string scene)
    {
        if(loading) return;
        if(!Application.CanStreamedLevelBeLoaded(scene)) { status.text="Escena no incluida: "+scene;return; }
        loading=true;sound.Click();Time.timeScale=1;SceneManager.LoadSceneAsync(scene);
    }
    public void Pause() { if(loading)return; paused=true;Time.timeScale=0;menu.SetActive(true);pauseButton.SetActive(false);sound.Click();Focus(firstButton.gameObject); }
    public void Resume() { paused=false;Time.timeScale=1;menu.SetActive(false);pauseButton.SetActive(true);EventSystem.current.SetSelectedGameObject(null);sound.Click(); }
    public void Volume(float value) { AudioListener.volume=value; PlayerPrefs.SetFloat("Backfranz.Volume",value); }
    public void Fullscreen(bool value) { if(!Application.isEditor)Screen.fullScreen=value; PlayerPrefs.SetInt("Backfranz.Fullscreen",value?1:0); }
    public void ReducedFlashes(bool value) { PlayerPrefs.SetInt("Backfranz.ReducedFlashes",value?1:0); }
    public void Quit()
    {
        PlayerPrefs.Save();Time.timeScale=1;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
        #else
        Application.Quit();
        #endif
    }
    void OnDestroy() { Time.timeScale=1;PlayerPrefs.Save(); }
}
