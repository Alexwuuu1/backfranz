using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        const string RadioItemName="Radio de mano", RadioItemSprite="utileria-radio-mano";
        bool hasCampusRadio;
        readonly GameObject[] campusRadioViews=new GameObject[2];

        bool InStreaming()=>IsUpperInterior(Room)&&UpperInteriorName(Room)=="Streaming";

        void ShowCampusRadio()
        {
            if(hasCampusRadio||!InStreaming())return;
            int phase=IsDark(Room)?1:0;
            if(!campusRadioViews[phase])
                campusRadioViews[phase]=World.PlaceCollectible(Room,World.Cell(Room,4,10),.57f,RadioItemName,RadioItemSprite);
        }

        bool TryCampusRadio(bool interact,out string prompt)
        {
            prompt="";
            if(hasCampusRadio||!InStreaming()||!Near(World.Cell(Room,4,9),.85f))return false;
            prompt="Recoger · "+RadioItemName;
            if(interact)
            {
                hasCampusRadio=true;
                foreach(var view in campusRadioViews)
                    if(view)view.SetActive(false);
                Sound.PickUp();
                Notify("Recogiste la radio de mano.",3);
            }
            return true;
        }

        string CampusRadioMessage()
        {
            if(!IsDark(Room))return "Lina · Prueba de sonido. Si me escuchas, la radio funciona.";
            if(finaleStage>0)return "Camila · El equipo está junto al portón. Sigue las marcas del piso hasta el giro.";
            if(Room==ForFloor(3,true)||IsDarkUpperInterior(Room)&&Floor(Room)==3)
            {
                if(!SignalsComplete)return "Voz entrecortada · Sonido está cerrado. La puerta necesita la red de señales.";
                if(!acousticsApproved)return "Lina · Busca las pistas de Streaming, Fotografía y Sala Mac antes de entrar en Sonido.";
                return "Lina · El canal acústico ya responde. Avísales a los demás.";
            }
            return "Canal de emergencia · "+EscapeObjective();
        }

        void ListenCampusRadio()
        {
            if(!hasCampusRadio||!BackpackOpen)return;
            Sound.RecordedVoice();
            backpackDescription.text=CampusRadioMessage();
        }
    }
}
