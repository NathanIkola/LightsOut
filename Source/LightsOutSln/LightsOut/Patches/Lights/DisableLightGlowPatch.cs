//************************************************
// Patch the glow update method to disable
// light glow when on standby
//************************************************

using HarmonyLib;
using LightsOut.Common;
using Verse;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(CompGlower))]
    [HarmonyPatch("ShouldBeLitNow", MethodType.Getter)]
    public class DisableLightGlowPatch
    {
        public static void Postfix(ThingComp __instance, ref bool __result)
        {
            if(Glowers.CanGlow(__instance) == false)
                __result = false;
        }
    }
}