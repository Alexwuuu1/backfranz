using System;
using UnityEngine;
using static Backfranz.Playable.CampusWorld;

namespace Backfranz.Playable
{
    public sealed partial class SessionFlow
    {
        static string SaveKey => Application.isBatchMode && Array.Exists(Environment.GetCommandLineArgs(),
            arg => arg == "-backfranzSmokeSave") ? "Backfranz.Smoke.Save.v1" : "Backfranz.Save.v1";
        static SaveData requestedSave;

        [Serializable]
        sealed class SaveData
        {
            public int version = 1;
            public int room, floor, visited, lanes, circuit;
            public int inductionBriefs;
            public AuditoriumTool auditoriumTools;
            public bool auditoriumWedged;
            public bool hasCampusRadio;
            public bool hasClassroomChalk, classroomLoopSeen, classroomChalkMarked;
            public bool hasWorkshopCable;
            public int workshopLamp;
            public bool servicesPassFound, servicesPassReturned;
            public int escapeSolved, escapeClues;
            public int escapeParts;
            public int caseEvidence, solvedCases;
            public int rescueRequests,rescueItems,rescuedPeople;
            public int escortSeen,escortedPeople,escorting;
            public bool escortInHall;
            public Vector2 escortLocalFeet;
            public int maintenanceReadings;
            public bool maintenanceApproved;
            public int signalSolved;
            public int[] signalLamps;
            public int blackoutTapes, blackoutWitnesses;
            public bool blackoutRebuilt;
            public int acousticClues;
            public bool acousticsApproved;
            public int galleryClues, galleryProgress;
            public int crewContacts;
            public bool crewAssigned;
            public int finaleStage;
            public float playingSeconds;
            public float escapeSeconds;
            public int stairOrigin, stairDestination;
            public Vector2 position;
            public bool horror, registered, fuse, power, key, badge;
            public bool heardUnattendedReader, heardServiceKnock;

            public bool Valid()
            {
                if (version != 1 || room < Entrance || room >= TotalRooms || (visited & ~127) != 0 ||
                    (lanes & ~63) != 0 || circuit < 0 || circuit > Sequence.Length ||
                    (inductionBriefs & ~InductionBriefMask) != 0 ||
                    (auditoriumTools & ~AllAuditoriumTools) != 0 ||
                    (auditoriumWedged&&(!horror||(auditoriumTools&AuditoriumTool.Wedge)==0)) ||
                    (servicesPassFound&&!horror||servicesPassReturned&&!servicesPassFound) ||
                    (classroomLoopSeen&&!horror||classroomChalkMarked&&(!hasClassroomChalk||!horror)) ||
                    workshopLamp<0||workshopLamp>2||(workshopLamp>0&&(!hasWorkshopCable||!horror)) ||
                    ((escapeSolved|escapeClues)&~EscapeTaskMask)!=0 ||
                    (escapeParts&~RelayPartsMask)!=0 ||
                    (caseEvidence&~AllEvidence)!=0 || (solvedCases&~AllCases)!=0 ||
                    ((rescueRequests|rescueItems|rescuedPeople)&~AllRescues)!=0 ||
                    ((escortSeen|escortedPeople)&~EscortMask)!=0 ||
                    (escortedPeople&~escortSeen)!=0 ||
                    escorting<0||escorting>EscortPeople.Length ||
                    (escortInHall&&escorting==0) ||
                    (escorting>0&&(!HasEscapeFlag(escortSeen,escorting-1)||HasEscapeFlag(escortedPeople,escorting-1))) ||
                    float.IsNaN(escortLocalFeet.x)||float.IsNaN(escortLocalFeet.y)||
                    Mathf.Abs(escortLocalFeet.x)>64||Mathf.Abs(escortLocalFeet.y)>32 ||
                    (maintenanceReadings&~AllPressureReadings)!=0 ||
                    (signalSolved&~AllSignals)!=0 ||
                    ((blackoutTapes|blackoutWitnesses)&~AllBlackoutClues)!=0 ||
                    (blackoutRebuilt&&(blackoutTapes!=AllBlackoutClues||blackoutWitnesses!=AllBlackoutClues)) ||
                    (acousticClues&~AllAcousticClues)!=0 ||
                    (galleryClues&~AllGalleryWitnesses)!=0 ||
                    galleryProgress<0||galleryProgress>GalleryMechanisms.Length ||
                    (galleryProgress>0&&galleryClues!=AllGalleryWitnesses) ||
                    (crewContacts&~AllCrew)!=0 ||
                    finaleStage<0||finaleStage>2 ||
                    (signalLamps!=null&&(signalLamps.Length!=SignalStations.Length||
                        Array.Exists(signalLamps, lamps=>(lamps&~SignalTarget)!=0))) ||
                    float.IsNaN(playingSeconds) || float.IsInfinity(playingSeconds) || playingSeconds<0 ||
                    float.IsNaN(escapeSeconds) || float.IsInfinity(escapeSeconds) || escapeSeconds<0 || escapeSeconds>playingSeconds ||
                    float.IsNaN(position.x) || float.IsNaN(position.y) ||
                    Mathf.Abs(position.x) > 64 || Mathf.Abs(position.y) > 32 ||
                    horror != IsDark(room) || IsLiftCabin(room)) return false;
                if (IsStairwell(room))
                    return stairOrigin >= Ground && stairOrigin <= Top && stairDestination >= Ground &&
                        stairDestination <= Top && Mathf.Abs(stairOrigin-stairDestination) == 1 &&
                        floor == Floor(stairOrigin);
                return floor == (room == Basement ? -1 : room == Entrance ? 0 : Floor(room));
            }
        }

        static SaveData ReadSave()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return null;
            try
            {
                var saved = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
                return saved != null && saved.Valid() ? saved : null;
            }
            catch (ArgumentException) { return null; }
        }

        public static bool CanContinue => ReadSave() != null;
        public static bool RequestContinue() => (requestedSave = ReadSave()) != null;
        public static void RequestNewGame() => requestedSave = null;

        public bool SaveGame()
        {
            if (!Player || Finished || Crossing || DialogueOpen || LiftOpen || StairsOpen || EscapePanelOpen ||
                JournalOpen || BackpackOpen || IsLiftCabin(Room) || !World.CanStand(Player.Position))
                return false;

            var save = new SaveData
            {
                room = Room,
                floor = Room == Basement ? -1 : Room == Entrance ? 0 : Floor(IsStairwell(Room) ? stairOrigin : Room),
                position = Player.Position-World.Offset(Room),
                horror = Horror == HorrorState.Active,
                registered = HasCard,
                inductionBriefs = inductionBriefs,
                auditoriumTools = auditoriumTools,
                auditoriumWedged = auditoriumWedged,
                hasCampusRadio = hasCampusRadio,
                hasClassroomChalk = hasClassroomChalk,
                classroomLoopSeen = classroomLoopSeen,
                classroomChalkMarked = classroomChalkMarked,
                hasWorkshopCable = hasWorkshopCable,
                workshopLamp = workshopLamp,
                servicesPassFound = servicesPassFound,
                servicesPassReturned = servicesPassReturned,
                stairOrigin = stairOrigin,
                stairDestination = stairDestination,
                fuse = HasFuse,
                power = World.Power,
                key = HasKey,
                badge = HasBadge,
                circuit = CircuitProgress,
                escapeSolved = escapeSolved,
                escapeClues = escapeClues,
                escapeParts = escapeParts,
                caseEvidence = caseEvidence,
                solvedCases = solvedCases,
                rescueRequests = rescueRequests,
                rescueItems = rescueItems,
                rescuedPeople = rescuedPeople,
                escortSeen = escortSeen,
                escortedPeople = escortedPeople,
                escorting = escorting,
                escortInHall = escortInHall,
                escortLocalFeet = escortLocalFeet,
                maintenanceReadings = maintenanceReadings,
                maintenanceApproved = maintenanceApproved,
                signalSolved = signalSolved,
                signalLamps = signalLamps,
                blackoutTapes = blackoutTapes,
                blackoutWitnesses = blackoutWitnesses,
                blackoutRebuilt = blackoutRebuilt,
                acousticClues = acousticClues,
                acousticsApproved = acousticsApproved,
                galleryClues = galleryClues,
                galleryProgress = galleryProgress,
                crewContacts = crewContacts,
                crewAssigned = crewAssigned,
                finaleStage = finaleStage,
                heardUnattendedReader = heardUnattendedReader,
                heardServiceKnock = heardServiceKnock,
                playingSeconds = playingSeconds,
                escapeSeconds = escapeSeconds
            };
            for (int floor = 0; floor < visits.Length; floor++)
                if (visits[floor]) save.visited |= 1 << floor;
            for (int lane = 0; lane < AccessLaneCount; lane++)
                if (World.SecurityOpen(lane)) save.lanes |= 1 << lane;
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
            return true;
        }

        bool RestoreRequestedSave()
        {
            var save = requestedSave;
            requestedSave = null;
            if (save == null) return false;

            Horror = save.horror ? HorrorState.Active : HorrorState.Normal;
            HasCard = HasSchedule = save.registered;
            inductionBriefs = save.inductionBriefs;
            auditoriumTools = save.auditoriumTools;
            auditoriumWedged = save.auditoriumWedged;
            hasCampusRadio = save.hasCampusRadio;
            hasClassroomChalk = save.hasClassroomChalk;
            classroomLoopSeen = save.classroomLoopSeen;
            classroomChalkMarked = save.classroomChalkMarked;
            hasWorkshopCable = save.hasWorkshopCable;
            workshopLamp = save.workshopLamp;
            servicesPassFound = save.servicesPassFound;
            servicesPassReturned = save.servicesPassReturned;
            HasFuse = save.fuse;
            HasKey = save.key;
            HasBadge = save.badge;
            CircuitProgress = save.circuit;
            CircuitSolved = CircuitProgress == Sequence.Length;
            escapeSolved = save.escapeSolved;
            escapeClues = save.escapeClues;
            escapeParts = save.escapeParts;
            caseEvidence = save.caseEvidence;
            solvedCases = save.solvedCases;
            rescueRequests = save.rescueRequests;
            rescueItems = save.rescueItems;
            rescuedPeople = save.rescuedPeople;
            escortSeen = save.escortSeen;
            escortedPeople = save.escortedPeople;
            if(HasEscapeFlag(rescuedPeople,NuriaAid))
            {
                // In older saves Nuria reached the exit after a supply delivery.
                escortSeen|=1<<NuriaEscort;
                escortedPeople|=1<<NuriaEscort;
                rescuedPeople&=~(1<<NuriaAid);
            }
            escorting = save.escorting;
            escortInHall = save.escortInHall;
            escortLocalFeet = save.escortLocalFeet;
            maintenanceReadings = save.maintenanceReadings;
            maintenanceApproved = save.maintenanceApproved;
            signalSolved = save.signalSolved;
            if(save.signalLamps!=null)signalLamps=save.signalLamps;
            blackoutTapes = save.blackoutTapes;
            blackoutWitnesses = save.blackoutWitnesses;
            blackoutRebuilt = save.blackoutRebuilt;
            acousticClues = save.acousticClues;
            acousticsApproved = save.acousticsApproved;
            galleryClues = save.galleryClues;
            galleryProgress = save.galleryProgress;
            crewContacts = save.crewContacts;
            crewAssigned = save.crewAssigned;
            finaleStage = save.finaleStage;
            heardUnattendedReader = save.heardUnattendedReader;
            heardServiceKnock = save.heardServiceKnock;
            playingSeconds = save.playingSeconds;
            escapeSeconds = save.escapeSeconds;
            for (int floor = 0; floor < visits.Length; floor++)
            {
                visits[floor] = (save.visited & (1 << floor)) != 0;
                if (visits[floor]) Visited++;
            }
            World.RestoreAccess(save.lanes, save.power, HasFuse, HasKey, HasBadge);
            bool retiredReception=IsFormerReception(save.room);
            if(retiredReception)save.room=save.horror?DarkGround:Ground;
            stairOrigin = save.stairOrigin;
            stairDestination = save.stairDestination;
            if (IsStairwell(save.room)) stairCore = save.room-FirstStairwell;

            Vector2 position = retiredReception ? World.Cell(save.room,57,16) : World.Offset(save.room)+save.position;
            World.Show(save.room);
            if (!World.CanStand(position))
            {
                if (IsStairwell(save.room)) position = World.StairLanding(save.room, stairOrigin > stairDestination);
                else if (save.room >= Ground && save.room <= Top || save.room >= DarkGround && save.room <= DarkTop)
                    position = save.room == Ground ? World.Entry(Ground) : World.Lift(save.room)+Vector2.down*2;
                else position = World.Entry(save.room);
            }
            Enter(save.room, false, position);
            return true;
        }
    }
}
