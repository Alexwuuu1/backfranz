using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        const string ServicesPassName="Credencial de Mónica";
        const string ServicesPassSprite="bloque-a-item-credencial-salida";
        bool servicesPassFound, servicesPassReturned, servicesDetourShown;
        GameObject servicesPassView;
        float nextRelayBuzz;

        Vector2 ServicesPassContact()=>World.Cell(Room,21,6);

        void EnterServices()
        {
            if(Room!=ForFloor(1,true))return;
            if(!servicesPassFound&&!servicesPassView)
                servicesPassView=World.PlaceCollectible(Room,World.Cell(Room,21,4),.81f,ServicesPassName,ServicesPassSprite);
            if(servicesPassReturned&&!servicesDetourShown)
            {
                World.MarkServicesDetour(Room);
                servicesDetourShown=true;
            }
        }

        bool TryServicesPass(bool interact,out string prompt)
        {
            prompt="";
            if(Room==ForFloor(1,true)&&!servicesPassFound&&Near(ServicesPassContact(),.9f))
            {
                prompt="Recoger · "+ServicesPassName;
                if(interact)
                {
                    servicesPassFound=true;
                    servicesPassView.SetActive(false);
                    Sound.PickUp();
                    Notify("Recogiste la credencial de Mónica.",3.4f);
                }
                return true;
            }
            if(!servicesPassFound||servicesPassReturned||!IsDarkUpperInterior(Room)||
                UpperInteriorName(Room)!="Dirección de Carrera"||!Near(BlackoutContact(),.85f))return false;
            prompt="Devolver credencial · Mónica";
            if(interact)
                OpenDialogue(new[]{new DialogueLine("Mónica","pb-recepcionista",
                    "Gracias. La persiana quedó trabada. Baja por el cruce del ascensor, sigue el pasillo de Arca y sube por el otro extremo. Dejé marcas verdes por donde se puede pasar. El ruido del relé viene del lado de Caja.")},
                    ()=>{servicesPassReturned=true;Sound.Confirm();});
            return true;
        }

        void UpdateServices(bool playing)
        {
            if(!playing||Room!=ForFloor(1,true)||Time.time<nextRelayBuzz)return;
            float distance=Vector2.Distance(Player.Position,World.Key);
            if(distance>5)return;
            nextRelayBuzz=Time.time+4.2f;
            Sound.ServicesRelay(1f-distance/6f);
        }
    }
}
