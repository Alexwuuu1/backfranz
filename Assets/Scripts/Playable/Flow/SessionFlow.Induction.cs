using UnityEngine;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        sealed class InductionActivity
        {
            public readonly string Source, Destination, Guide, Portrait, Brief, Station, Question;
            public readonly int Answer;
            public readonly string[] Options;

            public InductionActivity(string source,string destination,string guide,string portrait,string brief,
                string station,string question,int answer,params string[] options)
            {
                Source=source; Destination=destination; Guide=guide; Portrait=portrait; Brief=brief;
                Station=station; Question=question; Answer=answer; Options=options;
            }
        }

        static readonly InductionActivity[] Induction =
        {
            new InductionActivity("Posgrado","Caja","Irene","pb-recepcionista",
                "Soy Irene. Este sobre es para Caja. Si la ficha dice «solo consulta», va a la ventanilla azul. La roja recibe pagos y la amarilla entrega recibos. Tu ficha dice «solo consulta».",
                "Ventanilla de turnos","¿A qué ventanilla le corresponde una consulta de saldo?",1,
                "Ventanilla roja","Ventanilla azul","Ventanilla amarilla"),
            new InductionActivity("Cocina de prácticas","Histología y Fisiología","Andrés","pb-estudiante",
                "Me encargo de las bandejas de práctica. Las limpias llevan un círculo, las que esperan revisión un triángulo. La bandeja de hoy tiene un triángulo. En Histología puedes anotar su estado.",
                "Registro de bandejas","¿Cómo registras la bandeja de práctica?",2,
                "Lista para usar","Retirada","Pendiente de revisión"),
            new InductionActivity("Streaming","Sonido","Lina","pb-estudiante",
                "Acabamos de grabar una prueba. En Sonido verás el pico más alto en ámbar. Si se ilumina ámbar, baja un punto. Rojo exige bajar dos y verde deja el nivel como está.",
                "Prueba de micrófono","El pico está en ámbar. Ajusta la entrada antes de grabar.",0,
                "Bajar un punto","Bajar dos puntos","Dejar el nivel"),
            new InductionActivity("Cámara Gesell","Aula 4A","Celia","pb-estudiante",
                "Para la práctica de esta tarde el horario dice grupo B. El grupo A empieza en la mesa del fondo, el B en la mesa de la ventana y el C junto a la puerta.",
                "Lista de práctica","¿Dónde se reúne el grupo que figura en tu horario?",1,
                "Mesa del fondo","Mesa de la ventana","Mesa de la puerta"),
            new InductionActivity("Aula de grupos","Taller","Valeria","pb-estudiante",
                "Pedimos una llave de doce milímetros. El casillero 1 guarda las de diez y el 2 tiene las de once. La que necesitamos quedó en el siguiente.",
                "Casillero de herramientas","Prepara la llave pedida por el grupo.",2,
                "Casillero 1","Casillero 2","Casillero 3"),
            new InductionActivity("Sala de lectura",null,"Alma","pb-recepcionista",
                "Cuando termines el recorrido, lleva la ficha al mostrador del pasillo. Para acreditar la inducción, elige «Registro de primer ingreso». Las otras opciones son para préstamos.",
                "Mostrador de biblioteca","Elige el trámite de tu primer día.",0,
                "Registro de primer ingreso","Renovar préstamo","Reservar un libro")
        };

        const int InductionBriefMask=126;
        readonly System.Collections.Generic.HashSet<int> inductionRooms=new System.Collections.Generic.HashSet<int>();
        int inductionBriefs;
        int openInduction=-1;
        readonly SpriteRenderer[] inductionStationViews=new SpriteRenderer[Induction.Length];

        void ShowInductionContacts()
        {
            if(IsDark(Room)||!IsUpperInterior(Room)||!inductionRooms.Add(Room))return;
            int floor=Floor(Room);
            if(floor<1||floor>Induction.Length)return;
            var activity=Induction[floor-1];
            string area=Title(Room);
            if(area==activity.Source)
                World.MarkEscapeContact(Room,InductionPoint(),activity.Guide,activity.Portrait)
                    .GetComponent<SpriteRenderer>().color=Color.white;
            else if(area==activity.Destination)
            {
                var station=floor switch
                {
                    2=>World.PlaceCollectible(Room,World.Cell(Room,8,8),.51f,activity.Station,"bloque-a-panel-horario").GetComponent<SpriteRenderer>(),
                    3=>World.PlaceCollectible(Room,World.Cell(Room,6,8),.78f,activity.Station,"bloque-a-panel-horario").GetComponent<SpriteRenderer>(),
                    4=>World.PlaceCollectible(Room,World.Cell(Room,5,7),.68f,activity.Station,"bloque-a-panel-horario").GetComponent<SpriteRenderer>(),
                    _=>World.MarkInductionPosition(Room,InductionPoint(),activity.Station)
                };
                inductionStationViews[floor-1]=station;
                if(visits[floor])station.color=new Color(.55f,1f,.77f);
            }
        }

        Vector2 InductionPoint()=>Title(Room) switch
        {
            "Histología y Fisiología"=>World.Cell(Room,8,7),
            "Sonido"=>World.Cell(Room,6,7),
            "Aula 4A"=>World.Cell(Room,5,6),
            _=>World.Entry(Room)+Vector2.up*3
        };

        string InductionDirection(int floor)
        {
            var activity=Induction[floor-1];
            return HasEscapeFlag(inductionBriefs,floor)
                ?activity.Destination!=null?"Completa la actividad en "+activity.Destination+".":"Entrega tu ficha en el mostrador de la biblioteca."
                :"Habla con "+activity.Guide+" en "+activity.Source+".";
        }

        string InductionObjective()
        {
            if(!HasSchedule)return "Debería ir directo a Recepción. La ventanilla está junto al vidrio azul.";
            if(Room==Ground)
            {
                bool accessOpen=false;
                for(int lane=0;lane<AccessLaneCount;lane++)accessOpen|=World.SecurityOpen(lane);
                if(!accessOpen)return "Ya tengo mi credencial. Puedo escanear mi huella o pedirle al guardia que me abra.";
            }
            if(InductionComplete)
            {
                if(IsLiftCabin(Room)&&liftOrigin==Top)return "Ya terminé la inducción. Puedo elegir un piso más abajo en el panel.";
                if(Room==Top||Floor(Room)==6)return "Ya terminé la inducción. Debería bajar desde la biblioteca en el ascensor.";
                return "Debería volver a la biblioteca del piso 6 y bajar en el ascensor.";
            }
            int floor=IsLiftCabin(Room)?0:Floor(Room);
            if(floor>=1&&floor<=Induction.Length&&!visits[floor])return InductionThought(floor);
            for(int next=1;next<=Induction.Length;next++)
                if(!visits[next])return InductionThought(next);
            return "Debería volver a la biblioteca del piso 6.";
        }

        string InductionThought(int floor)
        {
            var activity=Induction[floor-1];
            if(!HasEscapeFlag(inductionBriefs,floor))
                return ThoughtOnFloor(floor,"Tengo que hablar con "+activity.Guide+" en "+activity.Source);
            if(activity.Destination!=null)
                return ThoughtOnFloor(floor,"Ahora tengo que completar la actividad en "+activity.Destination);
            return ThoughtOnFloor(floor,"Tengo que entregar mi ficha en el mostrador de la biblioteca");
        }

        bool TryInductionInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Room<Ground||IsDark(Room)||IsLiftCabin(Room)||IsStairwell(Room))return false;
            int floor=Floor(Room);
            if(floor<1||floor>Induction.Length)return false;
            var activity=Induction[floor-1];
            if(IsUpperInterior(Room)&&Title(Room)==activity.Source&&Near(InductionPoint(),.88f))
            {
                prompt="Hablar · "+activity.Guide;
                if(interact)
                    OpenDialogue(new[]{new DialogueLine(activity.Guide,activity.Portrait,activity.Brief)},
                        ()=>inductionBriefs|=1<<floor);
                return true;
            }
            bool atStation=activity.Destination==null
                ?Room==Top&&Near(World.MainStop(Top),1.05f)
                :IsUpperInterior(Room)&&Title(Room)==activity.Destination&&Near(InductionPoint(),.88f);
            if(atStation)
            {
                prompt="Revisar · "+activity.Station;
                if(interact)ReviewInduction(floor,activity);
                return true;
            }
            if(IsInterior(Room)||!Near(World.MainStop(Room)))return false;
            prompt="Consultar · Inducción";
            if(interact)Notify(visits[floor]?"Constancia registrada":InductionDirection(floor),4);
            return true;
        }

        void ReviewInduction(int floor,InductionActivity activity)
        {
            if(visits[floor])
            {
                Notify("Constancia registrada · "+Floors[floor]);
                return;
            }
            if(!HasSchedule||!HasEscapeFlag(inductionBriefs,floor))
            {
                Notify(InductionObjective(),4);
                return;
            }

            openInduction=floor;
            escapeTitle.text=activity.Station;
            escapeBody.text=activity.Question+"\n\n"+activity.Brief;
            escapeKeypad.SetActive(false);
            for(int i=0;i<escapeOptions.Length;i++)
            {
                var button=escapeOptions[i];
                button.gameObject.SetActive(true);
                button.interactable=true;
                button.GetComponentInChildren<Text>().text=activity.Options[i];
            }
            ShowMechanismPanel(escapeOptions[0].gameObject);
        }

        void ChooseInductionAnswer(int option)
        {
            int floor=openInduction;
            var activity=Induction[floor-1];
            if(option!=activity.Answer)
            {
                Sound.ScanResult(false);
                escapeTitle.text=activity.Station+" · Revisa la indicación";
                return;
            }
            visits[floor]=true;
            Visited++;
            if(inductionStationViews[floor-1])
                inductionStationViews[floor-1].color=new Color(.55f,1f,.77f);
            CloseEscapePanel();
            Confirm(InductionComplete?"Inducción completa · baja por el ascensor desde biblioteca":
                "Constancia registrada · "+Visited+"/7",5);
        }
    }
}
