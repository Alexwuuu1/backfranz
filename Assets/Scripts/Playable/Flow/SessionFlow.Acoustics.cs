using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (string Room,string Speaker,string Clue)[] AcousticClues=
        {
            ("Streaming","Lina","En la pista azul, tres ráfagas duraron un segundo. Solo una duró tres segundos. Busca esa en la primera fila."),
            ("Fotografía","Diego","En la pista ámbar, cada ráfaga dura el doble que la anterior. Busca la única secuencia que cumple la regla."),
            ("Sala Mac","Hugo","En la pista violeta, la ráfaga central dura lo mismo que las dos exteriores juntas.")
        };
        static readonly string[] AcousticLabels=
        {
            "1 · 2 · 1 · 1","1 · 1 · 1 · 1","1 · 3 · 1 · 1","2 · 2 · 1 · 1",
            "1 · 3 · 6","1 · 2 · 4","2 · 2 · 4","2 · 3 · 6",
            "1 · 2 · 2","2 · 2 · 1","1 · 4 · 2","1 · 3 · 2"
        };
        static readonly int[] AcousticRoute={2,1,3};
        const int AllAcousticClues=7;
        int acousticClues, acousticRow, lastAcousticPlate=-1;
        bool acousticArmed, acousticsApproved;
        readonly SpriteRenderer[] acousticPlates=new SpriteRenderer[12];

        bool InAcousticRoom()=>IsUpperInterior(Room)&&IsDark(Room)&&UpperInteriorName(Room)=="Sonido";
        Vector2 AcousticConsole()=>World.Cell(Room,8,4);
        Vector2 AcousticPlate(int index)=>World.Cell(Room,5+2*(index%4),5+2*(index/4));
        Vector2 AcousticWitness()=>World.Entry(Room)+new Vector2(2,1);

        int AcousticWitnessIndex()
        {
            if(!IsUpperInterior(Room))return -1;
            string room=UpperInteriorName(Room);
            for(int i=0;i<AcousticClues.Length;i++)if(AcousticClues[i].Room==room)return i;
            return -1;
        }

        void RefreshAcousticRoom()
        {
            if(InAcousticRoom()){UpdateAcousticPlates();return;}
            acousticArmed=false;
            acousticRow=0;
            lastAcousticPlate=-1;
        }

        void ShowAcousticContacts()
        {
            if(InAcousticRoom())
            {
                World.MarkEscapeContact(Room,AcousticConsole(),"Consola acústica","bloque-a-panel-horario");
                for(int i=0;i<acousticPlates.Length;i++)
                    acousticPlates[i]=World.MarkFloorPlate(Room,AcousticPlate(i),AcousticLabels[i]);
                UpdateAcousticPlates();
                return;
            }
            int witness=AcousticWitnessIndex();
            if(witness>=0)
                World.MarkEscapeContact(Room,AcousticWitness(),AcousticClues[witness].Speaker,"pb-estudiante");
        }

        bool TryAcousticInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(InAcousticRoom())
            {
                if(Near(AcousticConsole(),.75f))
                {
                    prompt=acousticsApproved?"Revisar · Canal acústico":"Armar recorrido acústico";
                    if(interact)
                    {
                        if(acousticsApproved)Notify("Canal acústico validado");
                        else if(acousticClues!=AllAcousticClues)Notify("Faltan las pistas de Streaming, Fotografía o Sala Mac",5);
                        else
                        {
                            acousticArmed=true;
                            acousticRow=0;
                            lastAcousticPlate=-1;
                            UpdateAcousticPlates();
                            Confirm("Pisa una placa por fila: azul, ámbar y violeta",5);
                        }
                    }
                    return true;
                }
                return false;
            }
            int index=AcousticWitnessIndex();
            if(index<0||!Near(AcousticWitness(),.88f))return false;
            var witness=AcousticClues[index];
            prompt="Hablar · "+witness.Speaker;
            if(interact)OpenDialogue(new[]{new DialogueLine(witness.Speaker,"pb-estudiante",witness.Clue)},
                ()=>acousticClues|=1<<index);
            return true;
        }

        public void ObserveAcousticMovement(Vector2 position)
        {
            if(!acousticArmed||!InAcousticRoom())return;
            int plate=-1;
            for(int index=0;index<acousticPlates.Length;index++)
                if((position-AcousticPlate(index)).sqrMagnitude<.19f){plate=index;break;}
            if(plate==lastAcousticPlate)return;
            lastAcousticPlate=plate;
            if(plate<0)return;
            if(plate!=4*acousticRow+AcousticRoute[acousticRow])
            {
                acousticArmed=false;
                acousticRow=0;
                UpdateAcousticPlates();
                Sound.Plate(false);
                Notify("Interferencia · vuelve a armar la consola",4);
                return;
            }
            acousticRow++;
            Sound.Plate(true);
            if(acousticRow==AcousticRoute.Length)
            {
                acousticsApproved=true;
                acousticArmed=false;
                Notify("Canal acústico validado · vuelve al cierre del portón",5);
            }
            else Notify("Fila "+acousticRow+"/3 · sigue con la siguiente");
            UpdateAcousticPlates();
        }

        void UpdateAcousticPlates()
        {
            for(int index=0;index<acousticPlates.Length;index++)
            {
                int row=index/4;
                bool correct=(acousticsApproved||acousticArmed&&row<acousticRow)&&index%4==AcousticRoute[row];
                var color=row==0?new Color(.29f,.58f,.95f):row==1?new Color(.96f,.65f,.29f):new Color(.71f,.42f,.89f);
                if(acousticPlates[index])acousticPlates[index].color=correct?new Color(.53f,1f,.65f):color;
            }
        }

        string AcousticObjective()
        {
            for(int i=0;i<AcousticClues.Length;i++)
                if(!HasEscapeFlag(acousticClues,i))
                    return ThoughtOnFloor(3,"Debería buscar una pista para el canal acústico en "+AcousticClues[i].Room);
            return ThoughtOnFloor(3,"Tengo que seguir las pistas para cruzar las placas de Sonido");
        }
    }
}
