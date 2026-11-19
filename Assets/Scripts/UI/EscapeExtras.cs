using Backfranz.Playable;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class EscapeExtras : MonoBehaviour
{
    const int PageSize=6;
    Font font;
    int page, selectedPerson;
    RectTransform content;
    Texture[] concepts;
    Button endingsTab;

    public void Build(BackfranzUI menu)
    {
        font=menu.status.font;
        var previous=GetComponentsInChildren<RawImage>(true);
        concepts=System.Array.ConvertAll(previous,image=>image.texture);
        foreach(Transform child in transform)child.gameObject.SetActive(false);

        Label(transform,"Extras","EXTRAS",.08f,.87f,.92f,.98f,32);
        endingsTab=Button(transform,"Finales",.06f,.76f,.33f,.86f,ShowEndings);
        Button(transform,"Personajes",.36f,.76f,.64f,.86f,ShowPeople);
        Button(transform,"Arte",.67f,.76f,.94f,.86f,ShowArt);
        Button(transform,"Volver",.32f,.03f,.68f,.115f,menu.ClosePanel);
        content=Rect(transform,"Contenido de Extras",.05f,.14f,.95f,.735f);
        ShowEndings();
    }

    public void Open()
    {
        ShowEndings();
        if(EventSystem.current)EventSystem.current.SetSelectedGameObject(endingsTab.gameObject);
    }

    void Clear()
    {
        foreach(Transform child in content)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    void ShowEndings()
    {
        Clear();
        for(int index=0;index<3;index++)
        {
            var ending=(EscapeArchive.Ending)(index+1);
            bool unlocked=EscapeArchive.Unlocked(ending);
            float left=.015f+.326f*index;
            var card=Rect(content,"Desenlace "+(index+1),left,.04f,left+.318f,.96f);
            var cover=Rect(card,"Ilustración",.045f,.49f,.955f,.98f).gameObject.AddComponent<Image>();
            cover.sprite=CampusLook.Sprite(unlocked?EscapeArchive.Cover(ending):"final-bloqueado");
            cover.preserveAspect=true;
            Label(card,"Título",unlocked?EscapeArchive.Title(ending):"Desenlace bloqueado",.03f,.37f,.97f,.50f,21);
            Label(card,"Historia",unlocked?EscapeArchive.Story(ending):"Completa una de las rutas del portón para descubrir su historia.",
                .04f,.015f,.96f,.355f,17);
        }
    }

    void ShowPeople()
    {
        Clear();
        int start=page*PageSize;
        for(int index=0;index<PageSize&&start+index<EscapeArchive.People.Length;index++)
        {
            int person=start+index;
            float y=.82f-index*.121f;
            Button(content,EscapeArchive.People[person].Name,.015f,y,.39f,y+.105f,()=>{
                selectedPerson=person;
                ShowPeople();
                FocusPerson();
            });
        }
        var entry=EscapeArchive.People[selectedPerson];
        var portrait=Rect(content,"Retrato de "+entry.Name,.51f,.52f,.88f,.98f).gameObject.AddComponent<Image>();
        portrait.sprite=CampusLook.Sprite(CampusLook.Person(entry.Name,entry.Art));
        portrait.preserveAspect=true;
        Label(content,"Nombre",entry.Name,.42f,.40f,.96f,.52f,26);
        Label(content,"Historia",entry.Bio,.44f,.08f,.95f,.39f,20);

        int pages=(EscapeArchive.People.Length+PageSize-1)/PageSize;
        Button(content,"Atrás",.015f,.035f,.12f,.14f,()=>TurnPage(-1)).interactable=page>0;
        Label(content,"Página",(page+1)+" / "+pages,.12f,.035f,.28f,.14f,18);
        Button(content,"Más",.29f,.035f,.39f,.14f,()=>TurnPage(1)).interactable=page+1<pages;
    }

    void TurnPage(int direction)
    {
        page+=direction;
        selectedPerson=page*PageSize;
        ShowPeople();
        FocusPerson();
    }

    void FocusPerson()
    {
        foreach(Transform child in content)
            if(child.gameObject.activeSelf&&child.name==EscapeArchive.People[selectedPerson].Name)
            {
                EventSystem.current.SetSelectedGameObject(child.gameObject);
                break;
            }
    }

    void ShowArt()
    {
        Clear();
        for(int index=0;index<concepts.Length;index++)
        {
            float left=.025f+.49f*index;
            var art=Rect(content,"Arte del campus",left,.22f,left+.47f,.91f).gameObject.AddComponent<RawImage>();
            art.texture=concepts[index];
        }
        Label(content,"Descripción","Arte del campus",.1f,.03f,.9f,.18f,23);
    }

    static RectTransform Rect(Transform parent,string name,float left,float bottom,float right,float top)
    {
        var child=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        child.SetParent(parent,false);
        child.anchorMin=new Vector2(left,bottom);
        child.anchorMax=new Vector2(right,top);
        child.offsetMin=child.offsetMax=Vector2.zero;
        return child;
    }

    Text Label(Transform parent,string name,string value,float left,float bottom,float right,float top,int size)
    {
        var text=Rect(parent,name,left,bottom,right,top).gameObject.AddComponent<Text>();
        text.font=font;
        text.text=value;
        text.fontSize=size;
        text.resizeTextForBestFit=true;
        text.resizeTextMinSize=12;
        text.resizeTextMaxSize=size;
        text.alignment=TextAnchor.MiddleCenter;
        text.color=new Color(.96f,.91f,.76f);
        text.raycastTarget=false;
        return text;
    }

    Button Button(Transform parent,string value,float left,float bottom,float right,float top,UnityEngine.Events.UnityAction action)
    {
        var rect=Rect(parent,value,left,bottom,right,top);
        var image=rect.gameObject.AddComponent<Image>();
        image.sprite=CampusLook.Sprite("button");
        image.type=Image.Type.Sliced;
        var button=rect.gameObject.AddComponent<Button>();
        button.targetGraphic=image;
        button.onClick.AddListener(action);
        Label(rect,"Texto",value,.05f,.08f,.95f,.92f,23);
        return button;
    }
}
