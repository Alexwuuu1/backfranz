using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
        SpriteRenderer unattendedReader;
        internal Vector2 DarkReaderPosition => Cell(DarkGround,39,15);
        internal bool InDarkServiceCorridor(Vector2 point)
        {
            var local=point-Cell(DarkGround,0,0);
            return local.x>=34.5f&&local.x<=39.5f&&local.y>=9&&local.y<=11.25f;
        }

        void PlaceUnattendedReader()
        {
            var position=DarkReaderPosition+Vector2.up*.35f;
            unattendedReader=Art(DarkGround,"solid",position,new Vector2(.24f,.13f),
                "Lector sin operador",CampusLook.Depth(DarkReaderPosition.y)+4).GetComponent<SpriteRenderer>();
            ShowUnattendedReader(false);
        }

        internal void ShowUnattendedReader(bool active)
        {
            unattendedReader.color=active?new Color(.96f,.18f,.22f):new Color(.21f,.32f,.35f);
        }

        static void AddDarkHallWalls(HashSet<Vector2Int> walls,int floor)
        {
            void AlongX(int y,int first,int last)
            {
                for(int x=first;x<=last;x++)walls.Add(new Vector2Int(x,y));
            }
            void AlongY(int x,int first,int last)
            {
                for(int y=first;y<=last;y++)walls.Add(new Vector2Int(x,y));
            }
            switch(floor){
                case 1: AlongY(18,12,18);break;
                case 2: AlongY(20,13,18);break;
                case 3: AlongX(12,16,18);break;
                case 4: AlongY(20,7,10);break;
                case 5: AlongX(11,24,30);break;
                case 6: AlongX(10,26,28);break;
            }
        }

        static bool DarkHallAisle(int floor,int x,int y)=>floor switch
        {
            1 => y>=8&&y<=9 || (x>=4&&x<=6||x>=26&&x<=28)&&y>=9&&y<=11,
            2 => y>=8&&y<=9&&x>=11 || (x>=11&&x<=14||x>=26&&x<=28)&&y>=9&&y<=12,
            3 => y>=7&&y<=8 || (x>=7&&x<=10||x>=27&&x<=29)&&y>=8&&y<=13,
            4 => x>=9&&x<=24&&y>=3&&y<=5,
            5 => y==8&&x>=15 || x==16&&y>=8&&y<=13,
            _ => false
        };

        static void AddDarkGroundDetour(HashSet<Vector2Int> area)
        {
            // The former direct route through the turnstiles runs into a full partition.
            for(int y=13;y<=18;y++)area.Remove(new Vector2Int(38,y));
            for(int x=32;x<=41;x++)for(int y=9;y<=11;y++)area.Add(new Vector2Int(x,y));
            for(int y=11;y<=12;y++){
                for(int x=32;x<=34;x++)area.Add(new Vector2Int(x,y));
                for(int x=39;x<=41;x++)area.Add(new Vector2Int(x,y));
            }
        }
    }
}
