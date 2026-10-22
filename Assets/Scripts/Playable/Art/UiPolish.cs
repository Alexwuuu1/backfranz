using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Backfranz.Playable
{
    public sealed class UiPolish : MonoBehaviour
    {
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
            var button=ui.pauseButton.GetComponent<Image>();button.sprite=CampusLook.Sprite("button");button.type=Image.Type.Sliced;button.color=Color.white;
            foreach(var text in button.GetComponentsInChildren<Text>())text.enabled=false;
            var rect=button.rectTransform;rect.anchorMin=rect.anchorMax=Vector2.one;rect.pivot=Vector2.one;rect.anchoredPosition=new Vector2(-24,-24);rect.sizeDelta=new Vector2(64,64);
            var icon=new GameObject("Pausa",typeof(RectTransform),typeof(Image));icon.transform.SetParent(rect,false);
            var ir=(RectTransform)icon.transform;ir.anchorMin=new Vector2(.15f,.15f);ir.anchorMax=new Vector2(.85f,.85f);ir.offsetMin=ir.offsetMax=Vector2.zero;
            icon.GetComponent<Image>().sprite=CampusLook.Sprite("pause");icon.GetComponent<Image>().raycastTarget=false;
        }
    }
}
