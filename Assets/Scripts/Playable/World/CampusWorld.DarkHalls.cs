using System.Collections.Generic;
using UnityEngine;

namespace Backfranz.Playable
{
    public sealed partial class CampusWorld
    {
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
                case 1:
                    // Two blind ends turn the records wing into a zigzag.
                    AlongY(10,6,11);AlongX(7,10,14);
                    AlongY(21,4,8);AlongX(8,21,24);
                    break;
                case 2:
                    // Lower isolation corridor and a return lane behind the partitions.
                    AlongY(8,6,10);AlongY(19,7,10);
                    AlongX(3,9,16);
                    break;
                case 3:
                    // Studios become bays around a staggered equipment passage.
                    AlongY(12,5,8);AlongX(8,12,15);
                    AlongY(20,7,10);AlongX(7,20,23);
                    break;
                case 4:
                    AlongY(10,5,8);AlongY(19,8,11);
                    AlongX(6,23,27);
                    break;
                case 5:
                    AlongX(5,8,14);AlongY(18,7,10);
                    AlongY(28,7,10);
                    break;
                case 6:
                    // The shelving lines hem in the open reading lobby.
                    AlongY(11,5,8);AlongY(20,6,10);
                    AlongX(6,5,11);AlongX(6,20,26);
                    break;
            }
        }

        static bool DarkHallAisle(int floor,int x,int y)=>floor switch
        {
            1 => y>=8&&y<=9&&x<20 || x>=24&&y>=5&&y<=10,
            2 => y>=5&&y<=6 || x>=26&&y>=6&&y<=15,
            3 => y>=5&&y<=6&&x>=11&&x<=24 || x>=16&&x<=19&&y>=6&&y<=10,
            4 => y>=7&&y<=8&&x<19 || x>=21&&x<=23&&y>=6&&y<=10,
            5 => y>=6&&y<=7&&x>=7&&x<=18 || x>=20&&x<=23&&y>=7&&y<=10,
            6 => x>=12&&x<=18&&y>=5&&y<=12 || y>=5&&y<=6,
            _ => false
        };

        void DecorateDarkHall(int room,int floor)
        {
            var (x,y)=floor switch{1=>(15,8),2=>(16,6),3=>(17,6),4=>(21,8),5=>(20,7),_=>(15,7)};
            var (accentX,accentY)=floor switch{1=>(21,8),2=>(8,8),3=>(20,8),4=>(19,9),5=>(18,8),_=>(20,8)};
            Prop(room,"bloque-a-cable-caido",x,y,2.4f,.75f,false);
            Prop(room,"bloque-a-charco",x+1,y+1,2.9f,.96f,false);
            Prop(room,"bloque-a-alarma",accentX,accentY,.76f,1.12f,false);
            FloorLight(room,x,y,new Vector2(5.6f,3.5f),new Color(.38f,.8f,.77f,.43f));
            FloorLight(room,accentX,accentY,new Vector2(3.3f,3.8f),new Color(.94f,.24f,.25f,.55f));
            var reflection=Art(room,floor%2==0?"shadow-02":"shadow-00",Cell(room,x+1,y+1),
                new Vector2(.86f,1.34f),"Silueta en el reflejo",-892).GetComponent<SpriteRenderer>();
            reflection.flipY=true;
            reflection.color=new Color(.41f,.68f,.74f,.54f);
        }

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
