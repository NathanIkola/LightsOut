using Verse;
using HarmonyLib;
using Verse.AI;
using LightsOut.Boilerplate;

namespace LightsOut.Patches.Lights
{
    /// <summary>
    /// Detects if a Pawn has changed rooms during a tick
    /// </summary>
    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch("TryEnterNextPathCell")]
    public class DetectPawnRoomChange
    {
        /// <summary>
        /// Grabs the room a Pawn is in at the beginning of a tick 
        /// and puts it into <paramref name="__state"/>
        /// </summary>
        /// <param name="___pawn">The Pawn being tracked</param>
        /// <param name="__state">The Room that <paramref name="__instance"/> was in at the beginning of the tick</param>
        public static void Prefix(Pawn ___pawn, ref Room __state)
        {
            if ((!LightsOutSettings.AnimalParty && ___pawn.RaceProps.Animal) || !LightsOutSettings.FlickLights) return;

            __state = ___pawn.GetRoom();
        }

        /// <summary>
        /// Compares <paramref name="__state"/> with the Room that
        /// <paramref name="___pawn"/> is in at the end of a tick
        /// to see if it changed rooms
        /// </summary>
        /// <param name="___pawn">The Pawn being tracked</param>
        /// <param name="__state">The Room that <paramref name="__instance"/> was in at the beginning of the tick</param>
        public static void Postfix(Pawn ___pawn, ref Room __state)
        {
            if ((!LightsOutSettings.AnimalParty && ___pawn.RaceProps.Animal) || !LightsOutSettings.FlickLights) return;

            Room oldRoom = __state;
            Room newRoom = ___pawn.GetRoom();

            // if there was a room change
            if (newRoom != oldRoom)
            {
                if (!(oldRoom is null) && Common.Lights.ShouldTurnOffAllLights(oldRoom, ___pawn))
                    Common.Lights.DisableAllLights(oldRoom);

                if (!(newRoom is null) && Common.Lights.ShouldTurnOffAllLights(newRoom, ___pawn))
                    Common.Lights.EnableAllLights(newRoom);
            }
        }
    }
}