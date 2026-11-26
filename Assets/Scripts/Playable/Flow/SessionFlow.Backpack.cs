using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (string Name,string Sprite,Func<SessionFlow,string> Describe,Func<SessionFlow,bool> Owned)[] BackpackItems =
        {
            ("Credencial de estudiante","card",
                _=>"Me la dieron en Recepción. Con la credencial puedo registrar mi huella en el control o pedirle al guardia que abra el paso ancho.",
                session=>session.HasCard),
            ("Horario de inducción","campus-item-horario",
                _=>"Aquí figuran los seis pisos de la inducción. El panel del ascensor me indica en cuáles me falta una constancia.",
                session=>session.HasSchedule),
            ("Constancia de inducción","bloque-a-item-guia-biblioteca",
                _=>"El mostrador de la biblioteca acreditó mi actividad del sexto piso.",
                session=>session.HasVisited(6)),
            ("Fusible","bloque-a-item-fusible",
                session=>session.World.Power
                    ?"Ya lo instalé en el cuadro del ascensor del subsuelo. La corriente de emergencia está activa."
                    :"Es del mismo tipo que el del cuadro del ascensor del subsuelo. Puedo colocarlo en el cuadro para devolverle la corriente.",
                session=>session.HasFuse),
            ("Llave técnica","bloque-a-item-llave-tecnica",
                _=>"La encontré en el piso 1. Junto a la llave estaba anotado el orden del circuito del piso 3.",
                session=>session.HasKey),
            ("Pase de salida","bloque-a-item-credencial-salida",
                _=>"Lo recuperé de la vitrina del piso 5. Sirve para el acceso de planta baja.",
                session=>session.HasBadge),
            (RecorderName,RecorderSprite,
                _=>"La encontré junto al estudio de Sonido. Puede reproducir una voz lejos de mí, si necesito que algo vaya a investigar.",
                session=>session.HasAuditoriumTool(AuditoriumTool.Recorder)),
            (WedgeName,WedgeSprite,
                session=>session.auditoriumWedged
                    ?"La dejé bajo la puerta de servicio del Auditorio. Mantiene abierto el paso hacia el vestíbulo oeste."
                    :"Es una cuña del Taller. Puedo deslizarla bajo una puerta de servicio para mantenerla abierta.",
                session=>session.HasAuditoriumTool(AuditoriumTool.Wedge)),
            (ServicesPassName,ServicesPassSprite,
                _=>"La encontré en Arca. En el reverso pone «Mónica · Dirección de Carrera». Debería devolvérsela.",
                session=>session.servicesPassFound&&!session.servicesPassReturned),
            ("Botiquín portátil","utileria-maletin-clinico",
                _=>"Lo saqué del armario de Simulación. Nuria se lastimó el pie y está esperando junto a las camillas.",
                session=>HasEscapeFlag(session.rescueItems,NuriaAid)&&!HasEscapeFlag(session.escortSeen,NuriaEscort)),
            (RadioItemName,RadioItemSprite,
                _=>"La encontré en Streaming. Puedo escuchar el canal de emergencia desde la mochila.",
                session=>session.hasCampusRadio),
            (ChalkName,ChalkSprite,
                _=>"La encontré en Aula 4A. Puedo dejar una cruz bajo el rótulo 4B y ver si el pasillo me devuelve a ese punto.",
                session=>session.hasClassroomChalk),
            (WorkshopCableName,WorkshopCableSprite,
                session=>session.workshopLamp>0?"Lo dejé conectado en "+session.WorkshopLampLocation()+".":
                    "Puedo conectar una lámpara del pasillo. Si retiro el cable para moverlo, esa lámpara se apagará.",
                session=>session.hasWorkshopCable)
        };

        GameObject backpackPanel;
        UnityEngine.UI.Button backpackClose;
        UnityEngine.UI.Button backpackListen;
        UnityEngine.UI.Text backpackName, backpackDescription;
        UnityEngine.UI.Image backpackPreview;
        readonly UnityEngine.UI.Button[] backpackRows=new UnityEngine.UI.Button[BackpackItems.Length];
        int selectedBackpackItem=-1;
        bool BackpackOpen=>backpackPanel&&backpackPanel.activeSelf;
        bool CanOpenBackpack=>!BackpackOpen&&!Finished&&!Crossing&&!DialogueOpen&&Time.timeScale>0;

        void CreateBackpack(Transform safe)
        {
            var caption=Application.isMobilePlatform?"Mochila":"Mochila [I]";
            var opener=Button(hudControls,caption,new Vector2(.58f,0f),new Vector2(.82f,1f),OpenBackpack,20);
            opener.name="Abrir mochila";

            var backdrop=Rect(safe,"Mochila de Yamil",Vector2.zero,Vector2.one);
            backdrop.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.021f,.037f,.052f,.79f);
            backpackPanel=backdrop.gameObject;
            var frame=Panel(backdrop,"Mochila",new Vector2(.12f,.13f),new Vector2(.88f,.87f));
            var heading=Label(frame,"Título",new Vector2(.055f,.83f),new Vector2(.75f,.96f),34);
            heading.text="Mochila de Yamil";
            heading.alignment=TextAnchor.MiddleLeft;
            heading.color=new Color(1f,.81f,.55f);

            for(int index=0;index<BackpackItems.Length;index++)
            {
                int itemIndex=index;
                var item=BackpackItems[index];
                var row=Button(frame,"",Vector2.zero,Vector2.one,()=>SelectBackpackItem(itemIndex));
                row.name=item.Name;
                Icon(row.transform,item.Sprite,new Vector2(.05f,.17f),new Vector2(.20f,.83f));
                var title=Label(row.transform,"Nombre",new Vector2(.23f,.07f),new Vector2(.965f,.93f),23);
                title.text=item.Name;
                title.alignment=TextAnchor.MiddleLeft;
                backpackRows[index]=row;
            }

            var details=Panel(frame,"Ficha del objeto",new Vector2(.51f,.16f),new Vector2(.945f,.81f));
            details.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            backpackPreview=Icon(details,BackpackItems[0].Sprite,new Vector2(.25f,.52f),new Vector2(.75f,.92f));
            backpackPreview.name="Sprite del objeto";
            backpackName=Label(details,"Nombre del objeto",new Vector2(.08f,.36f),new Vector2(.92f,.52f),28);
            backpackDescription=Label(details,"Descripción del objeto",new Vector2(.08f,.055f),new Vector2(.92f,.34f),23);
            backpackDescription.alignment=TextAnchor.UpperLeft;
            backpackListen=Button(details,"Escuchar",new Vector2(.25f,.035f),new Vector2(.75f,.15f),ListenCampusRadio,22);
            backpackClose=Button(frame,"Cerrar",new Vector2(.705f,.04f),new Vector2(.945f,.135f),CloseBackpack,23);
            backpackPanel.SetActive(false);
        }

        void OpenBackpack()
        {
            if(!CanOpenBackpack)return;
            Player.TouchAxis(Vector2.zero);
            pause.enabled=false;
            pause.pauseButton.SetActive(false);
            Time.timeScale=0;
            backpackPanel.SetActive(true);
            RefreshBackpack();
            Sound.Click();
        }

        void RefreshBackpack()
        {
            int first=-1, ownedCount=0;
            for(int index=0;index<BackpackItems.Length;index++)
            {
                bool owned=BackpackItems[index].Owned(this);
                backpackRows[index].gameObject.SetActive(owned);
                if(!owned)continue;
                if(first<0)first=index;
                ownedCount++;
            }
            float spacing=Mathf.Min(.105f,.66f/Mathf.Max(1,ownedCount));
            float top=.80f;
            foreach(var row in backpackRows)
            {
                if(!row.gameObject.activeSelf)continue;
                var bounds=(RectTransform)row.transform;
                bounds.anchorMin=new Vector2(.055f,top-spacing+.015f);
                bounds.anchorMax=new Vector2(.48f,top);
                top-=spacing;
            }
            int selection=selectedBackpackItem>=0&&BackpackItems[selectedBackpackItem].Owned(this)?selectedBackpackItem:first;
            SelectBackpackItem(selection);
            EventSystem.current.SetSelectedGameObject(selection>=0?backpackRows[selection].gameObject:backpackClose.gameObject);
        }

        void SelectBackpackItem(int index)
        {
            selectedBackpackItem=index;
            for(int row=0;row<backpackRows.Length;row++)
                backpackRows[row].targetGraphic.color=row==index?new Color(1f,.84f,.59f):Color.white;
            backpackPreview.enabled=index>=0;
            bool radio=index>=0&&BackpackItems[index].Name==RadioItemName;
            backpackListen.gameObject.SetActive(radio);
            backpackDescription.rectTransform.anchorMin=new Vector2(.08f,radio?.17f:.055f);
            if(index<0)
            {
                backpackName.text="Mochila vacía";
                backpackDescription.text="Aquí podré revisar los objetos que encuentre.";
                return;
            }
            var item=BackpackItems[index];
            backpackPreview.sprite=CampusLook.Sprite(item.Sprite);
            backpackName.text=item.Name;
            backpackDescription.text=item.Describe(this);
        }

        IEnumerator CloseBackpackAfterEscape()
        {
            // Let the pause menu see this key press while it is still disabled.
            yield return null;
            CloseBackpack();
        }

        void CloseBackpack()
        {
            if(!BackpackOpen)return;
            backpackPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            Sound.Back();
            Time.timeScale=1;
            pause.enabled=true;
            pause.pauseButton.SetActive(true);
        }
    }
}
