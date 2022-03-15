using HarmonyLib;
using LightsOut.Common;
using Verse;

namespace LightsOut.Patches.Lights
{
    /// <summary>
    /// Turns off glowers if necessary
    /// </summary>
    [HarmonyPatch(typeof(CompGlower))]
    [HarmonyPatch("ShouldBeLitNow", MethodType.Getter)]
    public class DisableLightGlowPatch
    {
        /// <summary>
        /// Disables a glower when needed
        /// </summary>
        /// <param name="__instance">The glower being affected</param>
        /// <param name="__result">Whether or not this glower should be able to glow</param>
        public static void Postfix(ThingComp __instance, ref bool __result)
        {
            if(Glowers.CanGlow(__instance) == false)
                __result = false;
        }
    }
}