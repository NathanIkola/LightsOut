using Verse;
using HarmonyLib;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Disables all mod-compatible things in a Room when
    /// the Room finishes being made (like when loading)
    /// </summary>
    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("FloodAndSetRooms")]
    public class DisableThingsInRoomsOnSpawn
    {
        /// <summary>
        /// Checks if a Building's Room is empty when it spawns
        /// </summary>
        /// <param name="__1">The Room this ThingWithComps is being spawned into</param>
        public static void Postfix(Room __1)
        {
            Room room = __1;
            if (room is null) return;

            if (Common.Lights.ShouldTurnOffAllLights(room, null))
                Common.Lights.DisableAllLights(room);
            else
                Common.Lights.EnableAllLights(room);
        }
    }
}