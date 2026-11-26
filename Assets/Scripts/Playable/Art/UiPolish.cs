using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Backfranz.Playable
{
    [DefaultExecutionOrder(50)]
    public sealed class UiPolish : MonoBehaviour
    {
        static readonly Color Ink=new Color(.045f,.076f,.105f);
        static readonly Color Cream=new Color(.97f,.92f,.81f);
        static readonly Color Amber=new Color(.96f,.65f,.36f);
        RectTransform menuFrame, viewport;
        bool gameplay;
        int viewportShape=-1;
        Image selectionRail;
        readonly Dictionary<GameObject,RectTransform> titleItems=new();
        GameObject selectedTitleItem;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
        static void Loaded(Scene scene,LoadSceneMode mode)
        {var ui=Object.FindFirstObjectByType<BackfranzUI>();if(ui)ui.gameObject.AddComponent<UiPolish>();}
        void Start()
        {
            var ui=GetComponent<BackfranzUI>();
            foreach(var text in GetComponentsInChildren<Text>(true)){
                text.font=ui.status.font;
                if(text.gameObject.name=="Controles"||text.gameObject.name=="Autoría"||text.gameObject.name=="Identidad"||text.gameObject.name=="Nota opciones"||text.gameObject.name=="Confirmación")text.gameObject.SetActive(false);
                text.text=text.text.Replace("PANTALLA COMPLETA (EN LA APLICACIÓN)","PANTALLA COMPLETA");
            }
            foreach(var image in GetComponentsInChildren<Image>(true)){
                if(image.GetComponent<Button>()){image.sprite=CampusLook.Sprite("button");image.type=Image.Type.Sliced;image.color=Color.white;}
                else if(image.name=="Panel"||image.name=="Pausa"||image.name=="Menú principal"){
                    image.sprite=CampusLook.Sprite("panel");image.type=Image.Type.Sliced;image.color=Color.white;
                    if(image.name=="Panel"){
                        var r=image.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=Vector2.zero;
                    }
                }else if(image.name.EndsWith("fondo modal")){
                    var r=image.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
                }
            }
            StyleMenu(ui);
            var button=ui.pauseButton.GetComponent<Image>();button.sprite=CampusLook.Sprite("button");button.type=Image.Type.Sliced;button.color=Color.white;
            foreach(var text in button.GetComponentsInChildren<Text>())text.enabled=false;
            var rect=button.rectTransform;rect.anchorMin=rect.anchorMax=Vector2.one;rect.pivot=Vector2.one;rect.anchoredPosition=new Vector2(-24,-24);rect.sizeDelta=new Vector2(64,64);
            var icon=new GameObject("Pausa",typeof(RectTransform),typeof(Image));icon.transform.SetParent(rect,false);
            var ir=(RectTransform)icon.transform;ir.anchorMin=new Vector2(.15f,.15f);ir.anchorMax=new Vector2(.85f,.85f);ir.offsetMin=ir.offsetMax=Vector2.zero;
            icon.GetComponent<Image>().sprite=CampusLook.Sprite("pause");icon.GetComponent<Image>().raycastTarget=false;
        }

        void StyleMenu(BackfranzUI ui)
        {
            gameplay=ui.gameplay;
            viewport=(RectTransform)transform;
            var menu=(RectTransform)ui.menu.transform;
            var frameImage=ui.menu.GetComponent<UnityEngine.UI.Image>();
            menuFrame=menu;
            if(gameplay)
            {
                var children=new List<Transform>();
                foreach(Transform child in menu)children.Add(child);
                Place(menu,0,0,1,1);
                frameImage.sprite=null;
                frameImage.color=new Color(.018f,.028f,.045f,.70f);
                frameImage.raycastTarget=true;
                menuFrame=Box(menu,"Panel de pausa",Ink).rectTransform;
                foreach(var child in children)child.SetParent(menuFrame,false);
            }
            else
            {
                frameImage.sprite=null;
                frameImage.color=Color.clear;
                frameImage.raycastTarget=false;
                foreach(Transform child in viewport)
                    if(child.TryGetComponent<UnityEngine.UI.RawImage>(out var background))
                    {
                        Place(background.rectTransform,0,0,1,1);
                        if(background.texture)
                        {
                            var cover=child.gameObject.AddComponent<AspectRatioFitter>();
                            cover.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
                            cover.aspectRatio=(float)background.texture.width/background.texture.height;
                        }
                    }
                GetComponent<BackfranzTransition>()?.BuildTitleStage(viewport,menu);
            }
            LayoutFrame();

            var logo=menuFrame.Find("Título Backfranz");
            if(logo)
            {
                logo.gameObject.SetActive(!gameplay);
                Place((RectTransform)logo,.085f,.12f,.94f,.255f);
                if(!gameplay&&logo.TryGetComponent<RawImage>(out var titleArt))
                    titleArt.color=new Color(.80f,.76f,.68f);
            }
            var heading=menuFrame.Find("Estado del campus").GetComponent<UnityEngine.UI.Text>();
            heading.text=gameplay?"PAUSA":"TU PRIMER DÍA EMPIEZA AQUÍ";
            ConfigureText(heading,gameplay?49:19,Amber,TextAnchor.MiddleLeft);
            Place(heading.rectTransform,.09f,gameplay?.09f:.267f,.91f,gameplay?.21f:.302f);
            if(gameplay)
            {
                var summary=Caption(menuFrame,"Zona y tiempo",heading.font,23,Cream);
                Place(summary.rectTransform,.09f,.21f,.91f,.335f);
                ui.SetPauseLocationLabel(summary);
            }
            if(gameplay){
                var band=Box(menuFrame,"Línea del menú",Amber);
                Place(band.rectTransform,.09f,.343f,.91f,.347f);
            }

            var buttons=new List<UnityEngine.UI.Button>();
            foreach(Transform child in menuFrame)
                if(child.TryGetComponent<UnityEngine.UI.Button>(out var found))buttons.Add(found);
            float buttonTop=gameplay?.38f:.367f;
            float slot=gameplay?.095f:.076f;
            for(int i=0;i<buttons.Count;i++)
            {
                var item=buttons[i];
                Place((RectTransform)item.transform,.09f,buttonTop+i*slot,.91f,buttonTop+i*slot+slot*.79f);
                var image=item.GetComponent<UnityEngine.UI.Image>();
                image.sprite=null;
                var colors=item.colors;
                colors.normalColor=new Color(.09f,.153f,.20f);
                colors.highlightedColor=new Color(.20f,.285f,.34f);
                colors.selectedColor=new Color(.28f,.33f,.35f);
                colors.pressedColor=new Color(.14f,.21f,.25f);
                colors.disabledColor=new Color(.065f,.10f,.136f);
                if(gameplay)item.colors=colors;
                else{
                    item.transition=Selectable.Transition.None;
                    image.color=Color.clear;
                }
                var label=item.GetComponentInChildren<UnityEngine.UI.Text>(true);
                ConfigureText(label,26,item.interactable?Cream:new Color(.56f,.64f,.66f),TextAnchor.MiddleLeft);
                Place(label.rectTransform,.058f,0,.94f,1);
                if(gameplay){
                    var marker=Box(item.transform,"Indicador",i==0?Amber:new Color(.24f,.37f,.43f));
                    Place(marker.rectTransform,0,.22f,.009f,.78f);
                }else{
                    titleItems.Add(item.gameObject,(RectTransform)item.transform);
                }

                var navigation=item.navigation;
                navigation.mode=Navigation.Mode.Explicit;
                navigation.selectOnUp=Adjacent(buttons,i,-1);
                navigation.selectOnDown=Adjacent(buttons,i,1);
                navigation.selectOnLeft=null;
                navigation.selectOnRight=null;
                item.navigation=navigation;
            }
            if(!gameplay){
                selectionRail=Box(menuFrame,"Selección activa",Amber);
                selectionRail.rectTransform.anchorMin=selectionRail.rectTransform.anchorMax=new Vector2(0,.5f);
                selectionRail.rectTransform.sizeDelta=new Vector2(15,3);
                selectionRail.enabled=false;
            }
            ui.status.transform.SetParent(menuFrame,false);
            ConfigureText(ui.status,20,Amber,TextAnchor.MiddleLeft);
            Place(ui.status.rectTransform,.09f,.887f,.91f,.968f);
        }

        void Update()
        {
            if(menuFrame)LayoutFrame();
            if(!selectionRail||!menuFrame.gameObject.activeInHierarchy)return;
            var current=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            if(!current||!titleItems.TryGetValue(current,out var button)||!current.GetComponent<Button>().interactable){
                selectionRail.enabled=false;
                return;
            }
            Vector2 goal=menuFrame.InverseTransformPoint(button.TransformPoint(button.rect.center));
            goal.x=menuFrame.InverseTransformPoint(button.TransformPoint(new Vector2(button.rect.xMin,0))).x-menuFrame.rect.xMin+4;
            var rail=selectionRail.rectTransform;
            goal.y-=menuFrame.rect.center.y;
            rail.anchoredPosition=selectionRail.enabled?Vector2.Lerp(rail.anchoredPosition,goal,1-Mathf.Exp(-22*Time.unscaledDeltaTime)):goal;
            selectionRail.enabled=true;
            if(selectedTitleItem&&selectedTitleItem!=current)GetComponent<BackfranzUI>().sound.Click();
            selectedTitleItem=current;
        }

        void LayoutFrame()
        {
            float aspect=viewport.rect.width/Mathf.Max(1,viewport.rect.height);
            int shape=aspect<1.08f?0:aspect<1.48f?1:2;
            if(shape==viewportShape)return;
            viewportShape=shape;
            float right=shape==0?.94f:shape==1?.53f:.374f;
            if(gameplay)
                Place(menuFrame,.048f,.075f,shape==0?.952f:right+.048f,.925f);
            else Place(menuFrame,.034f,.036f,shape==0?.88f:shape==1?.52f:.369f,.964f);
        }

        static UnityEngine.UI.Button Adjacent(List<UnityEngine.UI.Button> buttons,int from,int direction)
        {
            for(int offset=1;offset<buttons.Count;offset++)
            {
                int index=(from+direction*offset+buttons.Count)%buttons.Count;
                if(buttons[index].interactable)return buttons[index];
            }
            return null;
        }

        static void Place(RectTransform rect,float left,float top,float right,float bottom)
        {
            rect.anchorMin=new Vector2(left,1-bottom);
            rect.anchorMax=new Vector2(right,1-top);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }

        static UnityEngine.UI.Image Box(Transform parent,string name,Color color)
        {
            var box=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));
            box.transform.SetParent(parent,false);
            var image=box.GetComponent<UnityEngine.UI.Image>();
            image.raycastTarget=false;
            image.color=color;
            return image;
        }

        static UnityEngine.UI.Text Caption(Transform parent,string name,Font font,int size,Color color)
        {
            var caption=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Text));
            caption.transform.SetParent(parent,false);
            var text=caption.GetComponent<UnityEngine.UI.Text>();
            text.font=font;
            ConfigureText(text,size,color,TextAnchor.MiddleLeft);
            return text;
        }

        static void ConfigureText(UnityEngine.UI.Text text,int size,Color color,TextAnchor alignment)
        {
            text.color=color;
            text.alignment=alignment;
            text.fontSize=size;
            text.verticalOverflow=VerticalWrapMode.Truncate;
            text.raycastTarget=false;
            text.resizeTextForBestFit=true;
            text.resizeTextMinSize=13;
            text.resizeTextMaxSize=size;
        }
    }
}
