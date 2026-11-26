using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        sealed class EscapeTask
        {
            public readonly int Floor, X, Y, ClueFloor, ClueX, ClueY, Answer;
            public readonly string Name, Operation, Witness, Clue, Completed;
            public readonly string[] Options;

            public EscapeTask(int floor,int x,int y,int clueFloor,int clueX,int clueY,
                string name,string operation,string witness,string clue,string completed,int answer,params string[] options)
            {
                Floor=floor;X=x;Y=y;ClueFloor=clueFloor;ClueX=clueX;ClueY=clueY;
                Name=name;Operation=operation;Witness=witness;Clue=clue;Completed=completed;Answer=answer;Options=options;
            }
        }

        static readonly EscapeTask[] EscapeTasks =
        {
            new EscapeTask(6,15,15,6,17,8,"Índice de evacuación","¿En qué sección se archivan los planos?",
                "Bibliotecaria","La sección se ordena por el último carácter. B7 termina en número. D-A termina en vocal. El plano termina en consonante.",
                "Encontraste un plano con la ruta de mantenimiento del Bloque A.",2,"B7","D-A","C-R"),
            new EscapeTask(2,12,15,4,5,15,"Puesto de primeros auxilios","Separa el frasco seguro de los contaminados.",
                "Profesora","La muestra intacta no lleva triángulo ni tapa oscura. El círculo con tapa blanca es el frasco que quedó sellado.",
                "El frasco contiene la llave del gabinete de comunicaciones.",1,"Triángulo blanco","Círculo blanco","Círculo negro"),
            new EscapeTask(3,14,7,5,26,15,"Grabadora de emergencias","Reordena las tres partes de la transmisión.",
                "Estudiante de audiovisual","La voz dijo: primero corta el zumbido. Después comprueba el micrófono. Solo entonces da el aviso.",
                "La grabación identifica el tablero de control de la salida.",0,"Cortar zumbido","Probar micrófono","Emitir aviso"),
            new EscapeTask(1,12,15,3,26,15,"Cuadro de relés","Elige el puente que alimenta la salida sin sobrecargar el relé.",
                "Técnico","El tablero pide exactamente ocho unidades. La línea Norte da tres, la línea Este da cinco y la línea Sur da seis.",
                "Volvió la alimentación de respaldo de la salida.",2,"Norte + Sur","Este + Sur","Norte + Este"),
            new EscapeTask(4,26,15,1,5,14,"Observación Gesell","Introduce la hora de cierre en formato HHMM.",
                "Delegada","Entramos a las 19:10. Pasaron once minutos hasta la observación y nueve más hasta cerrar el registro.",
                "Entre los relojes encontraste la tarjeta del archivo de talleres.",1,"19:21","19:24","19:30"),
            new EscapeTask(5,15,15,2,5,15,"Prensa del taller","Alinea las muescas con la plantilla de emergencia.",
                "Encargada de cocina","Lee las marcas desde el borde que tiene una flecha. Después del borde hay un hueco largo, uno corto y otro largo.",
                "La prensa liberó el mando mecánico del portón.",0,"Larga · corta · larga","Corta · larga · corta","Larga · larga · corta"),
            new EscapeTask(0,32,15,6,16,15,"Cierre del portón","El sistema pide desarmar los tres seguros en orden.",
                "Bibliotecaria","El esquema de evacuación marca una secuencia: primero comprueba que haya energía, luego libera el seguro y por último gira el mando.",
                "El portón exterior ya se puede abrir. Sigue el pasillo hacia la entrada.",2,"Liberar seguro","Girar mando","Comprobar energía")
        };

        static readonly (string Room,string Name,string Sprite,string Text)[] EscapeEncounters =
        {
            ("Café","Rita","pb-recepcionista","Hay siete tazas y nadie las pidió. Si sales, acuérdate de avisar que todavía hay gente aquí."),
            ("Game Room","Nico","pb-estudiante","Intenté abrir el portón con el mando de la consola. Obviamente no funcionó. Busca en el taller un mando de verdad."),
            ("Financiero","Elías","pb-guardia","Los relés de emergencia van por pares. No pruebes todas las líneas a la vez."),
            ("Servicios Estudiantiles","Mariela","pb-recepcionista","Varios bajamos juntos. Si encuentras a alguien que sepa algo, anótalo. El pasillo se parece demasiado a sí mismo."),
            ("Admisiones","Lucía","pb-estudiante","Tengo la lista de ingreso. Tu apellido está aquí, pero el mío se borró cuando apagaron el lector."),
            ("Posgrado","Samuel","pb-estudiante","El registro de esta noche está en hora local. Si aparece otro reloj, fíjate qué momento está de más."),
            ("Sala de Simulación","Irene","pb-recepcionista","Solo una muestra quedó sellada. La persona que guardó el material tuvo clase en el cuarto."),
            ("Laboratorio PC","David","pb-estudiante","El respaldo volvió un segundo. La radio logró grabar lo que alcanzaron a decir antes del corte."),
            ("Sala Mac","Belén","pb-estudiante","Oí al técnico contar la carga de cada línea. Parecía un ejercicio hasta que saltaron las luces."),
            ("Fotografía","Pablo","pb-estudiante","Si enfoco el pasillo por el visor, el número de pisos cambia. Prefiero confiar en el panel del ascensor."),
            ("Aula 4A","Clara","pb-recepcionista","Una observación se anotó al llegar, otra al empezar y otra al cerrar. El resto de las horas son ruido."),
            ("Taller","Valeria","pb-estudiante","La plantilla de la prensa se puede leer en ambos sentidos. La flecha pequeña indica desde dónde empezar."),
            ("Sala de lectura","Joel","pb-estudiante","Si encuentras el plano, vuelve a hablar con la bibliotecaria. Ella sabe cómo traducir los símbolos del portón."),
            ("Dirección de Carrera","Noelia","pb-recepcionista","Los indicadores de afuera no responden hasta que el regulador del subsuelo mantiene la presión."),
            ("Streaming","Omar","pb-estudiante","Probé el mismo selector dos veces y volví al estado anterior. Si quieres deshacer un cambio, ya sabes cómo."),
            ("Aula de grupos","Sofía","pb-estudiante","En el selector del extremo solo cambian dos lámparas. Los del centro afectan tres."),
            ("Baños","Mica","pb-estudiante","Escuché la sirena por las tuberías. Si consigues que suene otra vez, quizá los que quedan sepan hacia dónde ir.")
        };
        static readonly (int Floor,string Room,string Item,string Sprite)[] RelayParts =
        {
            (1,"Posgrado","Puente Norte","bloque-a-item-fusible"),
            (2,"Histología y Fisiología","Puente Este","bloque-a-item-llave-tecnica"),
            (3,"Laboratorio PC","Puente Sur","bloque-a-item-tablero-activo")
        };
        const int RelayPartsMask=(1<<3)-1;

        const int EscapeTaskMask=(1<<7)-1;
        int escapeSolved, escapeClues, escapeParts;
        readonly GameObject[] relayPartViews=new GameObject[RelayParts.Length];
        float playingSeconds, escapeSeconds;
        GameObject escapePanel;
        Text escapeTitle, escapeBody;
        readonly Button[] escapeOptions=new Button[3];
        GameObject escapeKeypad;
        Text keypadDisplay;
        string keypadEntry="";
        readonly System.Collections.Generic.HashSet<int> markedEscapeRooms=new System.Collections.Generic.HashSet<int>();
        int openEscapeTask=-1;
        int escapeSequenceStep;
        bool EscapePanelOpen=>escapePanel&&escapePanel.activeSelf;
        static string PlayDuration(float seconds)
        {
            int elapsed=Mathf.FloorToInt(seconds);
            return string.Format("{0:00}:{1:00}",elapsed/60,elapsed%60);
        }
        static bool HasEscapeFlag(int mask,int index)=>(mask&(1<<index))!=0;

        Vector2 EscapePoint(EscapeTask task,bool clue)
            =>World.Cell(Room,clue?task.ClueX:task.X,clue?task.ClueY:task.Y);

        void ShowEscapeContacts()
        {
            ShowEvacuees();
            ShowCrewPosts();
            ShowFinaleProps();
            ShowBlackoutReflection();
            if(!IsDark(Room)||!markedEscapeRooms.Add(Room))return;
            ShowInvestigationContacts();
            ShowRescueContacts();
            ShowMaintenanceContacts();
            ShowSignalContacts();
            ShowBlackoutContacts();
            ShowGalleryContacts();
            ShowAcousticContacts();
            ShowCrewContacts();
            if(IsInterior(Room)){
                if(TryEscapeEncounter(out var encounter))
                    World.MarkEscapeContact(Room,EscapeEncounterPoint(),encounter.Name,encounter.Sprite);
                int part=RelayPartIndex();
                if(part>=0&&!HasEscapeFlag(escapeParts,part))
                    relayPartViews[part]=World.MarkEscapeContact(Room,RelayPartPoint(),RelayParts[part].Item,RelayParts[part].Sprite);
                return;
            }
            if(Room<DarkGround||Room>DarkTop)return;
            int floor=Floor(Room);
            foreach(var task in EscapeTasks)
            {
                if(task.Floor==floor)
                {
                    if(floor==5)World.PlaceCollectible(Room,World.Cell(Room,15,16),.83f,task.Name,"bloque-a-panel-horario");
                    else if(floor==6)World.PlaceCollectible(Room,World.Cell(Room,15,17),.92f,task.Name,"bloque-a-panel-horario");
                    else World.MarkEscapeContact(Room,EscapePoint(task,false),task.Name,"bloque-a-panel-horario");
                }
                if(task.ClueFloor==floor)World.MarkEscapeContact(Room,EscapePoint(task,true),task.Witness,WitnessSprite(task));
            }
        }

        static string WitnessSprite(EscapeTask task)
            => task.Witness=="Técnico"?"pb-guardia":task.Witness.StartsWith("Estudiante")||task.Witness=="Delegada"?
                "pb-estudiante":"pb-recepcionista";

        Vector2 EscapeEncounterPoint()=>World.Entry(Room)+Vector2.up*2;
        Vector2 RelayPartPoint()=>World.Entry(Room)+Vector2.right*2+Vector2.up;

        int RelayPartIndex()
        {
            if(!IsUpperInterior(Room))return -1;
            string title=UpperInteriorName(Room);
            for(int i=0;i<RelayParts.Length;i++)if(RelayParts[i].Room==title)return i;
            return -1;
        }

        bool TryCollectRelayPart(bool interact,out string prompt)
        {
            prompt="";
            int part=RelayPartIndex();
            if(part<0||HasEscapeFlag(escapeParts,part)||!Near(RelayPartPoint(),.95f))return false;
            prompt="Recoger · "+RelayParts[part].Item;
            if(interact){
                escapeParts|=1<<part;
                if(relayPartViews[part])relayPartViews[part].SetActive(false);
                Sound.PickUp();
                Notify("Guardaste "+RelayParts[part].Item+" en la mochila");
            }
            return true;
        }

        bool TryEscapeEncounter(out (string Room,string Name,string Sprite,string Text) encounter)
        {
            string room=Title(Room);
            foreach(var person in EscapeEncounters)
                if(person.Room==room){encounter=person;return true;}
            encounter=default;
            return false;
        }

        bool TryEscapeInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Horror!=HorrorState.Active)return false;
            if(TryFinaleInteraction(interact,out prompt))return true;
            if(TryGalleryInteraction(interact,out prompt))return true;
            if(TryInvestigationInteraction(interact,out prompt))return true;
            if(TryRescueInteraction(interact,out prompt))return true;
            if(TryAuditoriumRescue(interact,out prompt))return true;
            if(TryEscortInteraction(interact,out prompt))return true;
            if(TryMaintenanceInteraction(interact,out prompt))return true;
            if(TrySignalInteraction(interact,out prompt))return true;
            if(TryServicesPass(interact,out prompt))return true;
            if(TryBlackoutInteraction(interact,out prompt))return true;
            if(TryAcousticInteraction(interact,out prompt))return true;
            if(TryCrewInteraction(interact,out prompt))return true;
            if(TryCollectRelayPart(interact,out prompt))return true;
            if(IsInterior(Room)&&TryEscapeEncounter(out var encounter)&&Near(EscapeEncounterPoint(),.93f))
            {
                prompt="Hablar · "+encounter.Name;
                if(interact)OpenDialogue(new[]{new DialogueLine(encounter.Name,encounter.Sprite,encounter.Text)});
                return true;
            }
            if(Room==DarkGround&&Near(World.Exit(Room),1.55f))
            {
                bool ready=escapeSolved==EscapeTaskMask&&EvacuationObjective()==null;
                prompt=ready?"Abrir el portón":"Revisar el portón de salida";
                if(interact){
                    if(ready)BeginFinale();
                    else Notify("Portón asegurado · "+EscapeObjective());
                }
                return true;
            }
            if(Room<DarkGround||Room>DarkTop)return false;
            int floor=Floor(Room);
            for(int i=EscapeTasks.Length-1;i>=0;i--)
            {
                var task=EscapeTasks[i];
                if(task.ClueFloor!=floor||!Near(EscapePoint(task,true),.92f))continue;
                if(i==6&&!HasEscapeFlag(escapeSolved,0))continue;
                prompt="Escuchar · "+task.Witness;
                if(interact)OpenEscapeClue(i);
                return true;
            }
            for(int i=0;i<EscapeTasks.Length;i++)
            {
                var task=EscapeTasks[i];
                if(task.Floor!=floor||!Near(EscapePoint(task,false),1.08f))continue;
                prompt="Revisar · "+task.Name;
                if(interact)OpenEscapeTask(i);
                return true;
            }
            return false;
        }

        void OpenEscapeClue(int index)
        {
            var task=EscapeTasks[index];
            OpenDialogue(new[]{new DialogueLine(task.Witness,WitnessSprite(task),task.Clue)},
                ()=>escapeClues|=1<<index);
        }

        int EscapeCount()
        {
            int count=0;
            for(int i=0;i<EscapeTasks.Length;i++)if(HasEscapeFlag(escapeSolved,i))count++;
            return count;
        }

        string EscapeObjective()
        {
            if(finaleStage==1)return ThoughtOnFloor(0,"Tengo que llegar a la palanca antes del portón");
            if(finaleStage==2)return ThoughtOnFloor(0,"Puedo salir por el portón, por el paso de servicio o por la puerta estrecha");
            for(int i=0;i<EscapeTasks.Length;i++)
                if(!HasEscapeFlag(escapeSolved,i)){
                    var task=EscapeTasks[i];
                    if(i==EscapeTasks.Length-1&&EvacuationObjective() is string pending)return pending;
                    if(i==3&&escapeParts!=RelayPartsMask)
                        for(int part=0;part<RelayParts.Length;part++)
                            if(!HasEscapeFlag(escapeParts,part))
                                return ThoughtOnFloor(RelayParts[part].Floor,
                                    "Me falta el "+RelayParts[part].Item.ToLowerInvariant()+". Debería buscarlo en "+RelayParts[part].Room);
                    if(HasEscapeFlag(escapeClues,i))
                        return ThoughtOnFloor(task.Floor,"Ya tengo la pista. Debería revisar «"+task.Name+"»");
                    string article=task.Witness=="Técnico"||task.Witness=="Estudiante de audiovisual"?"el ":"la ";
                    return ThoughtOnFloor(task.ClueFloor,"Tengo que hablar con "+article+task.Witness.ToLowerInvariant());
                }
            return EvacuationObjective()??ThoughtOnFloor(0,"Ya puedo abrir el portón exterior");
        }

        string EvacuationObjective()
        {
            if(!InvestigationComplete)return InvestigationObjective();
            if(!maintenanceApproved)return Room==Basement?MaintenanceThought():
                "Tengo que bajar al subsuelo (piso −1) en el ascensor y revisar el regulador.";
            if(!SignalsComplete)return SignalObjective();
            if(!blackoutRebuilt)return BlackoutObjective();
            if(!acousticsApproved)return AcousticObjective();
            if(!GallerySolved)return GalleryObjective();
            return crewAssigned?null:CrewObjective();
        }

        void CreateEscapePanel(Transform safe)
        {
            bool mobile=Application.isMobilePlatform;
            var panel=Panel(safe,"Mecanismo de escape",
                mobile?new Vector2(.035f,.035f):new Vector2(.17f,.12f),
                mobile?new Vector2(.965f,.965f):new Vector2(.83f,.88f));
            escapePanel=panel.gameObject;
            escapeTitle=Label(panel,"Mecanismo",new Vector2(.06f,.84f),new Vector2(.94f,.96f),32);
            escapeBody=Label(panel,"Pista y operación",new Vector2(.07f,.54f),new Vector2(.93f,.82f),mobile?32:25);
            for(int i=0;i<escapeOptions.Length;i++){
                int index=i;
                float y=mobile?.403f-i*.135f:.44f-i*.115f;
                escapeOptions[i]=Button(panel,"Opción "+(i+1),
                    new Vector2(mobile?.08f:.15f,y),
                    new Vector2(mobile?.92f:.85f,y+(mobile?.124f:.085f)),
                    ()=>ChooseEscapeOption(index),mobile?34:24);
            }
            Button(panel,"Cerrar",
                mobile?new Vector2(.28f,.02f):new Vector2(.35f,.055f),
                mobile?new Vector2(.72f,.118f):new Vector2(.65f,.143f),CloseEscapePanel,mobile?32:24);
            var keypad=Rect(panel,"Teclado Gesell",Vector2.zero,Vector2.one);
            escapeKeypad=keypad.gameObject;
            keypadDisplay=Label(keypad,"Hora ingresada",new Vector2(.19f,.43f),new Vector2(.81f,.53f),28);
            for(int digit=0;digit<=9;digit++)
            {
                int value=digit;
                float x=.15f+(digit%5)*.142f,y=.35f-(digit/5)*.085f;
                Button(keypad,digit.ToString(),new Vector2(x,y),new Vector2(x+.13f,y+.073f),
                    ()=>PressKeypad(value),25);
            }
            Button(keypad,"Borrar",new Vector2(.17f,.17f),new Vector2(.45f,.24f),()=>ResetKeypad(),21);
            Button(keypad,"Validar",new Vector2(.55f,.17f),new Vector2(.83f,.24f),SubmitKeypad,21);
            escapeKeypad.SetActive(false);
            BuildSignalControls(panel);
            BuildBlackoutControls(panel);
            escapePanel.SetActive(false);

            CreateEscapeJournal(safe);
        }

        void OpenEscapeTask(int index)
        {
            var task=EscapeTasks[index];
            if(HasEscapeFlag(escapeSolved,index)){Notify(task.Completed);return;}
            int required=(1<<index)-1;
            if((escapeSolved&required)!=required){Notify("Antes: "+EscapeObjective(),5);return;}
            if(index==EscapeTasks.Length-1&&!InvestigationComplete){Notify("Falta validar el permiso en biblioteca",5);return;}
            if(index==EscapeTasks.Length-1&&!maintenanceApproved){Notify("Falta calibrar el regulador del subsuelo",5);return;}
            if(index==EscapeTasks.Length-1&&!SignalsComplete){Notify("Sin red de señales · revisa el diario",5);return;}
            if(index==EscapeTasks.Length-1&&!blackoutRebuilt){Notify("Reconstruye el apagón en el archivo de cámaras de planta baja",5);return;}
            if(index==EscapeTasks.Length-1&&!acousticsApproved){Notify("Incompleto · valida el canal de Sonido",5);return;}
            if(index==EscapeTasks.Length-1&&!GallerySolved){Notify("Libera el cerrojo en la Galería de servicio",5);return;}
            if(index==EscapeTasks.Length-1&&!crewAssigned){Notify("Falta distribuir al equipo de evacuación",5);return;}
            if(index==3&&escapeParts!=RelayPartsMask){Notify("Faltan puentes · revisa el diario",5);return;}
            escapeTitle.text=task.Name;
            escapeBody.text=task.Operation+"\n\n"+(HasEscapeFlag(escapeClues,index)?task.Clue:"Pista pendiente · "+Floors[task.ClueFloor]);
            bool keypad=index==4;
            escapeKeypad.SetActive(keypad);
            ResetKeypad();
            for(int i=0;i<escapeOptions.Length;i++){
                escapeOptions[i].gameObject.SetActive(!keypad);
                escapeOptions[i].GetComponentInChildren<Text>().text=task.Options[i];
                escapeOptions[i].interactable=HasEscapeFlag(escapeClues,index);
            }
            openEscapeTask=index;
            escapeSequenceStep=0;
            ShowMechanismPanel(keypad?escapeKeypad.GetComponentInChildren<Button>().gameObject:
                escapeOptions[0].interactable?escapeOptions[0].gameObject:null);
        }

        void ShowMechanismPanel(GameObject firstControl)
        {
            Player.TouchAxis(Vector2.zero);
            pause.enabled=false;pause.pauseButton.SetActive(false);
            Time.timeScale=0;
            escapePanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(firstControl);
        }

        void ResetKeypad()
        {
            keypadEntry="";
            keypadDisplay.text="__ : __";
        }

        void PressKeypad(int digit)
        {
            if(openEscapeTask!=4||!HasEscapeFlag(escapeClues,4)||keypadEntry.Length>=4)return;
            keypadEntry+=digit.ToString();
            string preview=keypadEntry.PadRight(4,'_');
            keypadDisplay.text=preview.Insert(2," : ");
            Sound.Click();
        }

        void SubmitKeypad()
        {
            if(openEscapeTask!=4||keypadEntry.Length!=4)return;
            if(keypadEntry=="1930"){ResolveEscapeTask();return;}
            escapeBody.text="El registro no coincide. Calcula la hora de cierre.\n\n"+EscapeTasks[4].Clue;
            Sound.ScanResult(false);
            ResetKeypad();
        }

        void ChooseEscapeOption(int option)
        {
            if(openInduction>=1){ChooseInductionAnswer(option);return;}
            if(pendingCrewPost>=0){ChooseCrewMember(option);return;}
            if(openMaintenance){ChooseMaintenanceSetting(option);return;}
            if(openCase>=0){ChooseInvestigationAnswer(option);return;}
            if(openEscapeTask<0||!HasEscapeFlag(escapeClues,openEscapeTask))return;
            var task=EscapeTasks[openEscapeTask];
            bool sequence=openEscapeTask==2||openEscapeTask==6;
            int expected=sequence?(task.Answer+escapeSequenceStep)%escapeOptions.Length:task.Answer;
            if(option!=expected){
                escapeSequenceStep=0;
                escapeBody.text="El mecanismo rechazó el intento.\n\n"+task.Clue;
                Sound.ScanResult(false);
                return;
            }
            if(sequence&&++escapeSequenceStep<escapeOptions.Length){
                escapeBody.text="Pasos completados · "+escapeSequenceStep+"/"+escapeOptions.Length+"\n\n"+task.Clue;
                Sound.Click();
                return;
            }
            ResolveEscapeTask();
        }

        void ResolveEscapeTask()
        {
            var task=EscapeTasks[openEscapeTask];
            escapeSolved|=1<<openEscapeTask;
            CloseEscapePanel();
            Sound.MechanismReleased();
            Notify(task.Completed,5);
        }

        void CloseEscapePanel()
        {
            if(!EscapePanelOpen)return;
            openEscapeTask=-1;
            openInduction=-1;
            openCase=-1;
            openMaintenance=false;
            openSignal=-1;
            pendingCrewPost=-1;
            signalControls.SetActive(false);
            CloseBlackoutControls();
            escapePanel.SetActive(false);
            Time.timeScale=1;
            pause.enabled=true;pause.pauseButton.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
