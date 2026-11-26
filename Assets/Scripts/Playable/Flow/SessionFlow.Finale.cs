using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        int finaleStage;
        EscapeArchive.Ending endingSeen;
        SpriteRenderer finaleShadow;
        SpriteRenderer libraryShadow;
        float libraryShadowSeenAt=-1f;
        UnityEngine.UI.Image finaleDarkness;
        readonly System.Collections.Generic.HashSet<int> hallVisits=new System.Collections.Generic.HashSet<int>();
        readonly System.Collections.Generic.HashSet<int> hallAlterations=new System.Collections.Generic.HashSet<int>();
        Vector2 EmergencyLever()=>World.Cell(DarkGround,58,11);
        Vector2 RecordingExit()=>World.Cell(DarkGround,57,5);
        Vector2 NarrowExit()=>World.Cell(DarkGround,60,4);

        void BeginFinale()
        {
            finaleStage=1;
            if(blackoutReflection)blackoutReflection.SetActive(false);
            ShowFinaleProps();
            Sound.Door(true);
            OpenDialogue(new[]{
                new DialogueLine("Camila","pb-recepcionista","¡Alto! Se cortó la luz y el portón volvió a cerrarse. La palanca de emergencia está al otro lado del giro."),
                new DialogueLine("Iván","pb-estudiante","Todavía puedo sostener el paso. Alumbra las marcas del piso. Hay algo moviéndose detrás de ti.")
            });
        }

        void ShowFinaleProps()
        {
            if(Room!=DarkGround||finaleStage==0||finaleShadow)return;
            finaleShadow=World.MarkEscapeContact(Room,World.Cell(DarkGround,56,7),"Figura en el pasillo","shadow-00").GetComponent<SpriteRenderer>();
            World.MarkEscapeContact(Room,EmergencyLever(),"Palanca de emergencia","bloque-a-item-tablero-activo");
            World.MarkEscapeContact(Room,RecordingExit(),"Grabación final","bloque-a-item-tablero-apagado");
            World.MarkEscapeContact(Room,NarrowExit(),"Salida estrecha","tour-puerta-vidrio");
        }

        void AlterReturningHall()
        {
            if(Room<DarkGround||Room>DarkTop||Room==DarkGround)return;
            if(hallVisits.Add(Room)||!hallAlterations.Add(Room))return;
            if(Room==ForFloor(3,true))
            {
                World.MarkEscapeContact(Room,World.Cell(Room,20,11),"Puerta que no estaba","tour-puerta-vidrio");
                Sound.DistantKnock();
                Notify("Esa puerta no estaba antes.",4);
            }
            else if(Room==DarkTop)
            {
                libraryShadow=World.MarkEscapeContact(Room,World.Cell(Room,22,6),
                    "Figura entre los estantes","shadow-02").GetComponent<SpriteRenderer>();
                libraryShadow.color=new Color(.57f,.69f,.72f,.73f);
            }
        }

        void UpdateLibraryShadow(bool playing)
        {
            if(!playing||Room!=DarkTop||!libraryShadow||!libraryShadow.enabled)return;
            Vector2 feet=World.Cell(DarkTop,22,6);
            if(libraryShadowSeenAt<0f)
            {
                if(Vector2.Distance(Player.Position,feet)>5f||!World.HasLineOfSight(Player.Position,feet,false))return;
                libraryShadowSeenAt=Time.time;
                Sound.DistantKnock();
                Notify("La radio se corta. Hay alguien entre los estantes.",3.5f);
            }
            float elapsed=Time.time-libraryShadowSeenAt;
            libraryShadow.sprite=CampusLook.Sprite("shadow-"+Mathf.Min((int)(elapsed*5),3).ToString("00"));
            libraryShadow.color=new Color(.57f,.69f,.72f,.73f*Mathf.Clamp01(2.4f-elapsed));
            if(elapsed>=2.4f)libraryShadow.enabled=false;
        }

        void UpdateFinale()
        {
            bool active=Room==DarkGround&&finaleStage>0&&!Finished;
            finaleDarkness.gameObject.SetActive(active);
            if(!active||!finaleShadow)return;
            if(!Crossing&&!DialogueOpen&&Time.timeScale>0)
            {
                var origin=World.Cell(DarkGround,56,7);
                finaleShadow.transform.position=origin+new Vector2(Mathf.Sin(Time.time*.49f)*.75f,
                    Mathf.Sin(Time.time*.64f)*1.65f);
                finaleShadow.sprite=CampusLook.Sprite("shadow-"+((int)(Time.time*5)%4).ToString("00"));
            }
        }

        bool TryFinaleInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Room!=DarkGround||finaleStage==0)return false;
            if(finaleStage==1&&Near(EmergencyLever(),.88f))
            {
                prompt="Bajar · Palanca de emergencia";
                if(interact)
                {
                    finaleStage=2;
                    Sound.Latch();
                    OpenDialogue(new[]{
                        new DialogueLine("Paula","pb-recepcionista","El portón está libre. Si cruzas por aquí, salimos contigo. La puerta estrecha solo deja pasar a una persona."),
                        new DialogueLine("Camila","pb-recepcionista","La radio sigue recibiendo una voz. Si quieres llevarte la grabación, recógela en el puesto del giro. Saldrás por el paso de servicio.")
                    });
                }
                return true;
            }
            if(finaleStage==2&&Near(RecordingExit(),.78f))
                return FinalChoice(interact,EscapeArchive.Ending.Eco,"Llevar la grabación · Paso de servicio",out prompt);
            if(finaleStage==2&&Near(NarrowExit(),.80f))
                return FinalChoice(interact,EscapeArchive.Ending.Umbral,"Salir solo · Puerta estrecha",out prompt);
            if(Near(World.Exit(Room),1.34f))
            {
                if(finaleStage==2)
                    return FinalChoice(interact,EscapeArchive.Ending.Amanecer,"Salir con el equipo · Portón principal",out prompt);
                prompt="Portón sin energía";
                if(interact)Notify("Usa la palanca de emergencia al otro lado del giro");
                return true;
            }
            return false;
        }

        bool FinalChoice(bool interact,EscapeArchive.Ending ending,string action,out string prompt)
        {
            prompt=action;
            if(interact)
            {
                string question=ending switch
                {
                    EscapeArchive.Ending.Amanecer=>"Paula espera junto al portón. Si cruzo por aquí, saldrá el grupo que reunimos.",
                    EscapeArchive.Ending.Eco=>"Puedo llevar la grabación por el paso de servicio. Los demás tendrán que esperar junto al portón.",
                    _=>"Solo hay espacio para una persona. Si cruzo, el grupo se queda atrás."
                };
                OpenDialogue(new[]{new DialogueLine("Yamil","yamil-00",question)},()=>{
                    endingSeen=ending;
                    EscapeArchive.Unlock(ending);
                    Finish(true);
                },true,"Cruzar");
            }
            return true;
        }

        string RescueEpilogue()
        {
            int rescued=RescuedCount();
            if(endingSeen==EscapeArchive.Ending.Amanecer)
            {
                if(rescued==RescueTotal)
                    return "La lista está completa. Elena sostiene la puerta desde la acera mientras salen los últimos.";
                if(rescued==0)
                    return "Solo sale el equipo de evacuación. Paula deja la lista abierta por si alguien más llega a la calle.";
                return "Salen quienes llegaron al punto de encuentro. Paula guarda los nombres de quienes todavía faltan.";
            }
            if(endingSeen==EscapeArchive.Ending.Eco)
            {
                if(HasEscapeFlag(escortedPeople,1))
                    return "En la cinta, Marcos repite por radio el nombre de quienes seguían en el pasillo. Yamil conserva esa parte.";
                return rescued>0
                    ?"La última transmisión deja constancia de quienes llegaron al punto de encuentro."
                    :"En la última transmisión, Paula llama a los pisos. Solo se escucha el zumbido del edificio.";
            }
            return rescued>0
                ?"Del otro lado, el grupo que se había reunido sigue golpeando el portón. Yamil se queda en la acera escuchando."
                :"La radio del equipo sigue encendida. Yamil baja el volumen antes de alejarse.";
        }
    }
}
