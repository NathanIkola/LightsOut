//************************************************
// Disable all compatible things in a room when
// the room finishes being made, such as when
// loading a save
//************************************************

using Verse;
using HarmonyLib;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("FloodAndSetRooms")]
    public class DisableThingsInRoomsOnSpawn
    {
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