using Verse;
using HarmonyLib;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    /// <summary>
    /// Detects if a Pawn has changed rooms during a tick
    /// </summary>
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.Tick))]
    public class DetectPawnRoomChange
    {
        /// <summary>
        /// Grabs the room a Pawn is in at the beginning of a tick 
        /// and puts it into <paramref name="__state"/>
        /// </summary>
        /// <param name="__instance">The Pawn being tracked</param>
        /// <param name="__state">The Room that <paramref name="__instance"/> was in at the beginning of the tick</param>
        public static void Prefix(Pawn __instance, ref Room __state)
        {
            __state = __instance.GetRoom();
        }

        /// <summary>
        /// Compares <paramref name="__state"/> with the Room that
        /// <paramref name="__instance"/> is in at the end of a tick
        /// to see if it changed rooms
        /// </summary>
        /// <param name="__instance">The Pawn being tracked</param>
        /// <param name="__state">The Room that <paramref name="__instance"/> was in at the beginning of the tick</param>
        public static void Postfix(Pawn __instance, Room __state)
        {
            // animals PLEASE don't turn on my lights
            if (__instance.RaceProps.Animal || !ModSettings.FlickLights) return;

            Room newRoom = __instance.GetRoom();

            // if there was a room change
            if(newRoom != __state)
            {
                if (!(__state is null) && Common.Lights.ShouldTurnOffAllLights(__state, __instance))
                    Common.Lights.DisableAllLights(__state);

                if (!(newRoom is null) && Common.Lights.ShouldTurnOffAllLights(newRoom, __instance))
                    Common.Lights.EnableAllLights(newRoom);
            }
        }
    }
}