using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (int Floor,string Room,string Speaker,string Statement)[] Evidence =
        {
            (1,"Caja","Iris","El registro de llaves marca tres retiros. Cada persona retiró una sola llave: 18:05, 18:15 y 18:25."),
            (4,"Sala Jurídica","Renata","Tomás retiró antes que Bruno. Elisa firmó al último. La llave del portón salió a las 18:15."),
            (2,"Cocina de prácticas","Andrés","El ramal A terminó seco. El vapor salió por un solo ramal, y la tubería de retorno siguió fría."),
            (3,"Sala Mac","Mateo","El ramal B es retorno. La alarma pide aislar el ramal por el que pasó vapor, no el retorno."),
            (5,"Taller","Eva","Hay tres salidas en el esquema: Sur, Patio y Servicio. El sensor del acceso Sur marcó bloqueo."),
            (6,"Sala de lectura","Alma","El Patio quedó sin continuidad. La consola debe seleccionar el acceso que conserva continuidad y apertura.")
        };

        static readonly (string Name,string Question,int Answer,string[] Options)[] Cases =
        {
            ("Registro de llaves","¿Quién retiró la llave del portón?",1,new[]{"Tomás","Bruno","Elisa"}),
            ("Aislamiento de vapor","¿Qué ramal debe aislarse?",2,new[]{"Ramal A","Ramal B","Ramal C"}),
            ("Salida de emergencia","¿Qué acceso admite la orden de evacuación?",0,new[]{"Servicio","Patio","Sur"})
        };

        const int AllCases=(1<<3)-1, AllEvidence=(1<<6)-1;
        int caseEvidence, solvedCases, openCase=-1;
        bool InvestigationComplete=>solvedCases==AllCases;
        Vector2 EvidencePoint()=>World.Entry(Room)+Vector2.left*2+Vector2.up;
        Vector2 InvestigationDesk()=>World.Cell(DarkTop,15,9);

        int EvidenceInRoom()
        {
            if(!IsUpperInterior(Room))return -1;
            string title=UpperInteriorName(Room);
            for(int index=0;index<Evidence.Length;index++)
                if(Evidence[index].Room==title)return index;
            return -1;
        }

        void ShowInvestigationContacts()
        {
            int evidence=EvidenceInRoom();
            if(evidence>=0)
                World.MarkEscapeContact(Room,EvidencePoint(),Evidence[evidence].Speaker,"pb-estudiante");
        }

        bool TryInvestigationInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Room==DarkTop&&Near(InvestigationDesk(),.92f))
            {
                prompt="Revisar · Mesa de investigación";
                if(interact)OpenInvestigation();
                return true;
            }
            int index=EvidenceInRoom();
            if(index<0||!Near(EvidencePoint(),.90f))return false;
            var evidence=Evidence[index];
            prompt="Hablar · "+evidence.Speaker;
            if(interact)OpenDialogue(new[]{new DialogueLine(evidence.Speaker,"pb-estudiante",evidence.Statement)},
                ()=>caseEvidence|=1<<index);
            return true;
        }

        string InvestigationObjective()
        {
            for(int index=0;index<Evidence.Length;index++)
                if(!HasEscapeFlag(caseEvidence,index))
                    return ThoughtOnFloor(Evidence[index].Floor,"Me falta un testimonio. Debería buscarlo en "+Evidence[index].Room);
            return ThoughtOnFloor(6,"Tengo que cotejar los testimonios en la mesa del cruce central de la biblioteca");
        }

        void OpenInvestigation()
        {
            if(InvestigationComplete){Notify("Permiso de evacuación validado");return;}
            int index=0;
            while(HasEscapeFlag(solvedCases,index))index++;
            int first=index*2;
            int required=3<<first;
            if((caseEvidence&required)!=required)
            {
                Notify("Busca testimonios · "+Evidence[first].Room+" y "+Evidence[first+1].Room,5);
                return;
            }
            openCase=index;
            var item=Cases[index];
            escapeTitle.text=item.Name;
            escapeBody.text=item.Question+"\n\n"+Evidence[first].Statement+"\n"+Evidence[first+1].Statement;
            escapeKeypad.SetActive(false);
            for(int option=0;option<escapeOptions.Length;option++)
            {
                escapeOptions[option].gameObject.SetActive(true);
                escapeOptions[option].interactable=true;
                escapeOptions[option].GetComponentInChildren<Text>().text=item.Options[option];
            }
            ShowMechanismPanel(escapeOptions[0].gameObject);
        }

        void ChooseInvestigationAnswer(int option)
        {
            if(option!=Cases[openCase].Answer)
            {
                Sound.Scanner();
                Notify("La conclusión contradice el registro");
                escapeTitle.text=Cases[openCase].Name+" · revisa la deducción";
                return;
            }
            solvedCases|=1<<openCase;
            CloseEscapePanel();
            Confirm(InvestigationComplete?"Permiso de evacuación validado":"Deducción confirmada · revisa el siguiente expediente",4);
        }
    }
}
