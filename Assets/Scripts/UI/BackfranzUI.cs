using Backfranz.Playable;
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
    private GameObject returnPanel;
    private Text pauseLocation;
    private GameObject modalTrigger;
    void Awake() { Time.timeScale=1; AudioListener.volume=PlayerPrefs.GetFloat("Backfranz.Volume",.7f); }
    void Start()
    {
        volume.SetValueWithoutNotify(AudioListener.volume);
        fullscreen.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Backfranz.Fullscreen",Screen.fullScreen?1:0)==1);
        reducedFlashes.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Backfranz.ReducedFlashes",0)==1);
        if(!Application.isEditor) Screen.fullScreen=fullscreen.isOn;
        menu.SetActive(!gameplay); if(pauseButton) pauseButton.SetActive(gameplay);
        AddSaveControls();
        if(gameplay) CreateReturnPanel();
        else
        {
            sound.SetTitleMusic();
            Focus(firstButton.gameObject);
        }
    }
    void AddSaveControls()
    {
        var original = (RectTransform)firstButton.transform;
        var label = firstButton.GetComponentInChildren<Text>();
        if(!gameplay) label.text="Nueva partida";
        var extra = Instantiate(firstButton, original.parent);
        extra.name=gameplay?"Guardar partida":"Continuar partida";
        extra.GetComponentInChildren<Text>().text=extra.name;
        extra.onClick=new Button.ButtonClickedEvent();
        extra.onClick.AddListener(gameplay ? (UnityEngine.Events.UnityAction)Save : Continue);
        float spacing=original.rect.height+12;
        foreach(Transform sibling in original.parent){
            if(sibling==original||sibling==extra.transform||!sibling.TryGetComponent<Button>(out _))continue;
            var rect=(RectTransform)sibling;
            rect.anchoredPosition+=Vector2.down*spacing;
        }
        ((RectTransform)extra.transform).anchoredPosition=original.anchoredPosition+Vector2.down*spacing;
        extra.transform.SetSiblingIndex(original.GetSiblingIndex()+1);
        extra.interactable=gameplay||SessionFlow.CanContinue;
    }
    void Save()
    {
        var session=FindFirstObjectByType<SessionFlow>();
        Save(session);
    }
    bool Save(SessionFlow session)
    {
        if(session&&session.SaveGame())
        {
            status.text="Partida guardada";
            sound.SaveConfirmation();
            return true;
        }
        status.text="No se puede guardar aquí";
        sound.Back();
        return false;
    }
    public void Continue()
    {
        if(loading)return;
        if(SessionFlow.RequestContinue())Load("Nivel01");
        else status.text="No hay una partida guardada válida";
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
        if(loading)return;
        if(!modal) modalTrigger=EventSystem.current.currentSelectedGameObject;
        sound.Click(); if(modal) modal.transform.parent.gameObject.SetActive(false); menu.SetActive(false);
        modal=panel;modal.transform.parent.gameObject.SetActive(true);
        var selectable=panel.GetComponentInChildren<Selectable>(); if(selectable) Focus(selectable.gameObject);
    }
    public void Options() { Panel(options); }
    public void Extras()
    {
        if(!extras.TryGetComponent<EscapeExtras>(out var archive))
        {
            archive=extras.AddComponent<EscapeExtras>();
            archive.Build(this);
        }
        Panel(extras);
        archive.Open();
    }
    public void Credits() { Panel(credits); }
    public void AskQuit() { Panel(confirmation); }
    public void ClosePanel()
    {
        sound.Back();
        if(modal) modal.transform.parent.gameObject.SetActive(false);
        modal=null;
        menu.SetActive(true);
        Focus(modalTrigger&&modalTrigger.activeInHierarchy?modalTrigger:firstButton.gameObject);
        modalTrigger=null;
    }
    public void Play()
    {
        if(loading)return;
        SessionFlow.RequestNewGame();
        Load("Nivel01");
    }
    public void MainMenu()
    {
        if(!gameplay){Load("MenuPrincipal");return;}
        if(paused&&!loading) Panel(returnPanel);
    }

    void CreateReturnPanel()
    {
        var template=confirmation.transform.parent;
        var backdrop=Instantiate(template.gameObject,template.parent);
        backdrop.name="Volver al menú · fondo modal";
        returnPanel=backdrop.transform.GetChild(0).gameObject;
        foreach(var title in returnPanel.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            if(title.name=="Título") title.text="VOLVER AL MENÚ";
            if(title.name=="Confirmación")
            {
                title.name="Aviso de progreso";
                title.text="Guarda la partida antes de volver al menú si quieres conservar el progreso desde tu último punto.";
            }
        }
        var leave=returnPanel.transform.Find("Sí, salir").GetComponent<UnityEngine.UI.Button>();
        leave.name="Salir sin guardar";
        leave.GetComponentInChildren<UnityEngine.UI.Text>().text="Salir sin guardar";
        leave.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();
        leave.onClick.AddListener(()=>Load("MenuPrincipal"));
        var save=Instantiate(leave,returnPanel.transform);
        save.name="Guardar y salir";
        save.GetComponentInChildren<UnityEngine.UI.Text>().text=save.name;
        save.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();
        save.onClick.AddListener(()=>{
            if(Save(FindFirstObjectByType<SessionFlow>()))Load("MenuPrincipal");
            else ClosePanel();
        });
        var cancel=returnPanel.transform.Find("Cancelar");
        PlaceReturnAction(save.transform, .075f, .63f, .925f, .74f);
        PlaceReturnAction(leave.transform, .075f, .78f, .49f, .89f);
        PlaceReturnAction(cancel, .51f, .78f, .925f, .89f);
        cancel.SetAsFirstSibling();
        backdrop.SetActive(false);
    }

    static void PlaceReturnAction(Transform target,float left,float top,float right,float bottom)
    {
        var area=(RectTransform)target;
        area.anchorMin=new Vector2(left,1-bottom);
        area.anchorMax=new Vector2(right,1-top);
        area.offsetMin=area.offsetMax=Vector2.zero;
        var label=target.GetComponentInChildren<UnityEngine.UI.Text>(true);
        var textArea=label.rectTransform;
        textArea.anchorMin=new Vector2(.035f,0);
        textArea.anchorMax=new Vector2(.965f,1);
        textArea.offsetMin=textArea.offsetMax=Vector2.zero;
        label.alignment=TextAnchor.MiddleCenter;
        label.resizeTextForBestFit=true;
        label.resizeTextMaxSize=24;
    }
    void Load(string scene)
    {
        if(loading) return;
        if(!Application.CanStreamedLevelBeLoaded(scene)) { status.text="Escena no incluida: "+scene;return; }
        loading=true;
        Time.timeScale=1;
        if(!gameplay&&scene=="Nivel01"&&TryGetComponent<BackfranzTransition>(out var title)){
            StartCoroutine(LoadFromTitle(scene,title));
            return;
        }
        sound.Click();
        SceneManager.LoadSceneAsync(scene);
    }

    System.Collections.IEnumerator LoadFromTitle(string scene,BackfranzTransition title)
    {
        yield return title.PlayDeparture();
        SceneManager.LoadSceneAsync(scene);
    }
    internal void SetPauseLocationLabel(Text label)=>pauseLocation=label;
    public void Pause()
    {
        if(loading||paused||!isActiveAndEnabled||Time.timeScale==0)return;
        paused=true;
        Time.timeScale=0;
        status.text="";
        if(pauseLocation&&FindFirstObjectByType<SessionFlow>() is SessionFlow session)
            pauseLocation.text=session.PauseLocation;
        menu.SetActive(true);
        pauseButton.SetActive(false);
        sound.Click();
        Focus(firstButton.gameObject);
    }
    public void Resume() { paused=false;Time.timeScale=1;status.text="";menu.SetActive(false);pauseButton.SetActive(true);EventSystem.current.SetSelectedGameObject(null);sound.Back(); }
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
