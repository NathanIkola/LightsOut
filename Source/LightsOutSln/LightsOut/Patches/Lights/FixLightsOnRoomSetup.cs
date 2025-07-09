using HarmonyLib;
using Verse;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(RegionAndRoomUpdater), "FloodAndSetRooms")]
    public class FixLightsOnRoomSetup
    {
        /// <summary>
        /// Runs after a room is resized, automatically enabling/disabling lights as needed
        /// </summary>
        /// <param name="room">The <see cref="Room"/> that was updated</param>
        public static void Postfix(Room room)
        {
            if (room is null) return;
            if (Common.Lights.ShouldTurnOffAllLights(room, null))
                Common.Lights.DisableAllLights(room);
            else 
                Common.Lights.EnableAllLights(room);
        }
    }
}