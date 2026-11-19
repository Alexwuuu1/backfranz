using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        // Each selector changes its own lamp and its immediate neighbours.
        static readonly int[] SignalSelectors={3,7,14,28,24};
        static readonly (int Floor,int X,int Y,int Initial,string Name)[] SignalStations=
        {
            (2,17,7,10,"Sirena de laboratorios"),
            (4,17,7,9,"Baliza de aulas"),
            (5,17,7,13,"Repetidor de talleres")
        };
        const int AllSignals=7, SignalTarget=31;
        int signalSolved;
        int[] signalLamps=System.Array.ConvertAll(SignalStations,station=>station.Initial);
        int openSignal=-1;
        readonly SpriteRenderer[] signalViews=new SpriteRenderer[SignalStations.Length];
        UnityEngine.UI.Button[] signalButtons;
        UnityEngine.UI.Image[] signalIndicators;
        GameObject signalControls;
        bool SignalsComplete=>signalSolved==AllSignals;

        void BuildSignalControls(Transform panel)
        {
            var controls=Rect(panel,"Selectores de señal",Vector2.zero,Vector2.one);
            signalControls=controls.gameObject;
            signalButtons=new UnityEngine.UI.Button[SignalSelectors.Length];
            signalIndicators=new UnityEngine.UI.Image[SignalSelectors.Length];
            for(int i=0;i<signalButtons.Length;i++)
            {
                int index=i;float x=.10f+i*.16f;
                signalButtons[i]=Button(controls,(i+1).ToString(),new Vector2(x,.31f),new Vector2(x+.144f,.43f),
                    ()=>ToggleSignal(index),24);
                var lamp=Rect(controls,"Lámpara "+(i+1),new Vector2(x+.049f,.475f),new Vector2(x+.095f,.529f));
                signalIndicators[i]=lamp.gameObject.AddComponent<UnityEngine.UI.Image>();
                signalIndicators[i].raycastTarget=false;
            }
            Button(controls,"Restablecer",new Vector2(.31f,.17f),new Vector2(.69f,.27f),ResetSignal,22);
            signalControls.SetActive(false);
        }

        void ShowSignalContacts()
        {
            if(Room<DarkGround||Room>DarkTop)return;
            for(int index=0;index<SignalStations.Length;index++)
            {
                var station=SignalStations[index];
                if(Floor(Room)!=station.Floor)continue;
                signalViews[index]=World.MarkEscapeContact(Room,World.Cell(Room,station.X,station.Y),station.Name,
                    "bloque-a-item-tablero-activo").GetComponent<SpriteRenderer>();
                UpdateSignalView(index);
            }
        }

        void UpdateSignalView(int index)
        {
            if(signalViews[index])signalViews[index].color=
                HasEscapeFlag(signalSolved,index)?new Color(.66f,1f,.73f):new Color(.92f,.62f,.48f);
        }

        bool TrySignalInteraction(bool interact,out string prompt)
        {
            prompt="";
            if(Room<DarkGround||Room>DarkTop)return false;
            for(int index=0;index<SignalStations.Length;index++)
            {
                var station=SignalStations[index];
                if(Floor(Room)!=station.Floor||!Near(World.Cell(Room,station.X,station.Y),.90f))continue;
                prompt="Revisar · "+station.Name;
                if(interact)
                {
                    if(!maintenanceApproved)Notify("Sin presión. Primero calibra el regulador del subsuelo.");
                    else if(HasEscapeFlag(signalSolved,index))Notify("Señal en línea");
                    else OpenSignalStation(index);
                }
                return true;
            }
            return false;
        }

        string SignalObjective()
        {
            for(int index=0;index<SignalStations.Length;index++)
                if(!HasEscapeFlag(signalSolved,index))
                    return Floors[SignalStations[index].Floor]+" · "+SignalStations[index].Name;
            return "PB · abre el portón exterior";
        }

        void OpenSignalStation(int index)
        {
            openSignal=index;
            escapeTitle.text=SignalStations[index].Name;
            foreach(var button in escapeOptions)button.gameObject.SetActive(false);
            escapeKeypad.SetActive(false);
            signalControls.SetActive(true);
            RefreshSignalLamps();
            ShowMechanismPanel(signalButtons[0].gameObject);
        }

        void RefreshSignalLamps()
        {
            escapeBody.text="Enciende las cinco lámparas. Cada selector invierte su lámpara y las contiguas.";
            for(int index=0;index<signalIndicators.Length;index++)
            {
                bool lit=HasEscapeFlag(signalLamps[openSignal],index);
                signalIndicators[index].color=lit?new Color(.99f,.77f,.36f):new Color(.29f,.38f,.41f);
                signalButtons[index].GetComponentInChildren<UnityEngine.UI.Text>().text=
                    (index+1)+(lit?" · Luz":" · Apagada");
            }
        }

        void ResetSignal()
        {
            if(openSignal<0)return;
            signalLamps[openSignal]=SignalStations[openSignal].Initial;
            RefreshSignalLamps();
        }

        void ToggleSignal(int selector)
        {
            if(openSignal<0)return;
            signalLamps[openSignal]^=SignalSelectors[selector];
            if(signalLamps[openSignal]==SignalTarget)
            {
                signalSolved|=1<<openSignal;
                UpdateSignalView(openSignal);
                CloseEscapePanel();
                Confirm(SignalsComplete?"Red de evacuación en línea":"Estación sincronizada",5);
                return;
            }
            Sound.ToggleSwitch();
            RefreshSignalLamps();
        }
    }
}
