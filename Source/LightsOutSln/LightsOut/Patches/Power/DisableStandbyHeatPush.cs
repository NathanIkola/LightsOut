using HarmonyLib;
using LightsOut.Common;
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
        /// Disables powered heat pushers while their parent is on standby
        /// </summary>
        /// <param name="__instance">The heat pusher being checked</param>
        /// <param name="__result">Whether the heat pusher should emit heat</param>
        public static void Postfix(CompHeatPusherPowered __instance, ref bool __result)
        {
            if (__result && Resources.CanConsumeResources(__instance.parent) == false)
                __result = false;
        }
    }
}
