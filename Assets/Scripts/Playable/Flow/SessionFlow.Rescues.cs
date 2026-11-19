using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (string Room,string Person,string Source,string Item,string Sprite,string Request,string Thanks)[] Rescues =
        {
            ("Café","Dalia","Cocina de prácticas","Botella sellada","bloque-a-item-fusible",
                "El grifo sale oscuro. En la cocina había agua sellada para las prácticas. ¿Puedes traer una botella?",
                "Gracias. Ya puedo cruzar el pasillo sin detenerme. Avisaré a los del Game Room."),
            ("Fotografía","Lázaro","Taller","Linterna de trabajo","bloque-a-item-llave-tecnica",
                "Perdí mi luz en el laboratorio. En el Taller dejaron una linterna para revisar las máquinas.",
                "Ahora veo el camino al ascensor. Si encuentras una salida, voy contigo."),
            ("Auditorio","Nuria","Sala de Simulación","Botiquín portátil","bloque-a-item-tablero-activo",
                "Me torcí el pie al bajar las gradas. En la sala de simulación tienen un botiquín portátil.",
                "Con esto puedo apoyar el pie. Me reuniré con los demás junto al portón.")
        };

        const int AllRescues=(1<<3)-1;
        static int RescueTotal=>Rescues.Length+EscortPeople.Length;
        int rescueRequests,rescueItems,rescuedPeople;
        readonly GameObject[] rescueItemViews=new GameObject[Rescues.Length];
        readonly GameObject[] rescuePeopleViews=new GameObject[Rescues.Length];
        int evacueesShown;

        Vector2 RescueContact()=>World.Entry(Room)+(Title(Room)=="Auditorio"?new Vector2(-1,1):new Vector2(-2,2));
        Vector2 RescueItemPoint()=>World.Entry(Room)+new Vector2(2,3);
        Vector2 EvacueePoint(int index)=>World.Cell(DarkGround,56,5+2*index);

        void ShowEvacuees()
        {
            if(Room!=DarkGround)return;
            for(int index=0;index<Rescues.Length;index++)
            {
                if(!HasEscapeFlag(rescuedPeople,index)||HasEscapeFlag(evacueesShown,index))continue;
                World.MarkEscapeContact(Room,EvacueePoint(index),Rescues[index].Person,"pb-estudiante");
                evacueesShown|=1<<index;
            }
        }

        void ShowRescueContacts()
        {
            if(!IsInterior(Room))return;
            string room=Title(Room);
            for(int index=0;index<Rescues.Length;index++)
            {
                var rescue=Rescues[index];
                if(rescue.Room==room&&!HasEscapeFlag(rescuedPeople,index))
                    rescuePeopleViews[index]=World.MarkEscapeContact(Room,RescueContact(),rescue.Person,"pb-estudiante");
                if(rescue.Source==room&&!HasEscapeFlag(rescueItems,index))
                    rescueItemViews[index]=World.MarkEscapeContact(Room,RescueItemPoint(),rescue.Item,rescue.Sprite);
            }
        }

        bool TryRescueInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Room==DarkGround)
            {
                for(int index=0;index<Rescues.Length;index++)
                {
                    if(!HasEscapeFlag(rescuedPeople,index)||!Near(EvacueePoint(index),.9f))continue;
                    var evacuee=Rescues[index];
                    prompt="Hablar · "+evacuee.Person;
                    if(interact)OpenDialogue(new[]{new DialogueLine(evacuee.Person,"pb-estudiante",
                        "Te espero aquí junto a la salida. Cuando se abra el portón, salimos.")});
                    return true;
                }
            }
            if(!IsInterior(Room))return false;
            string room=Title(Room);
            for(int i=0;i<Rescues.Length;i++)
            {
                int index=i;
                var rescue=Rescues[index];
                if(rescue.Room==room&&!HasEscapeFlag(rescuedPeople,index)&&Near(RescueContact(),.9f))
                {
                    prompt="Hablar · "+rescue.Person;
                    if(interact)
                    {
                        bool ready=HasEscapeFlag(rescueItems,index);
                        OpenDialogue(new[]{new DialogueLine(rescue.Person,"pb-estudiante",ready?rescue.Thanks:rescue.Request)},()=>{
                            rescueRequests|=1<<index;
                            if(ready){
                                rescuedPeople|=1<<index;
                                if(rescuePeopleViews[index])rescuePeopleViews[index].SetActive(false);
                                Sound.Gathered();
                            }
                        });
                    }
                    return true;
                }
                if(rescue.Source==room&&!HasEscapeFlag(rescueItems,index)&&Near(RescueItemPoint(),.9f))
                {
                    prompt="Recoger · "+rescue.Item;
                    if(interact)
                    {
                        rescueItems|=1<<index;
                        if(rescueItemViews[index])rescueItemViews[index].SetActive(false);
                        Sound.PickUp();
                        Notify("Recogiste "+rescue.Item);
                    }
                    return true;
                }
            }
            return false;
        }

        string SupplyRescueNotes()
        {
            var lines=new System.Text.StringBuilder("ENTREGAS");
            for(int index=0;index<Rescues.Length;index++)
            {
                if(!HasEscapeFlag(rescueRequests,index)&&!HasEscapeFlag(rescueItems,index))continue;
                var rescue=Rescues[index];
                lines.Append("\n\n").Append(rescue.Person).Append(" · ").Append(rescue.Room).AppendLine();
                if(HasEscapeFlag(rescuedPeople,index))lines.Append("Con el equipo en planta baja.");
                else if(HasEscapeFlag(rescueItems,index))lines.Append("Entrega pendiente · ").Append(rescue.Item);
                else lines.Append("Busca ").Append(rescue.Item).Append(" en ").Append(rescue.Source).Append('.');
            }
            if((rescueRequests|rescueItems)==0)lines.Append("\n\nHabla con quienes necesiten suministros.");
            return lines.ToString();
        }

        int RescuedCount()
        {
            int count=0;
            for(int index=0;index<Rescues.Length;index++)if(HasEscapeFlag(rescuedPeople,index))count++;
            for(int index=0;index<EscortPeople.Length;index++)if(HasEscapeFlag(escortedPeople,index))count++;
            return count;
        }
    }
}
