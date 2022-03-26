using RimWorld;
using System;
using Verse;

namespace LightsOut.Common
{
    /// <summary>
    /// Holds common room operations
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Rooms
    {
        /// <summary>
        /// Checks if a <paramref name="room"/> is empty of all pawns except <paramref name="excludedPawn"/>
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <param name="excludedPawn">The pawn to ignore while checking the room</param>
        /// <returns>Whether or not the room is empty</returns>
        public static bool RoomIsEmpty(Room room, Pawn excludedPawn)
        {
            DebugLogger.AssertFalse(room is null, "RoomIsEmpty was called on a null room");
            if (room is null || room.OutdoorsForWork || room.IsDoorway
                || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return false;

            bool done = false;
            uint attempts = 0;
            Thing[] things = null;
            while (!done)
            {
                try
                {
                    things = room.ContainedAndAdjacentThings.ToArray();
                    done = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) done = true;
                    }
                    else
                    {
                        DebugLogger.LogWarning($"(RoomIsEmpty): InvalidOperationException: {e.Message}");
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                DebugLogger.LogWarning($"(RoomIsEmpty): collection was unexpectedly updated {attempts} time(s). If this number is big please report it.");

            // now actually go through the collection
            foreach (Thing thing in things)
                if (thing is Pawn pawn && pawn.RaceProps.Humanlike && pawn != excludedPawn
                    // what if two pawns were both leaving the room at the same time haha... unless?
                    && (pawn.pather.nextCell.GetEdifice(pawn.Map) as Building_Door) == null
                    // what if a pawn is entering while another pawn is leaving haha... unless??
                    && (pawn.Position.GetEdifice(pawn.Map) as Building_Door) == null)
                {
                    return false;
                }

            return true;
        }

        /// <summary>
        /// Checks if all of the pawns in a <paramref name="room"/> are sleeping
        /// </summary>
        /// <param name="room">The room to check</param>
        /// <param name="excludedPawn">The pawn to ignore while checking the room</param>
        /// <returns>Whether or not all pawns in the room are sleeping</returns>
        public static bool AllPawnsSleeping(Room room, Pawn excludedPawn)
        {
            DebugLogger.AssertFalse(room is null, "AllPawnsSleeping called on a null room");
            if (room is null || room.OutdoorsForWork || room.IsDoorway
                || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return false;

            bool done = false;
            uint attempts = 0;
            Thing[] things = null;
            while (!done)
            {
                try
                {
                    things = room.ContainedAndAdjacentThings.ToArray();
                    done = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) done = true;
                    }
                    else
                    {
                        DebugLogger.LogWarning($"(AllPawnsSleeping): InvalidOperationException: {e.Message}");
                        done = true;
                    }
                }
            }
            if (attempts > 1)
                DebugLogger.LogWarning($"(AllPawnsSleeping): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");

            // now actually go through the collection
            foreach (Thing thing in things)
                if (thing is Pawn pawn && pawn.RaceProps.Humanlike && pawn != excludedPawn
                    // what if two pawns were both leaving the room at the same time haha... unless?
                    && (pawn.pather.nextCell.GetEdifice(pawn.Map) as Building_Door) == null
                    // what if a pawn is entering while another pawn is leaving haha... unless??
                    && (pawn.Position.GetEdifice(pawn.Map) as Building_Door) == null)
                {
                    if (pawn.jobs?.curDriver?.asleep != true)
                        return false;
                }

            return true;
        }

        /// <summary>
        /// Check if a building is in a specific room.
        /// It uses the custom GetRoom function so that mod
        /// compatibility patches can change this behavior easily
        /// </summary>
        /// <param name="building">The building to check</param>
        /// <param name="room">The room to check</param>
        /// <returns>Whether or not <paramref name="building"/> is in <paramref name="room"/></returns>
        public static bool IsInRoom(ThingWithComps building, Room room)
        {
            DebugLogger.AssertFalse(building is null, "IsInRoom called on a null building");
            DebugLogger.AssertFalse(room is null, "IsInRoom called on a null room");
            if (building is null || room is null || !(room.Map?.regionAndRoomUpdater?.Enabled ?? false)) return false;
            return GetRoom(building)?.ID == room.ID;
        }

        /// <summary>
        /// Basically just a passthrough to the existing GetRoom function,
        /// but it double checks that the RegionAndRoomUpdater is enabled
        /// and allows mod compatibility patches to change this behavior
        /// </summary>
        /// <param name="building">The building to check</param>
        /// <returns>The room that <paramref name="building"/> is in</returns>
        public static Room GetRoom(ThingWithComps building)
        {
            DebugLogger.AssertFalse(building is null, "GetRoom called on a null building");
            if (building is null) return null;
            if (!(building.Map?.regionAndRoomUpdater?.Enabled ?? false))
                return null;
            return building.GetRoom();
        }
    }
}