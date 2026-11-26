using UnityEngine;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (int Floor,string Room,string Name,string Brief)[] RadioCrew=
        {
            (1,"Dirección de Carrera","Camila","Puedo leer el plano y cantar el rumbo por radio. No tengo guantes para tocar el mecanismo."),
            (2,"Histología y Fisiología","Iván","Tengo guantes y herramientas para mantener abierto el mecanismo. Dejé mi radio en el laboratorio."),
            (5,"Aula de grupos","Paula","Conozco el protocolo de primeros auxilios. Puedo recibir a quienes crucen y revisar si necesitan ayuda.")
        };
        static readonly string[] CrewPosts={"Mando mecánico","Recepción de evacuados","Guía por radio"};
        static readonly int[] ExpectedCrew={1,2,0};
        const int AllCrew=7;
        int crewContacts, pendingCrewPost=-1;
        bool crewAssigned;
        readonly int[] crewDraft={-1,-1,-1};
        readonly GameObject[] crewViews=new GameObject[3];
        bool crewPostsShown;

        Vector2 CrewDesk()=>World.Cell(DarkGround,30,14);
        Vector2 CrewContact()=>World.Entry(Room)+new Vector2(-2,2);
        Vector2 CrewPost(int index)=>World.Cell(DarkGround,59,6+index*2);

        int CrewMemberInRoom()
        {
            if(!IsUpperInterior(Room))return -1;
            string title=Title(Room);
            for(int i=0;i<RadioCrew.Length;i++)
                if(RadioCrew[i].Room==title)return i;
            return -1;
        }

        void ShowCrewContacts()
        {
            if(Room==DarkGround)
                World.MarkEscapeContact(Room,CrewDesk(),"Puesto de coordinación","bloque-a-panel-horario");
            int person=CrewMemberInRoom();
            if(person>=0&&!crewAssigned)
                crewViews[person]=World.MarkEscapeContact(Room,CrewContact(),RadioCrew[person].Name,"pb-estudiante");
        }

        void ShowCrewPosts()
        {
            if(Room!=DarkGround||!crewAssigned||crewPostsShown)return;
            crewPostsShown=true;
            for(int i=0;i<ExpectedCrew.Length;i++)
                World.MarkEscapeContact(Room,CrewPost(i),RadioCrew[ExpectedCrew[i]].Name,"pb-estudiante");
        }

        bool TryCrewInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Room==DarkGround&&Near(CrewDesk(),.84f))
            {
                prompt="Revisar · Puesto de coordinación";
                if(interact)
                {
                    if(crewAssigned)Notify("Equipo en posición");
                    else if(!acousticsApproved)Notify("El equipo necesita el canal acústico");
                    else if(!GallerySolved)Notify("Libera el cerrojo de la Galería de servicio antes de reunir al equipo.");
                    else if(crewContacts!=AllCrew)Notify("Habla con Camila, Iván y Paula antes de asignar puestos",5);
                    else
                    {
                        System.Array.Fill(crewDraft,-1);
                        pendingCrewPost=0;
                        escapeKeypad.SetActive(false);
                        RefreshCrewPanel();
                        ShowMechanismPanel(escapeOptions[0].gameObject);
                    }
                }
                return true;
            }
            if(Room==DarkGround&&crewAssigned)
            {
                for(int i=0;i<CrewPosts.Length;i++)
                {
                    if(!Near(CrewPost(i),.83f))continue;
                    var member=RadioCrew[ExpectedCrew[i]];
                    prompt="Hablar · "+member.Name;
                    if(interact)OpenDialogue(new[]{new DialogueLine(member.Name,"pb-estudiante",CrewPosts[i]+" listo. Abre el portón cuando recibas la señal.")});
                    return true;
                }
            }
            int index=CrewMemberInRoom();
            if(index<0||crewAssigned||!Near(CrewContact(),.85f))return false;
            var person=RadioCrew[index];
            prompt="Hablar · "+person.Name;
            if(interact)OpenDialogue(new[]{new DialogueLine(person.Name,"pb-estudiante",person.Brief)},()=>crewContacts|=1<<index);
            return true;
        }

        string CrewObjective()
        {
            for(int i=0;i<RadioCrew.Length;i++)
                if(!HasEscapeFlag(crewContacts,i))
                    return ThoughtOnFloor(RadioCrew[i].Floor,"Tengo que buscar a "+RadioCrew[i].Name+" en "+RadioCrew[i].Room);
            return ThoughtOnFloor(0,"Ya hablé con todos. Tengo que repartir los puestos de evacuación en la mesa");
        }

        void RefreshCrewPanel()
        {
            escapeTitle.text="Coordinación · "+CrewPosts[pendingCrewPost];
            var brief=new System.Text.StringBuilder("Asigna una persona distinta a cada puesto.\n");
            for(int i=0;i<RadioCrew.Length;i++)
            {
                var member=RadioCrew[i];
                brief.AppendLine().Append(member.Name).Append(" · ").Append(member.Brief);
                escapeOptions[i].gameObject.SetActive(true);
                escapeOptions[i].interactable=System.Array.IndexOf(crewDraft,i)<0;
                escapeOptions[i].GetComponentInChildren<Text>().text=member.Name;
            }
            escapeBody.text=brief.ToString();
        }

        void ChooseCrewMember(int member)
        {
            if(System.Array.IndexOf(crewDraft,member)>=0)return;
            crewDraft[pendingCrewPost++]=member;
            if(pendingCrewPost<CrewPosts.Length)
            {
                RefreshCrewPanel();
                foreach(var option in escapeOptions)
                    if(option.interactable)
                    {
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(option.gameObject);
                        break;
                    }
                return;
            }
            for(int i=0;i<ExpectedCrew.Length;i++)
            {
                if(crewDraft[i]==ExpectedCrew[i])continue;
                System.Array.Fill(crewDraft,-1);
                pendingCrewPost=0;
                RefreshCrewPanel();
                escapeTitle.text="Asignación rechazada · "+CrewPosts[i];
                Sound.Scanner();
                return;
            }
            crewAssigned=true;
            foreach(var view in crewViews)if(view)view.SetActive(false);
            ShowCrewPosts();
            CloseEscapePanel();
            Confirm("Equipo en posición · el portón queda a tu cargo",5);
        }
    }
}
