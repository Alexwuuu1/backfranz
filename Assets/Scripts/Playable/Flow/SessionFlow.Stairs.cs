using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        GameObject stairPanel;
        Text stairTitle;
        Button stairUp;
        int stairCore, stairOrigin, stairDestination;
        bool StairsOpen => stairPanel.activeSelf;

        void CreateStairMenu(Transform safe)
        {
            var panel = Panel(safe, "Dirección de las gradas", new Vector2(.29f,.26f), new Vector2(.71f,.74f));
            stairPanel = panel.gameObject;
            stairTitle = Label(panel, "Gradas", new Vector2(.06f,.73f), new Vector2(.94f,.95f), 29);
            stairUp = Button(panel, "Subir", new Vector2(.08f,.39f), new Vector2(.47f,.63f), () => ChooseStairDirection(1), 24);
            Button(panel, "Bajar", new Vector2(.53f,.39f), new Vector2(.92f,.63f), () => ChooseStairDirection(-1), 24);
            Button(panel, "Cerrar", new Vector2(.29f,.09f), new Vector2(.71f,.30f), CloseStairMenu, 22);
            stairPanel.SetActive(false);
        }

        int NearbyStair()
        {
            if (Room < Ground || Room > Top) return -1;
            for (int core = 0; core < StairwellCount; core++)
                if (Near(World.Stairs(Room, core), .94f)) return core;
            return -1;
        }

        string StairPrompt()
        {
            if (IsStairwell(Room))
            {
                if (Near(World.StairLanding(Room, false), .96f)) return StairLandingLabel(false);
                if (Near(World.StairLanding(Room, true), .96f)) return StairLandingLabel(true);
                return "";
            }
            int core = NearbyStair();
            return core < 0 ? "" : StairName(core);
        }

        string StairLandingLabel(bool upper)
        {
            int floor = StairLandingRoom(upper);
            return (floor == stairOrigin ? "Volver · " : "Salir · ") + Floors[Floor(floor)];
        }

        int StairLandingRoom(bool upper)
            => upper ? Mathf.Max(stairOrigin, stairDestination) : Mathf.Min(stairOrigin, stairDestination);

        bool TryUseStairs()
        {
            if (IsStairwell(Room))
            {
                bool upper = Near(World.StairLanding(Room, true), .96f);
                if (!upper && !Near(World.StairLanding(Room, false), .96f)) return false;
                int destination = StairLandingRoom(upper);
                StartCoroutine(Travel(destination, false, World.Stairs(destination, stairCore)));
                return true;
            }

            int core = NearbyStair();
            if (core < 0) return false;
            stairCore = core;
            if (Room == Ground || Room == Top)
                ChooseStairDirection(Room == Ground ? 1 : -1);
            else
                OpenStairMenu();
            return true;
        }

        void OpenStairMenu()
        {
            stairTitle.text = StairName(stairCore);
            Player.TouchAxis(Vector2.zero);
            pause.enabled = false;
            pause.pauseButton.SetActive(false);
            Time.timeScale = 0;
            stairPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(stairUp.gameObject);
        }

        void CloseStairMenu()
        {
            if (!StairsOpen) return;
            stairPanel.SetActive(false);
            Time.timeScale = 1;
            pause.enabled = true;
            pause.pauseButton.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void ChooseStairDirection(int direction)
        {
            if (direction != -1 && direction != 1) return;
            if (!StairsOpen && Room != Ground && Room != Top) return;
            int destination = Room + direction;
            if (destination < Ground || destination > Top) return;
            CloseStairMenu();
            stairOrigin = Room;
            stairDestination = destination;
            int stairwell = FirstStairwell + stairCore;
            StartCoroutine(Travel(stairwell, false, World.StairLanding(stairwell, direction < 0)));
        }
    }
}
