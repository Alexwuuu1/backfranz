using UnityEngine;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (int X,int Y,string Label,string Reading)[] PressureReadings =
        {
            (6,11,"Manómetro de ingreso","La línea entra a 18 unidades de presión."),
            (20,12,"Medidor de retorno","El retorno consume 7 unidades."),
            (27,10,"Placa de purga","La purga consume otras 3 unidades antes de calibrar.")
        };

        const int AllPressureReadings=(1<<3)-1;
        int maintenanceReadings;
        bool maintenanceApproved, openMaintenance;
        Button basementButton;
        readonly SpriteRenderer[] pressureViews=new SpriteRenderer[PressureReadings.Length];
        SpriteRenderer regulatorView;
        Vector2 CalibrationPoint()=>World.Cell(Basement,24,10);

        void ShowMaintenanceContacts()
        {
            if(Room!=Basement)return;
            for(int i=0;i<PressureReadings.Length;i++)
            {
                var reading=PressureReadings[i];
                pressureViews[i]=World.PlaceServiceEquipment(Room,World.Cell(Room,reading.X,reading.Y),reading.Label,
                    "utileria-medidor-industrial",new Vector2(1.3f,1.45f));
            }
            regulatorView=World.PlaceServiceEquipment(Room,CalibrationPoint(),"Regulador de emergencia",
                "bloque-a-item-tablero-activo",new Vector2(1.77f,1.76f));
            RefreshMaintenanceReadings();
        }

        void RefreshMaintenanceReadings()
        {
            var checkedColor=new Color(.61f,1f,.79f);
            for(int i=0;i<pressureViews.Length;i++)
                pressureViews[i].color=HasEscapeFlag(maintenanceReadings,i)?checkedColor:new Color(.94f,.79f,.66f);
            regulatorView.color=maintenanceApproved?checkedColor:World.Power?Color.white:new Color(.53f,.54f,.60f);
        }

        bool TryMaintenanceInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Room!=Basement)return false;
            for(int i=0;i<PressureReadings.Length;i++)
            {
                var reading=PressureReadings[i];
                if(!Near(World.Cell(Room,reading.X,reading.Y),.85f))continue;
                prompt="Leer · "+reading.Label;
                if(interact)
                {
                    maintenanceReadings|=1<<i;
                    RefreshMaintenanceReadings();
                    Confirm(reading.Reading,4);
                }
                return true;
            }
            if(!Near(CalibrationPoint(),.92f))return false;
            prompt="Revisar · Regulador";
            if(interact)OpenMaintenancePanel();
            return true;
        }

        void OpenMaintenancePanel()
        {
            if(maintenanceApproved){Notify("Regulador habilitado");return;}
            if(!World.Power||maintenanceReadings!=AllPressureReadings)
            {
                Notify("Restablece la energía y revisa los tres medidores",5);
                return;
            }
            openMaintenance=true;
            escapeTitle.text="Regulador del subsuelo";
            escapeBody.text="Selecciona la presión disponible después de descontar el retorno y la purga.\n\n"+
                PressureReadings[0].Reading+" "+PressureReadings[1].Reading+" "+PressureReadings[2].Reading;
            escapeKeypad.SetActive(false);
            string[] settings={"8 unidades","11 unidades","14 unidades"};
            for(int i=0;i<escapeOptions.Length;i++)
            {
                escapeOptions[i].gameObject.SetActive(true);
                escapeOptions[i].interactable=true;
                escapeOptions[i].GetComponentInChildren<Text>().text=settings[i];
            }
            ShowMechanismPanel(escapeOptions[0].gameObject);
        }

        void ChooseMaintenanceSetting(int option)
        {
            if(option!=0){Sound.Scanner();escapeTitle.text="Regulador · ajuste rechazado";return;}
            maintenanceApproved=true;
            RefreshMaintenanceReadings();
            CloseEscapePanel();
            Confirm("Presión estabilizada · el portón ya puede recibir la orden",5);
        }

        public void ChooseBasement()
        {
            if(!LiftOpen||!IsDark(Room)||!InvestigationComplete||liftOrigin==Basement)return;
            CloseLift();
            StartCoroutine(Travel(Basement,false,World.Entry(Basement)));
        }
    }
}
