using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        bool tutorialActive, tutorialMoved, tutorialInteracted, tutorialPaused;
        float tutorialDistance;
        Vector2 tutorialPreviousPosition;
        int tutorialPreviousRoom;
        GameObject tutorialPanel;
        Text tutorialTitle, tutorialBody;

        void BeginTutorial()
        {
            tutorialActive = true;
            tutorialPreviousPosition = Player.Position;
            tutorialPreviousRoom = Room;
            var panel = Panel(statusPanel.transform, "Tutorial de primera partida",
                new Vector2(.02f, .57f), new Vector2(.37f, .78f));
            tutorialPanel = panel.gameObject;
            tutorialTitle = Label(panel, "Paso", new Vector2(.06f, .75f), new Vector2(.94f, .95f), 23);
            tutorialTitle.alignment = TextAnchor.MiddleLeft;
            tutorialTitle.color = new Color(1f, .73f, .39f);
            tutorialBody = Label(panel, "Instrucción", new Vector2(.06f, .25f), new Vector2(.94f, .73f), 22);
            tutorialBody.alignment = TextAnchor.UpperLeft;
            Button(panel, "Omitir tutorial", new Vector2(.52f, .04f), new Vector2(.94f, .23f), EndTutorial, 18);
            UpdateTutorial();
        }

        void UpdateTutorial()
        {
            if (!tutorialActive) return;
            bool available = !Finished && !Crossing && !LiftOpen && !DialogueOpen &&
                !EscapePanelOpen && !JournalOpen && !StairsOpen;
            float distance = Vector2.Distance(Player.Position, tutorialPreviousPosition);
            if (available && Time.timeScale > 0 && Room == tutorialPreviousRoom && distance < 1f)
                tutorialDistance += distance;
            tutorialPreviousPosition = Player.Position;
            tutorialPreviousRoom = Room;
            tutorialMoved |= tutorialDistance >= 2f;
            tutorialInteracted |= Room != Entrance;
            tutorialPaused |= available && Time.timeScale == 0 && pause.menu.activeInHierarchy;
            tutorialPanel.SetActive(available && Time.timeScale > 0);

            if (!tutorialMoved)
            {
                tutorialTitle.text = "TUTORIAL · 1/4 · DESPLAZARSE";
                tutorialBody.text = Application.isMobilePlatform
                    ? "Usa las flechas de abajo a la izquierda. Camina unos pasos por la vereda."
                    : "Muévete con WASD o las flechas. Camina unos pasos. Mantén Shift para correr.";
            }
            else if (!tutorialInteracted)
            {
                tutorialTitle.text = "TUTORIAL · 2/4 · INTERACTUAR";
                tutorialBody.text = Application.isMobilePlatform
                    ? "Acércate al estudiante y toca el botón de la mano. Toca Continuar para avanzar el diálogo."
                    : "Acércate al estudiante y pulsa E o Espacio. En los diálogos también puedes usar Enter.";
            }
            else if (!tutorialPaused)
            {
                tutorialTitle.text = "TUTORIAL · 3/4 · PAUSA Y GUARDADO";
                tutorialBody.text = Application.isMobilePlatform
                    ? "Abre PAUSA arriba a la derecha. Allí puedes guardar. Elige Reanudar para volver."
                    : "Pulsa Escape para abrir la pausa. Allí puedes guardar. Elige Reanudar para volver.";
            }
            else if (!HasCard)
            {
                tutorialTitle.text = "TUTORIAL · 4/4 · TU PRIMER OBJETIVO";
                tutorialBody.text = "Sigue la misión de arriba. Sube las gradas, entra al Bloque A y habla en la ventanilla de Recepción.";
            }
            else
            {
                EndTutorial();
                Notify("Tutorial completado · consulta tu misión y reúne las seis constancias", 6);
            }
        }

        void EndTutorial()
        {
            tutorialActive = false;
            tutorialPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
