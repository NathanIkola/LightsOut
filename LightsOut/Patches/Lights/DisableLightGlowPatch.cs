//************************************************
// Patch the glow update method to disable
// light glow when on standby
//************************************************

using HarmonyLib;
using LightsOut.Utility;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(CompGlower))]
    [HarmonyPatch("ShouldBeLitNow", MethodType.Getter)]
    public class DisableLightGlowPatch
    {
        public static void Postfix(ThingComp __instance, ref bool __result)
        {
            Building parent = __instance.parent as Building;
            if (!ModSettings.FlickLights && !ModResources.IsTable(parent))
                return;

            if(ModResources.CanGlow(__instance) == false)
                __result = false;
        }
    }
}