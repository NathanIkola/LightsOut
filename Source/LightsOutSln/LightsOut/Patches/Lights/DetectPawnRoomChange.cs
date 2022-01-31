//************************************************
// Detect if a pawn is entering or leaving a
// room and activate or deactivate any lights
// as necessary
//************************************************

using Verse;
using HarmonyLib;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Tick")]
    public class DetectPawnRoomChange
    {
        public static void Prefix(Pawn __instance, ref Room __state)
        {
            __state = __instance.GetRoom();
        }

        public static void Postfix(Pawn __instance, Room __state)
        {
            // animals PLEASE don't turn on my lights
            if (!__instance.RaceProps.Humanlike || !ModSettings.FlickLights) return;

            Room newRoom = __instance.GetRoom();

            // if there was a room change
            if(newRoom != __state)
            {
                if (Common.Lights.ShouldTurnOffAllLights(__state, __instance))
                    Common.Lights.DisableAllLights(__state);

                if (Common.Lights.ShouldTurnOffAllLights(newRoom, __instance))
                    Common.Lights.EnableAllLights(newRoom);
            }
        }
    }
}