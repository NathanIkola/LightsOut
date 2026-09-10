using HarmonyLib;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Prevents standby buildings from pushing heat while LightsOut has disabled their resource consumption
    /// </summary>
    [HarmonyPatch(typeof(CompHeatPusherPowered))]
    [HarmonyPatch(nameof(CompHeatPusherPowered.ShouldPushHeatNow), MethodType.Getter)]
    public class DisableStandbyHeatPush
    {
        /// <summary>
        /// Prevents continuous heat production while the parent is on standby
        /// </summary>
        /// <param name="__instance">The heat pusher being checked</param>
        /// <param name="__result">Whether the heat pusher would otherwise produce heat</param>
        public static void Postfix(CompHeatPusherPowered __instance, ref bool __result)
        {
            if (Resources.CanConsumeResources(__instance.parent) == false)
                __result = false;
        }
    }
}
