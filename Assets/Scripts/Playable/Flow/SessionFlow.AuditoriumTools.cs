using System;
using System.Collections.Generic;
using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        [Flags]
        enum AuditoriumTool { None=0, Recorder=1, Wedge=2 }

        const AuditoriumTool AllAuditoriumTools=AuditoriumTool.Recorder|AuditoriumTool.Wedge;
        const string RecorderName="Grabadora de prácticas", RecorderSprite="utileria-grabadora";
        const string WedgeName="Cuña de goma", WedgeSprite="utileria-cuna-goma";

        AuditoriumTool auditoriumTools;
        readonly Dictionary<int,GameObject> auditoriumToolViews=new Dictionary<int,GameObject>();

        bool HasAuditoriumTool(AuditoriumTool tool)=>(auditoriumTools&tool)!=0;

        AuditoriumTool ToolInRoom()
        {
            if(Room==ForFloor(3,false)||Room==ForFloor(3,true))return AuditoriumTool.Recorder;
            if(IsUpperInterior(Room)&&UpperInteriorName(Room)=="Taller")return AuditoriumTool.Wedge;
            return AuditoriumTool.None;
        }

        Vector2 ToolContact(AuditoriumTool tool)=>tool==AuditoriumTool.Recorder
            ?World.Cell(Room,25,7):World.Cell(Room,5,8);

        void ShowAuditoriumTool()
        {
            var tool=ToolInRoom();
            if(tool==AuditoriumTool.None)return;
            if(auditoriumToolViews.TryGetValue(Room,out var view))
            {
                view.SetActive(!HasAuditoriumTool(tool));
                return;
            }
            if(HasAuditoriumTool(tool))return;
            bool recorder=tool==AuditoriumTool.Recorder;
            // Both items sit on existing or dedicated furniture beside their interaction point.
            var support=recorder?World.Cell(Room,25,6):World.Cell(Room,5,9);
            auditoriumToolViews.Add(Room,World.PlaceCollectible(Room,support,recorder?.49f:.72f,
                recorder?RecorderName:WedgeName,recorder?RecorderSprite:WedgeSprite));
        }

        bool TryAuditoriumTool(bool collect,out string prompt)
        {
            prompt="";
            var tool=ToolInRoom();
            if(tool==AuditoriumTool.None||HasAuditoriumTool(tool)||!Near(ToolContact(tool),.78f))return false;
            string name=tool==AuditoriumTool.Recorder?RecorderName:WedgeName;
            prompt="Recoger · "+name;
            if(collect)
            {
                auditoriumTools|=tool;
                auditoriumToolViews[Room].SetActive(false);
                Sound.PickUp();
                Notify("Recogiste "+name.ToLowerInvariant()+".",3);
            }
            return true;
        }
    }
}
