using UnityEngine;
using UnityEngine.UI;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static readonly (int X, int Y, string Name, string Sprite, string Statement)[] GalleryWitnesses =
        {
            (5,6,"Mara","pb-recepcionista",
                "Las franjas verdes son refugios. Fuera de ellas, los pasos atraen a la sombra. Si se acerca, detente o vuelve a la luz. La ventilación y el retorno van antes que el cerrojo."),
            (27,7,"Felipe","pb-estudiante",
                "El tubo de retorno daba tirones mientras la ventilación estaba cerrada. El retorno solo se puede abrir después de dejar salir el aire."),
            (22,16,"Celia","pb-estudiante",
                "El cerrojo queda junto a la entrada. Cuando se abra, solo quedará reunir al equipo para que pueda sujetar el paso.")
        };
        static readonly (int X, int Y, string Name, string Sprite, Vector2 Size)[] GalleryMechanisms =
        {
            (27,13,"Ventilación","utileria-ventilador-muro",new Vector2(1.68f,1.54f)),
            (17,16,"Retorno","utileria-medidor-industrial",new Vector2(1.38f,1.52f)),
            (8,5,"Cerrojo","bloque-a-item-tablero-apagado",new Vector2(1.13f,1.3f))
        };
        const int AllGalleryWitnesses = 7;
        const float GalleryNoiseLimit = 5f;
        const float GalleryRefugeRadius = 1.65f;
        int galleryClues, galleryProgress, galleryRefuge;
        float galleryNoise, lastGalleryStep;
        bool galleryWarned;
        SpriteRenderer galleryShadow;
        readonly SpriteRenderer[] galleryMechanismViews = new SpriteRenderer[GalleryMechanisms.Length];
        GameObject galleryWatchPanel;
        Text galleryWatchText;
        Image galleryWatchBar;
        bool GalleryReady => blackoutRebuilt && acousticsApproved;
        bool GallerySolved => galleryProgress == GalleryMechanisms.Length;
        Vector2 GalleryPosition(int x,int y) => World.Cell(ServiceGallery,x,y);
        Vector2 GallerySwitch(int index) => GalleryPosition(GalleryMechanisms[index].X,GalleryMechanisms[index].Y);
        Vector2 GalleryWitness(int index) => GalleryPosition(GalleryWitnesses[index].X,GalleryWitnesses[index].Y);

        void ShowGalleryContacts()
        {
            if(Room != ServiceGallery) return;
            for(int i = 0; i < GalleryWitnesses.Length; i++)
            {
                var person = GalleryWitnesses[i];
                World.MarkEscapeContact(Room,GalleryWitness(i),person.Name,person.Sprite);
            }
            for(int i = 0; i < GalleryMechanisms.Length; i++)
            {
                var mechanism=GalleryMechanisms[i];
                galleryMechanismViews[i]=World.PlaceServiceEquipment(Room,GallerySwitch(i),mechanism.Name,
                    mechanism.Sprite,mechanism.Size);
            }
            galleryShadow = World.MarkEscapeContact(Room,GalleryPosition(20,14),"Sombra de la galería","shadow-00").GetComponent<SpriteRenderer>();
            RefreshGalleryMechanisms();
        }

        void EnterGalleryWatch()
        {
            galleryNoise = 0;
            galleryWarned = false;
            if(Room != ServiceGallery) return;
            float closest = float.PositiveInfinity;
            for(int i = 0; i < World.GalleryLightCount; i++)
            {
                float distance = (Player.Position-World.GalleryLight(i)).sqrMagnitude;
                if(distance >= closest) continue;
                closest = distance;
                galleryRefuge = i;
            }
            galleryShadow.transform.position = GalleryPosition(20,14) + Vector2.up*.62f;
        }

        int GalleryLightAt(Vector2 position)
        {
            for(int i = 0; i < World.GalleryLightCount; i++)
                if((World.GalleryLight(i)-position).sqrMagnitude <= GalleryRefugeRadius*GalleryRefugeRadius) return i;
            return -1;
        }

        // Called by the motor for each substep so sprinting and running into walls are distinguished.
        public bool ObserveGalleryMovement(Vector2 from,Vector2 to,bool sprinting)
        {
            if(Room != ServiceGallery || GallerySolved) return false;
            int refuge = GalleryLightAt(to);
            if(refuge >= 0)
            {
                galleryRefuge = refuge;
                galleryNoise = 0;
                galleryWarned = false;
                return false;
            }
            float distance = Vector2.Distance(from,to);
            if(distance <= .001f) return false;
            lastGalleryStep = Time.time;
            galleryNoise += distance * (sprinting ? 1.8f : .53f);
            if(galleryNoise < GalleryNoiseLimit)
            {
                if(!galleryWarned && galleryNoise >= GalleryNoiseLimit*.6f)
                {
                    galleryWarned = true;
                    Sound.ShadowWarning();
                }
                return false;
            }

            Player.TouchAxis(Vector2.zero);
            Player.transform.position = World.GalleryLight(galleryRefuge);
            galleryNoise = 0;
            galleryWarned = false;
            Sound.ShadowCaught();
            Notify("La sombra te devolvió a la última luz. El avance se conserva.",4);
            return true;
        }

        void CreateGalleryWatch(Transform safe)
        {
            var plate = Panel(safe,"Atención de la sombra",new Vector2(.02f,.315f),new Vector2(.23f,.40f));
            galleryWatchPanel = plate.gameObject;
            plate.GetComponent<Image>().raycastTarget = false;
            galleryWatchText = Label(plate,"Estado de la sombra",new Vector2(.06f,.36f),new Vector2(.94f,.87f),21);
            var track = Rect(plate,"Fondo de atención",new Vector2(.10f,.19f),new Vector2(.90f,.30f));
            var background = track.gameObject.AddComponent<Image>();
            background.color = new Color(.07f,.12f,.16f);
            background.raycastTarget = false;
            galleryWatchBar = Rect(track,"Nivel de atención",Vector2.zero,Vector2.one).gameObject.AddComponent<Image>();
            galleryWatchBar.raycastTarget = false;
            galleryWatchPanel.SetActive(false);
        }

        void UpdateGalleryWatch(bool playing)
        {
            bool active = Room == ServiceGallery && !GallerySolved && !Finished;
            galleryWatchPanel.SetActive(active && playing);
            if(!active || !playing) return;
            bool inLight = GalleryLightAt(Player.Position) >= 0;
            if(inLight || Time.time-lastGalleryStep>.3f)
                galleryNoise = Mathf.MoveTowards(galleryNoise,0,Time.deltaTime*1.75f);
            float attention = galleryNoise/GalleryNoiseLimit;
            if(attention < .35f) galleryWarned = false;
            galleryWatchText.text = inLight ? "A salvo bajo la luz" :
                attention>=.6f ? "La sombra se acerca" : "Atención de la sombra";
            galleryWatchBar.rectTransform.anchorMax = new Vector2(inLight?1:attention,1);
            galleryWatchBar.color = inLight ? new Color(.34f,.84f,.66f) :
                Color.Lerp(new Color(.85f,.66f,.31f),new Color(.91f,.26f,.29f),attention);
            var anchor = GalleryPosition(20,14)+new Vector2(Mathf.Sin(Time.time*.74f)*4,.38f);
            var visualLift=Vector2.up*.62f;
            Vector2 feet=galleryShadow.transform.position-(Vector3)visualLift;
            bool canApproach=!inLight&&World.HasLineOfSight(feet,Player.Position,false);
            Vector2 target=canApproach?Vector2.Lerp(anchor,Player.Position,attention*.86f):anchor;
            var step=Vector2.MoveTowards(feet,target,Time.deltaTime*Mathf.Lerp(1.1f,4.1f,attention));
            if(World.CanStand(step,false))galleryShadow.transform.position=step+visualLift;
            galleryShadow.sprite = CampusLook.Sprite("shadow-" + ((int)(Time.time*5)%4).ToString("00"));
            galleryShadow.sortingOrder = CampusLook.Depth(galleryShadow.transform.position.y-.62f)-3;
            galleryShadow.color = new Color(.58f,.71f,.82f,Mathf.Lerp(.48f,1,attention));
        }

        bool TryGalleryInteraction(bool interact,out string prompt)
        {
            prompt = "";
            if(Room == DarkGround && Near(World.GalleryEntrance,.80f))
            {
                prompt = GalleryReady?"Entrar · Galería de servicio":"Revisar · Galería de servicio";
                if(interact)
                {
                    if(GalleryReady) UseDoor(World.GalleryEntrance,ServiceGallery,World.Entry(ServiceGallery));
                    else Notify("El cierre responde al archivo de cámaras y al canal de Sonido.");
                }
                return true;
            }
            if(Room != ServiceGallery) return false;
            if(Near(World.GalleryExit,1.04f))
            {
                prompt = "Volver a planta baja";
                if(interact) UseDoor(World.GalleryExit,DarkGround,World.GalleryEntrance+Vector2.down);
                return true;
            }
            for(int i = 0; i < GalleryWitnesses.Length; i++)
            {
                if(!Near(GalleryWitness(i),.85f)) continue;
                int index = i;
                var person = GalleryWitnesses[index];
                prompt = "Hablar · " + person.Name;
                if(interact)
                    OpenDialogue(new[]{new DialogueLine(person.Name,person.Sprite,person.Statement)},()=>galleryClues |= 1<<index);
                return true;
            }
            for(int i = 0; i < GalleryMechanisms.Length; i++)
            {
                if(!Near(GallerySwitch(i),.86f)) continue;
                prompt = "Revisar · " + GalleryMechanisms[i].Name;
                if(interact) UseGallerySwitch(i);
                return true;
            }
            // All exits and possible interactions in this room are handled here.
            return true;
        }

        void UseGallerySwitch(int index)
        {
            if(index < galleryProgress)
            {
                Notify(GalleryMechanisms[index].Name+" en posición");
                return;
            }
            if(galleryClues != AllGalleryWitnesses)
            {
                Notify("Habla con Mara, Felipe y Celia antes de mover el mecanismo.",4);
                return;
            }
            if(index != galleryProgress)
            {
                Sound.ScanResult(false);
                Notify("El mecanismo se traba. Revisa el orden anotado en el diario.",4);
                return;
            }
            galleryProgress++;
            RefreshGalleryMechanisms();
            if(GallerySolved)
            {
                Sound.MechanismReleased();
                Notify("Cerrojo liberado · vuelve al equipo en planta baja",5);
            }
            else
            {
                Sound.ToggleSwitch();
                Notify(GalleryMechanisms[index].Name+" en posición · "+galleryProgress+"/"+GalleryMechanisms.Length);
            }
        }

        void RefreshGalleryMechanisms()
        {
            for(int i = 0; i < galleryMechanismViews.Length; i++)
                galleryMechanismViews[i].color = i<galleryProgress?new Color(.55f,.96f,.75f):new Color(.84f,.70f,.55f);
            if(GallerySolved)galleryMechanismViews[2].sprite=CampusLook.Sprite("bloque-a-item-tablero-activo");
            galleryShadow.enabled = !GallerySolved;
        }

        string GalleryObjective()
        {
            if(Room != ServiceGallery)return ThoughtOnFloor(0,"Tengo que liberar el cerrojo. La Galería de servicio queda en el pasillo oeste, junto al Auditorio");
            if(GallerySolved)return "Ya liberé el cerrojo. Tengo que volver con el equipo a planta baja.";
            for(int i=0;i<GalleryWitnesses.Length;i++)
                if(!HasEscapeFlag(galleryClues,i))return "Tengo que buscar a "+GalleryWitnesses[i].Name+". Será mejor moverme entre las luces.";
            return "Debería consultar el diario antes de mover los mecanismos de la galería.";
        }
    }
}
