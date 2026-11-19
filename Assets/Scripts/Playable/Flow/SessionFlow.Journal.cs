using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        const int CluesPerPage = 2;
        readonly List<string> journalClues = new List<string>();
        GameObject journalPanel, journalButton;
        Text journalHeading, journalPage, journalStatus, journalPageNumber;
        Button journalBack, journalNext, journalClose;
        int journalCluePage;
        bool JournalOpen => journalPanel && journalPanel.activeSelf;

        void CreateEscapeJournal(Transform safe)
        {
            journalButton = Button(safe, "Diario", new Vector2(.39f,.91f), new Vector2(.52f,.98f), OpenJournal, 22).gameObject;
            journalButton.SetActive(false);
            var journal = Panel(safe, "Diario de escape", new Vector2(.13f,.09f), new Vector2(.87f,.90f));
            journalPanel = journal.gameObject;
            journalHeading = Label(journal, "Título del diario", new Vector2(.06f,.87f), new Vector2(.94f,.98f), 30);
            Button(journal, "Progreso", new Vector2(.07f,.755f), new Vector2(.275f,.85f), ShowJournalProgress, 23);
            Button(journal, "Pistas", new Vector2(.29f,.755f), new Vector2(.495f,.85f), ShowJournalClues, 23);
            Button(journal, "Rescates", new Vector2(.51f,.755f), new Vector2(.715f,.85f), ShowJournalRescues, 23);
            Button(journal, "Pedir orientación", new Vector2(.73f,.755f), new Vector2(.93f,.85f), ShowJournalHint, 21);
            journalPage = Label(journal, "Página del diario", new Vector2(.07f,.185f), new Vector2(.93f,.71f), 25);
            journalPage.alignment = TextAnchor.UpperLeft;
            journalStatus = Label(journal, "Evacuación", new Vector2(.53f,.185f), new Vector2(.93f,.71f), 24);
            journalStatus.alignment = TextAnchor.UpperLeft;
            journalPageNumber = Label(journal, "Número de página", new Vector2(.33f,.145f), new Vector2(.67f,.205f), 20);
            journalBack = Button(journal, "Anterior", new Vector2(.07f,.05f), new Vector2(.31f,.14f), PreviousCluePage, 22);
            journalNext = Button(journal, "Siguiente", new Vector2(.69f,.05f), new Vector2(.93f,.14f), NextCluePage, 22);
            journalClose = Button(journal, "Cerrar", new Vector2(.36f,.05f), new Vector2(.64f,.14f), CloseJournal, 23);
            journalPanel.SetActive(false);
        }

        void OpenJournal()
        {
            if(Horror != HorrorState.Active || Finished || Crossing || Time.timeScale == 0 || DialogueOpen) return;
            Player.TouchAxis(Vector2.zero);
            pause.enabled = false;
            pause.pauseButton.SetActive(false);
            Time.timeScale = 0;
            journalPanel.SetActive(true);
            Sound.Journal(true);
            journalCluePage = 0;
            CollectJournalClues();
            ShowJournalProgress();
            EventSystem.current.SetSelectedGameObject(journalClose.gameObject);
        }

        void CloseJournal()
        {
            if(!JournalOpen) return;
            journalPanel.SetActive(false);
            Sound.Journal(false);
            Time.timeScale = 1;
            pause.enabled = true;
            pause.pauseButton.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
        }

        void SetJournalLayout(bool twoColumns, bool showPagination)
        {
            journalPage.rectTransform.anchorMax = new Vector2(twoColumns ? .49f : .93f,.71f);
            journalStatus.gameObject.SetActive(twoColumns);
            journalPageNumber.gameObject.SetActive(showPagination);
            journalBack.gameObject.SetActive(showPagination);
            journalNext.gameObject.SetActive(showPagination);
        }

        void ShowJournalProgress()
        {
            SetJournalLayout(true, false);
            journalHeading.text = "Diario · " + EscapeCount() + "/" + EscapeTasks.Length;
            var mechanisms = new StringBuilder("MECANISMOS\n");
            for(int i = 0; i < EscapeTasks.Length; i++)
            {
                var task = EscapeTasks[i];
                mechanisms.Append("\n").Append(task.Name).Append(" · ");
                mechanisms.Append(HasEscapeFlag(escapeSolved,i) ? "Listo" :
                    HasEscapeFlag(escapeClues,i) ? "Pista anotada" : "Pendiente");
            }
            journalPage.text = mechanisms.ToString();

            var evacuation = new StringBuilder("EVACUACIÓN\n");
            evacuation.Append("\nPermiso · ").Append(InvestigationComplete ? "Validado" : "Pendiente");
            evacuation.Append("\nRegulador · ").Append(maintenanceApproved ? "Calibrado" : "Pendiente");
            evacuation.Append("\nRed de señales · ").Append(SignalsComplete ? "En línea" : "Pendiente");
            evacuation.Append("\nRegistro del apagón · ").Append(blackoutRebuilt ? "Sincronizado" : "Pendiente");
            evacuation.Append("\nCanal acústico · ").Append(acousticsApproved ? "Validado" : "Pendiente");
            evacuation.Append("\nGalería de servicio · ").Append(galleryProgress).Append("/").Append(GalleryMechanisms.Length);
            evacuation.Append("\nEquipo · ").Append(crewAssigned ? "En posición" : "Pendiente");
            int parts = 0;
            for(int i = 0; i < RelayParts.Length; i++)
                if(HasEscapeFlag(escapeParts,i)) parts++;
            evacuation.Append("\nPuentes del relé · ").Append(parts).Append("/").Append(RelayParts.Length);
            journalStatus.text = evacuation.ToString();
        }

        void ShowJournalRescues()
        {
            SetJournalLayout(true,false);
            journalHeading.text = "Rescates · "+RescuedCount()+"/"+RescueTotal;
            journalPage.text = SupplyRescueNotes();
            journalStatus.text = EscortRescueNotes();
        }

        void CollectJournalClues()
        {
            journalClues.Clear();
            for(int i = 0; i < EscapeTasks.Length; i++)
            {
                if(!HasEscapeFlag(escapeClues,i)) continue;
                var task = EscapeTasks[i];
                AddClue(task.Name, task.Witness + " · " + Floors[task.ClueFloor], task.Clue);
            }
            for(int i = 0; i < Evidence.Length; i++)
            {
                if(!HasEscapeFlag(caseEvidence,i)) continue;
                var evidence = Evidence[i];
                AddClue(Cases[i/2].Name, evidence.Speaker + " · " + evidence.Room, evidence.Statement);
            }
            for(int i = 0; i < PressureReadings.Length; i++)
                if(HasEscapeFlag(maintenanceReadings,i))
                    AddClue(PressureReadings[i].Label, "Subsuelo", PressureReadings[i].Reading);
            for(int i = 0; i < AcousticClues.Length; i++)
            {
                if(!HasEscapeFlag(acousticClues,i)) continue;
                var clue = AcousticClues[i];
                AddClue("Canal acústico", clue.Speaker + " · " + clue.Room, clue.Clue);
            }
            for(int i = 0; i < BlackoutTapes.Length; i++)
            {
                if(!HasEscapeFlag(blackoutTapes,i)) continue;
                var clip = BlackoutTapes[i];
                AddClue("Clip · " + clip.Title, clip.Room, clip.Description);
            }
            for(int i = 0; i < BlackoutWitnesses.Length; i++)
            {
                if(!HasEscapeFlag(blackoutWitnesses,i)) continue;
                var person = BlackoutWitnesses[i];
                AddClue("Registro del apagón", person.Name + " · " + person.Room, person.Statement);
            }
            for(int i = 0; i < GalleryWitnesses.Length; i++)
            {
                if(!HasEscapeFlag(galleryClues,i)) continue;
                var person = GalleryWitnesses[i];
                AddClue("Galería de servicio", person.Name, person.Statement);
            }
            for(int i = 0; i < RadioCrew.Length; i++)
            {
                if(!HasEscapeFlag(crewContacts,i)) continue;
                var person = RadioCrew[i];
                AddClue("Equipo de evacuación", person.Name + " · " + person.Room, person.Brief);
            }

            void AddClue(string subject, string source, string note)
            {
                journalClues.Add(subject + "\n" + source + "\n" + note);
            }
        }

        void ShowJournalClues()
        {
            int pages = (journalClues.Count + CluesPerPage - 1) / CluesPerPage;
            SetJournalLayout(false, pages > 1);
            journalHeading.text = "Pistas anotadas · " + journalClues.Count;
            if(pages == 0)
            {
                journalPage.text = "Todavía no hay pistas anotadas. Habla con quienes siguen en el edificio y revisa los equipos.";
                return;
            }

            int first = journalCluePage * CluesPerPage;
            int count = Mathf.Min(CluesPerPage, journalClues.Count - first);
            journalPage.text = string.Join("\n\n", journalClues.GetRange(first, count));
            journalPageNumber.text = (journalCluePage + 1) + " / " + pages;
            journalBack.interactable = journalCluePage > 0;
            journalNext.interactable = journalCluePage + 1 < pages;
        }

        void PreviousCluePage()
        {
            if(journalCluePage == 0) return;
            journalCluePage--;
            ShowJournalClues();
        }

        void NextCluePage()
        {
            if((journalCluePage + 1) * CluesPerPage >= journalClues.Count) return;
            journalCluePage++;
            ShowJournalClues();
        }

        void ShowJournalHint()
        {
            SetJournalLayout(false, false);
            journalHeading.text = "Orientación";
            journalPage.text = EscapeObjective();
        }
    }
}
