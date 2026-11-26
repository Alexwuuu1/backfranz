using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        const string WorkshopCableName="Cable de extensión", WorkshopCableSprite="utileria-bobina-cable";
        bool hasWorkshopCable;
        int workshopLamp;
        readonly GameObject[] workshopCableViews=new GameObject[2];

        bool InWorkshop()=>IsUpperInterior(Room)&&UpperInteriorName(Room)=="Taller";
        bool InDarkWorkshopHall=>Room==ForFloor(5,true);

        void EnterWorkshop()
        {
            if(InDarkWorkshopHall)World.SetWorkshopLight(workshopLamp-1);
            if(hasWorkshopCable||!InWorkshop())return;
            int phase=IsDark(Room)?1:0;
            if(!workshopCableViews[phase])
                workshopCableViews[phase]=World.PlaceCollectible(Room,World.Cell(Room,14,10),.62f,WorkshopCableName,WorkshopCableSprite);
        }

        bool TryWorkshopCable(bool interact,out string prompt)
        {
            prompt="";
            if(!hasWorkshopCable&&InWorkshop()&&Near(World.Cell(Room,14,9),.85f))
            {
                prompt="Recoger · "+WorkshopCableName;
                if(interact)
                {
                    hasWorkshopCable=true;
                    foreach(var view in workshopCableViews)
                        if(view)view.SetActive(false);
                    Sound.PickUp();
                    Notify("Recogiste el cable de extensión.",3);
                }
                return true;
            }
            if(!InDarkWorkshopHall)return false;
            for(int i=0;i<2;i++)
            {
                if(!Near(World.WorkshopSocket(Room,i),.88f))continue;
                int lamp=i+1;
                bool connected=workshopLamp==lamp;
                prompt=connected?"Desconectar · Recuperar cable":
                    !hasWorkshopCable||workshopLamp!=0?"Revisar · Lámpara de trabajo":"Conectar cable · Encender lámpara";
                if(!interact)return true;
                if(!hasWorkshopCable)Notify("Esta lámpara necesita el cable del Taller.",3);
                else if(workshopLamp!=0&&!connected)Notify("El cable sigue enchufado en "+WorkshopLampLocation()+".",3);
                else
                {
                    workshopLamp=connected?0:lamp;
                    World.SetWorkshopLight(workshopLamp-1);
                    Sound.ToggleSwitch();
                    Notify(connected?"Recuperé el cable. La lámpara se apagó.":
                        "La lámpara encendió. Podemos reunirnos bajo esta luz.",3.8f);
                }
                return true;
            }
            return false;
        }

        string WorkshopLampLocation()=>workshopLamp==1?"el banco junto a Aula de grupos":"la mesa de montaje del pasillo inferior";

        bool NearWorkshopRefuge()=>workshopLamp>0&&InDarkWorkshopHall&&Near(World.WorkshopRefuge(Room,workshopLamp-1),.94f);

        string WorkshopRescueThought()
        {
            if(escorting!=ElenaEscort+1||(!InDarkWorkshopHall&&!IsEscortSource(ElenaEscort,Room)))return null;
            return workshopLamp>0?"Puedo llevar a Elena a la luz de "+WorkshopLampLocation()+".":
                "Tengo que acompañar a Elena hasta la luz que sigue encendida en el extremo este.";
        }
    }
}
