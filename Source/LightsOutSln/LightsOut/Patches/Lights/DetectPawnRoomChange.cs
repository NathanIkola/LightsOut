using Verse;
using HarmonyLib;
using ModSettings = LightsOut.Boilerplate.ModSettings;
using System.Collections.Generic;

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
        public static void Prefix(Pawn __instance)
        {

            RoomCache[__instance] = __instance.GetRoom();
        }

        /// <summary>
        /// Compares <paramref name="__state"/> with the Room that
        /// <paramref name="__instance"/> is in at the end of a tick
        /// to see if it changed rooms
        /// </summary>
        /// <param name="__instance">The Pawn being tracked</param>
        /// <param name="__state">The Room that <paramref name="__instance"/> was in at the beginning of the tick</param>
        public static void Postfix(Pawn __instance)
        {
            Room oldRoom = RoomCache[__instance];

            // animals PLEASE don't turn on my lights
            if (__instance.RaceProps.Animal || !ModSettings.FlickLights) return;

            Room newRoom = __instance.GetRoom();

            // if there was a room change
            if (newRoom != oldRoom)
            {
                if (!(oldRoom is null) && Common.Lights.ShouldTurnOffAllLights(oldRoom, __instance))
                    Common.Lights.DisableAllLights(oldRoom);

                if (!(newRoom is null) && Common.Lights.ShouldTurnOffAllLights(newRoom, __instance))
                    Common.Lights.EnableAllLights(newRoom);
            }
        }

        /// <summary>
        /// The cache that holds the room in lieu of __state
        /// </summary>
        private static Dictionary<Pawn, Room> RoomCache { get; } = new Dictionary<Pawn, Room>();
    }
}