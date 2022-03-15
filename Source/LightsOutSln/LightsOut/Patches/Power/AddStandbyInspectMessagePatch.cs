using RimWorld;
using HarmonyLib;
using Verse;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Adds the "On Standby" message in the inspect panel
    /// </summary>
    [HarmonyPatch(typeof(CompPower))]
    [HarmonyPatch(nameof(CompPower.CompInspectStringExtra))]
    public class AddStandbyInspectMessagePatch
    {
        /// <summary>
        /// Checks if a CompPowerTrader is disabled, and if so changes the output
        /// of the inspect string to say "On Standby"
        /// </summary>
        /// <param name="__instance">The CompPower being affected</param>
        /// <param name="__result">The output string from the function</param>
        public static void Postfix(CompPower __instance, ref string __result)
        {
            if (__instance is CompPowerTrader powerTrader && Common.Resources.CanConsumeResources(powerTrader) == false)
                __result = "LightsOut_OnStandby".Translate();
        }
    }
}