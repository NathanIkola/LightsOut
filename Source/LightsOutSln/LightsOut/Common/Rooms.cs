//************************************************
// Holds all of the common room operations
//************************************************

using RimWorld;
using System;
using Verse;

namespace LightsOut.Common
{
    [StaticConstructorOnStartup]
    public static class Rooms
    {
        //****************************************
        // Detect if any pawns are in the room
        // or if the room is outdoors
        //****************************************
        public static bool RoomIsEmpty(Room room, Pawn pawn)
        {
            if (room is null || room.OutdoorsForWork || room.IsDoorway
                || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)
                || room.Map?.mapPawns.AllPawns is null) return false;

            foreach (Pawn otherPawn in room.Map.mapPawns.AllPawns)
                if (otherPawn.GetRoom() == room && otherPawn.RaceProps.Humanlike && otherPawn != pawn
                        // what if two pawns were both leaving the room at the same time haha... unless?
                        && (otherPawn.pather.nextCell.GetEdifice(otherPawn.Map) as Building_Door) == null
                        // what if a pawn is entering while another pawn is leaving haha... unless??
                        && (otherPawn.Position.GetEdifice(otherPawn.Map) as Building_Door) == null)
                {
                    return false;
                }

            return true;
        }

        //****************************************
        // Detect if all pawns in a room are
        // currently sleeping except the one
        // passed in
        //****************************************
        public static bool AllPawnsSleeping(Room room, Pawn pawn)
        {
            if (room is null || room.OutdoorsForWork || room.IsDoorway
                || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)
                || room.Map?.mapPawns.AllPawns is null) return false;

            foreach (Pawn otherPawn in room.Map.mapPawns.AllPawns)
                if (otherPawn.GetRoom() == room && otherPawn.RaceProps.Humanlike && otherPawn != pawn
                        // what if two pawns were both leaving the room at the same time haha... unless?
                        && (otherPawn.pather.nextCell.GetEdifice(otherPawn.Map) as Building_Door) == null
                        // what if a pawn is entering while another pawn is leaving haha... unless??
                        && (otherPawn.Position.GetEdifice(otherPawn.Map) as Building_Door) == null)
                {
                    if (otherPawn.jobs?.curDriver?.asleep != true)
                        return false;
                }

            return true;
        }

        //****************************************
        // Check if a building is in a room
        //****************************************
        public static bool IsInRoom(Building building, Room room)
        {
            if (building is null || room is null || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return false;
            return GetRoom(building)?.ID == room.ID;
        }

        //****************************************
        // Returns the room a particular
        // building is a part of
        //****************************************
        public static Room GetRoom(Building building)
        {
            if (building is null) return null;
            if (!(building.Map?.regionAndRoomUpdater?.Enabled ?? false))
                return null;
            return building.GetRoom();
        }
    }
}