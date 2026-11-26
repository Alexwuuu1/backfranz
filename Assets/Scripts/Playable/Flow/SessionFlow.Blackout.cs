using UnityEngine;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (int Floor, string Room, string Title, string Description, int CorrectTime)[] BlackoutTapes =
        {
            (0, "Financiero", "Molinetes", "La pantalla marca una salida. El corredor está vacío. Al final del clip, el lector se pone rojo.", 4),
            (2, "Sala de Simulación", "Monitor de la camilla", "Se apaga el techo. El monitor se mantiene unos segundos con la batería y registra un pulso.", 1),
            (3, "Fotografía", "Destello", "El flash ilumina la pared. En el reflejo del vidrio hay alguien de espaldas.", 3),
            (5, "Taller", "Prensa", "La prensa da un golpe con el interruptor abajo. La bandeja de trabajo se mueve sola.", 2)
        };

        static readonly (int Floor, string Room, string Name, string Sprite, string Statement)[] BlackoutWitnesses =
        {
            (0, "Admisiones", "Vera", "pb-recepcionista", "La camilla abrió la secuencia. El monitor pasó a batería a las 19:10. Miré el reloj porque estaba cerrando admisiones."),
            (1, "Dirección de Carrera", "Mónica", "pb-recepcionista", "Recibí el parte del taller. Su prensa golpeó cinco minutos después de que el monitor de la camilla pasara a batería."),
            (4, "Aula 4A", "Sara", "pb-estudiante", "El destello de Fotografía se vio por la ventana del pasillo. Fue cinco minutos después del golpe de la prensa."),
            (6, "Sala de lectura", "Raúl", "pb-estudiante", "El reloj de recepción anotó las 19:30 para el cierre de los molinetes. Fue lo último que apareció en las cámaras.")
        };

        const int AllBlackoutClues = 15;
        static readonly string[] BlackoutTimes = {"Elegir hora", "19:10", "19:15", "19:20", "19:30"};
        int blackoutTapes, blackoutWitnesses;
        bool blackoutRebuilt;
        GameObject blackoutControls;
        GameObject blackoutReflection;
        Text blackoutResult;
        readonly int[] blackoutDraft = new int[BlackoutTapes.Length];
        readonly Button[] blackoutDials = new Button[BlackoutTapes.Length];
        readonly Text[] blackoutDialLabels = new Text[BlackoutTapes.Length];
        Vector2 BlackoutDesk() => World.Cell(DarkGround, 47, 14);
        Vector2 BlackoutContact() => World.Entry(Room) + new Vector2(2, 2);

        int BlackoutTapeInRoom()
        {
            if(!IsInterior(Room)) return -1;
            for(int i = 0; i < BlackoutTapes.Length; i++)
                if(BlackoutTapes[i].Room == Title(Room)) return i;
            return -1;
        }

        int BlackoutWitnessInRoom()
        {
            if(!IsInterior(Room)) return -1;
            for(int i = 0; i < BlackoutWitnesses.Length; i++)
                if(BlackoutWitnesses[i].Room == Title(Room)) return i;
            return -1;
        }

        void ShowBlackoutContacts()
        {
            if(Room == DarkGround)
                World.MarkEscapeContact(Room, BlackoutDesk(), "Archivo de cámaras", "bloque-a-item-tablero-apagado");
            int tape = BlackoutTapeInRoom();
            if(tape >= 0)
                World.MarkEscapeContact(Room, BlackoutContact(), "Clip · " + BlackoutTapes[tape].Title, "bloque-a-panel-horario");
            int witness = BlackoutWitnessInRoom();
            if(witness >= 0)
            {
                var person = BlackoutWitnesses[witness];
                World.MarkEscapeContact(Room, BlackoutContact(), person.Name, person.Sprite);
            }
        }

        void ShowBlackoutReflection()
        {
            if(Room != DarkGround || !blackoutRebuilt || finaleStage > 0 || blackoutReflection) return;
            blackoutReflection = World.MarkEscapeContact(Room, World.Cell(DarkGround, 50, 17),
                "Reflejo en el vidrio", "shadow-02");
            blackoutReflection.GetComponent<SpriteRenderer>().color = new Color(.48f,.59f,.67f,.57f);
        }

        bool TryBlackoutInteraction(bool interact, out string prompt)
        {
            prompt = "";
            if(Room == DarkGround && Near(BlackoutDesk(), .83f))
            {
                prompt = "Revisar · Archivo de cámaras";
                if(interact) ReviewBlackoutDesk();
                return true;
            }
            if(!IsInterior(Room) || !Near(BlackoutContact(), .72f)) return false;
            int tape = BlackoutTapeInRoom();
            if(tape >= 0)
            {
                var clip = BlackoutTapes[tape];
                prompt = "Ver clip · " + clip.Title;
                if(interact)
                    OpenDialogue(new[]{new DialogueLine("Grabación", "bloque-a-panel-horario", clip.Description)},
                        () => blackoutTapes |= 1 << tape);
                return true;
            }
            int witness = BlackoutWitnessInRoom();
            if(witness < 0) return false;
            var person = BlackoutWitnesses[witness];
            prompt = "Hablar · " + person.Name;
            if(interact)
                OpenDialogue(new[]{new DialogueLine(person.Name, person.Sprite, person.Statement)},
                    () => blackoutWitnesses |= 1 << witness);
            return true;
        }

        string BlackoutObjective()
        {
            for(int i = 0; i < BlackoutTapes.Length; i++)
                if(!HasEscapeFlag(blackoutTapes,i))
                    return ThoughtOnFloor(BlackoutTapes[i].Floor,"Me falta una grabación del apagón. Debería buscarla en "+BlackoutTapes[i].Room);
            for(int i = 0; i < BlackoutWitnesses.Length; i++)
                if(!HasEscapeFlag(blackoutWitnesses,i))
                    return ThoughtOnFloor(BlackoutWitnesses[i].Floor,
                        "Puedo preguntarle a "+BlackoutWitnesses[i].Name+" qué pasó en "+BlackoutWitnesses[i].Room);
            return ThoughtOnFloor(0,"Tengo que reconstruir el apagón en el archivo de cámaras");
        }

        void BuildBlackoutControls(Transform parent)
        {
            var layout = Rect(parent, "Cronología del apagón", Vector2.zero, Vector2.one);
            blackoutControls = layout.gameObject;
            for(int i = 0; i < blackoutDials.Length; i++)
            {
                int index = i;
                float bottom = .455f - i * .092f;
                var name = Label(layout, "Clip " + (i + 1), new Vector2(.09f,bottom), new Vector2(.60f,bottom+.075f), 24);
                name.alignment = TextAnchor.MiddleLeft;
                name.text = BlackoutTapes[i].Title;
                blackoutDials[i] = Button(layout, "Hora " + (i + 1), new Vector2(.61f,bottom), new Vector2(.91f,bottom+.075f),
                    () => CycleBlackoutTime(index), 23);
                blackoutDialLabels[i] = blackoutDials[i].GetComponentInChildren<Text>();
            }
            blackoutResult = Label(layout, "Resultado de la cronología", new Vector2(.09f,.55f), new Vector2(.91f,.64f), 22);
            Button(layout, "Reproducir", new Vector2(.68f,.055f), new Vector2(.94f,.143f), VerifyBlackoutTimeline, 22);
            blackoutControls.SetActive(false);
        }

        void ReviewBlackoutDesk()
        {
            if(blackoutRebuilt)
            {
                Notify("Grabación sincronizada · el paso de servicio figura como habilitado");
                return;
            }
            if(!SignalsComplete)
            {
                Notify("El archivo de cámaras sigue desconectado. Restablece la red de señales.");
                return;
            }
            if(blackoutTapes != AllBlackoutClues || blackoutWitnesses != AllBlackoutClues)
            {
                Notify("El archivo pide cuatro clips y cuatro testimonios. Consulta el diario.",5);
                return;
            }
            escapeTitle.text = "Reconstruir el apagón";
            escapeBody.text = "Asigna una hora distinta a cada clip según los testimonios.";
            escapeBody.rectTransform.anchorMin = new Vector2(.07f,.66f);
            foreach(var option in escapeOptions) option.gameObject.SetActive(false);
            escapeKeypad.SetActive(false);
            for(int i = 0; i < blackoutDials.Length; i++)
                blackoutDialLabels[i].text = BlackoutTimes[blackoutDraft[i]];
            blackoutResult.text = "";
            blackoutControls.SetActive(true);
            ShowMechanismPanel(blackoutDials[0].gameObject);
        }

        void CycleBlackoutTime(int index)
        {
            if(!EscapePanelOpen || !blackoutControls.activeSelf) return;
            blackoutDraft[index] = (blackoutDraft[index] + 1) % BlackoutTimes.Length;
            blackoutDialLabels[index].text = BlackoutTimes[blackoutDraft[index]];
            blackoutResult.text = "";
            Sound.Click();
        }

        void VerifyBlackoutTimeline()
        {
            if(!EscapePanelOpen || !blackoutControls.activeSelf) return;
            int usedTimes = 0;
            foreach(int time in blackoutDraft)
            {
                if(time == 0 || HasEscapeFlag(usedTimes,time))
                {
                    blackoutResult.text = "Elige una hora distinta para cada clip.";
                    return;
                }
                usedTimes |= 1 << time;
            }
            for(int i = 0; i < blackoutDraft.Length; i++)
            {
                if(blackoutDraft[i] == BlackoutTapes[i].CorrectTime) continue;
                blackoutResult.text = "El montaje no coincide con los testimonios.";
                return;
            }

            blackoutRebuilt = true;
            CloseEscapePanel();
            ShowBlackoutReflection();
            Sound.Playback();
            OpenDialogue(new[]{
                new DialogueLine("Grabación","bloque-a-panel-horario","19:30. Paso de servicio habilitado. Salidas registradas: una."),
                new DialogueLine("Yamil","yamil-00","El portón seguía cerrado en las cuatro imágenes. ¿Por dónde salió esa persona?")
            });
        }

        void CloseBlackoutControls()
        {
            if(!blackoutControls.activeSelf) return;
            blackoutControls.SetActive(false);
            escapeBody.rectTransform.anchorMin = new Vector2(.07f,.54f);
        }
    }
}
