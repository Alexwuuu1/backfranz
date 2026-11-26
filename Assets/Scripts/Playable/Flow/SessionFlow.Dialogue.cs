using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        readonly struct DialogueLine
        {
            public readonly string Speaker, Sprite, Text;

            public DialogueLine(string speaker, string sprite, string text)
            {
                Speaker = speaker;
                Sprite = sprite;
                Text = text;
            }
        }

        static readonly DialogueLine[] ArrivalDialogue =
        {
            new DialogueLine("Yamil", "yamil-00", "Soy Yamil. Hoy empiezo mi primer semestre en Unifranz. Vine a conocer el Bloque A antes de que comiencen las clases."),
            new DialogueLine("Yamil", "yamil-00", "En Recepción me esperan con mi credencial y mi horario. Después visitaré las áreas del campus y completaré seis actividades de inducción."),
            new DialogueLine("Yamil", "yamil-00", "Dicen que algunos estudiantes han encontrado pisos que no aparecen en el directorio. Espero que solo sea una historia para los nuevos."),
            new DialogueLine("Yamil", "yamil-00", "El Bloque A queda frente al Instituto Americano. Buscaré las gradas junto a las tiendas. Puedo preguntar al estudiante que está en la calle.")
        };
        GameObject arrivalTitle;

        static readonly DialogueLine[] StudentDialogue =
        {
            new DialogueLine("Estudiante", "pb-estudiante", "¿También vienes a la inducción? Sube las gradas al lado de las tiendas. En el rellano, gira a la derecha."),
            new DialogueLine("Yamil", "yamil-00", "Sí. ¿Sabes dónde entregan el horario?"),
            new DialogueLine("Estudiante", "pb-estudiante", "En Recepción. Cuando entres, busca la ventanilla junto al vidrio azul.")
        };

        static readonly DialogueLine[] ReceptionDialogue =
        {
            new DialogueLine("Recepcionista", "pb-recepcionista", "Buen día. ¿Vienes por el horario de primer semestre?"),
            new DialogueLine("Yamil", "yamil-00", "Sí, soy Yamil. Es mi primer día."),
            new DialogueLine("Recepcionista", "pb-recepcionista", "Aquí tienes tu credencial y tu horario. Pasa por el lector de huella. Hay seis actividades de inducción y el ascensor muestra en qué pisos te faltan constancias.")
        };
        static readonly DialogueLine[] ReceptionRegistered =
        {
            new DialogueLine("Recepcionista", "pb-recepcionista", "Ya tienes tu credencial. Puedes usar el ascensor o las gradas después del control.")
        };
        static readonly DialogueLine[] GuardAwaitingRegistration =
        {
            new DialogueLine("Guardia", "pb-guardia", "Pasa primero por la ventanilla de Recepción para que te habiliten el ingreso.")
        };
        static readonly DialogueLine[] GuardAccessibleAccess =
        {
            new DialogueLine("Guardia", "pb-guardia", "Tu ingreso está habilitado. Si necesitas el paso ancho, te abro la puerta de vidrio.")
        };

        GameObject dialoguePanel, dialogueContinue, dialogueChoices;
        Button acceptChoice;
        Text dialogueSpeaker, dialogueBody, dialogueAction;
        Image dialoguePortrait;
        DialogueLine[] dialogue;
        System.Action dialogueCompleted;
        bool requiresChoice;
        int dialogueIndex;
        float dialogueReveal;
        int visibleCharacters;

        public bool DialogueOpen => dialogue != null;

        void RegisterAtReception()
        {
            if (HasCard) return;
            HasCard = true;
            HasSchedule = true;
            visits[0] = true;
            Visited++;
            Confirm("Credencial y horario recibidos");
        }

        void BeginSecurityAccess(int lane, bool fromGuard)
        {
            if (World.SecurityOpen(lane)) return;
            if (lane == AccessibleLane && !fromGuard)
            {
                Notify("El guardia abre la puerta accesible");
                return;
            }
            StartCoroutine(ScanSecurity(lane, HasCard, fromGuard));
        }

        IEnumerator ScanSecurity(int lane, bool authorized, bool fromGuard)
        {
            Crossing = true;
            Player.TouchAxis(Vector2.zero);
            pause.enabled = false;
            pause.pauseButton.SetActive(false);
            Notify(fromGuard ? "El guardia abre la puerta" : "Escaneando huella…");
            if(fromGuard)Sound.Door();
            else Sound.Scanner();
            for (int pulse = 0; pulse < 4; pulse++)
            {
                World.ShowSecuritySignal(lane, pulse % 2 == 0 ? new Color(1f,.78f,.44f) : Color.white);
                yield return new WaitForSecondsRealtime(.13f);
            }
            World.ShowSecuritySignal(lane, authorized ? new Color(.65f,1f,.75f) : new Color(1f,.48f,.44f));
            if(!fromGuard)Sound.ScanResult(authorized);
            if (authorized)
            {
                for (float elapsed = 0; elapsed < .32f; elapsed += Time.unscaledDeltaTime)
                {
                    World.ShowSecurityOpening(lane, elapsed/.32f);
                    yield return null;
                }
                World.ShowSecurityOpening(lane, 1f);
                Notify("Acceso autorizado");
            }
            else Notify("Acceso denegado · habla en Recepción");
            Crossing = false;
            pause.enabled = true;
            pause.pauseButton.SetActive(true);
        }

        bool TryTalk()
        {
            if (Room == Entrance && Near(World.StudentContact, 1.2f))
                OpenDialogue(StudentDialogue);
            else if (Room == Ground && Near(World.ReceptionContact, 1.1f))
                OpenDialogue(HasCard?ReceptionRegistered:ReceptionDialogue, RegisterAtReception);
            else if (Room == Ground && Near(World.GuardContact, .8f))
            {
                if (HasCard&&!World.SecurityOpen(AccessibleLane))
                    OpenDialogue(GuardAccessibleAccess, () => BeginSecurityAccess(AccessibleLane, true), true);
                else if(HasCard)OpenDialogue(GuardAccessibleAccess);
                else OpenDialogue(GuardAwaitingRegistration);
            }
            else return false;
            return true;
        }

        bool TryTalkToCampusPerson()
        {
            var person = World.NearbyPerson(Room,Player.Position);
            if(person==null)return false;
            OpenDialogue(new[]{ new DialogueLine(person.Name,person.Sprite,person.Greeting) });
            return true;
        }

        void CreateDialogue(Transform safe)
        {
            var intro=Rect(safe,"Cartela de apertura",new Vector2(.077f,.745f),new Vector2(.47f,.937f));
            arrivalTitle=intro.gameObject;
            var ground=intro.gameObject.AddComponent<UnityEngine.UI.Image>();
            ground.color=new Color(.045f,.076f,.105f,.95f);
            ground.raycastTarget=false;
            var rule=Rect(intro,"Línea de apertura",new Vector2(.045f,.17f),new Vector2(.054f,.83f))
                .gameObject.AddComponent<UnityEngine.UI.Image>();
            rule.color=new Color(.96f,.65f,.36f);
            rule.raycastTarget=false;
            var place=Label(intro,"Lugar",new Vector2(.081f,.54f),new Vector2(.94f,.86f),22);
            place.alignment=TextAnchor.MiddleLeft;
            place.color=rule.color;
            place.text="LA PAZ · BLOQUE A";
            var title=Label(intro,"Capítulo",new Vector2(.081f,.18f),new Vector2(.95f,.57f),33);
            title.alignment=TextAnchor.MiddleLeft;
            title.text="Primer día de inducción";
            arrivalTitle.SetActive(false);

            bool mobile=Application.isMobilePlatform;
            var panel = Panel(safe, "Diálogo",
                mobile?new Vector2(.04f,.035f):new Vector2(.10f,.045f),
                mobile?new Vector2(.96f,.40f):new Vector2(.90f,.30f));
            dialoguePanel = panel.gameObject;
            dialoguePortrait = Icon(panel, "yamil-00", new Vector2(.025f,.14f), new Vector2(.13f,.84f));
            dialogueSpeaker = Label(panel, "Personaje", new Vector2(.15f,.68f), new Vector2(.96f,.92f), 27);
            dialogueSpeaker.color = new Color(1f,.73f,.39f);
            dialogueSpeaker.alignment = TextAnchor.MiddleLeft;
            dialogueBody = Label(panel, "Texto", new Vector2(.15f,mobile?.39f:.30f), new Vector2(.96f,.68f), mobile?31:26);
            dialogueBody.alignment = TextAnchor.UpperLeft;
            var button = Button(panel, "Continuar",
                new Vector2(mobile?.60f:.695f,.065f), new Vector2(.96f,mobile?.36f:.29f), Player.Interact, mobile?29:20);
            dialogueAction = button.GetComponentInChildren<Text>();
            dialogueContinue=button.gameObject;
            var choices=Rect(panel,"Respuesta",Vector2.zero,Vector2.one);
            dialogueChoices=choices.gameObject;
            acceptChoice=Button(choices,"Abrir el paso",new Vector2(mobile?.28f:.48f,.07f),new Vector2(mobile?.62f:.72f,mobile?.36f:.28f),()=>CloseDialogue(true),mobile?27:19);
            Button(choices,"Ahora no",new Vector2(mobile?.65f:.73f,.07f),new Vector2(.96f,mobile?.36f:.28f),()=>CloseDialogue(false),mobile?27:19);
            dialogueChoices.SetActive(false);
            dialoguePanel.SetActive(false);
        }

        void OpenDialogue(DialogueLine[] lines, System.Action onComplete = null,bool choice=false,
            string acceptLabel="Abrir el paso")
        {
            if (tutorialActive && !ReferenceEquals(lines, ArrivalDialogue)) tutorialInteracted = true;
            dialogue = lines;
            arrivalTitle.SetActive(ReferenceEquals(lines,ArrivalDialogue));
            dialogueCompleted = onComplete;
            requiresChoice = choice;
            if(choice)
            {
                acceptChoice.name=acceptLabel;
                acceptChoice.GetComponentInChildren<Text>().text=acceptLabel;
            }
            dialogueIndex = 0;
            Player.TouchAxis(Vector2.zero);
            pause.enabled = false;
            pause.pauseButton.SetActive(false);
            touch.SetActive(false);
            hintPanel.SetActive(false);
            statusPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            dialoguePanel.SetActive(true);
            ShowDialogueLine();
        }

        int voicedLetters;
        float nextVoicePulse;

        void ShowDialogueLine()
        {
            var line = dialogue[dialogueIndex];
            dialogueSpeaker.text = line.Speaker;
            dialogueReveal = 0;
            visibleCharacters = 0;
            voicedLetters=0;
            nextVoicePulse=0;
            Sound.StopDialogueVoice();
            dialogueBody.text = "";
            dialogueContinue.SetActive(true);
            dialogueChoices.SetActive(false);
            dialoguePortrait.sprite = CampusLook.Sprite(CampusLook.Person(line.Speaker,line.Sprite));
            string action = dialogueIndex + 1 == dialogue.Length ? "Cerrar" : "Continuar";
            dialogueAction.text = Application.isMobilePlatform ? action : action + " [E]";
        }

        void UpdateDialogue()
        {
            if (!DialogueOpen) return;
            var line = dialogue[dialogueIndex];
            if (visibleCharacters >= line.Text.Length) return;
            dialogueReveal += Time.unscaledDeltaTime * 52f;
            int count = Mathf.Min(line.Text.Length, Mathf.FloorToInt(dialogueReveal));
            if (count == visibleCharacters) return;
            int pulse=-1;
            for(int i=visibleCharacters;i<count;i++){
                if(!char.IsLetterOrDigit(line.Text[i]))continue;
                if(voicedLetters%4==0)pulse=i;
                voicedLetters++;
            }
            if(pulse>=0&&Time.unscaledTime>=nextVoicePulse){
                Sound.DialogueVoice(line.Speaker,line.Text[pulse],pulse);
                nextVoicePulse=Time.unscaledTime+.06f;
            }
            visibleCharacters = count;
            dialogueBody.text = line.Text.Substring(0,count);
        }

        void AdvanceDialogue()
        {
            if(dialogueChoices.activeSelf)return;
            pause.sound.Click();
            string current = dialogue[dialogueIndex].Text;
            if (visibleCharacters < current.Length)
            {
                Sound.StopDialogueVoice();
                visibleCharacters = current.Length;
                dialogueBody.text = current;
                return;
            }
            if(requiresChoice&&dialogueIndex+1==dialogue.Length){
                dialogueContinue.SetActive(false);
                dialogueChoices.SetActive(true);
                StartCoroutine(FocusChoice());
                return;
            }
            dialogueIndex++;
            if (dialogueIndex < dialogue.Length)
            {
                ShowDialogueLine();
                return;
            }
            CloseDialogue(true);
        }

        IEnumerator FocusChoice()
        {
            yield return null;
            if(dialogueChoices.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(acceptChoice.gameObject);
        }

        void CloseDialogue(bool accepted)
        {
            Sound.StopDialogueVoice();
            dialogue = null;
            dialoguePanel.SetActive(false);
            arrivalTitle.SetActive(false);
            statusPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            pause.enabled = true;
            pause.pauseButton.SetActive(true);
            var onComplete = dialogueCompleted;
            dialogueCompleted = null;
            if(accepted)onComplete?.Invoke();
            RefreshHUD();
        }
    }
}
