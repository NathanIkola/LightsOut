//************************************************
// Add the "On Standby" message in the inspect
// panel when the building is selected and the
// building is inactive
//************************************************

using RimWorld;
using HarmonyLib;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(CompPower))]
    [HarmonyPatch("CompInspectStringExtra")]
    public class AddStandbyInspectMessagePatch
    {
        public static void Postfix(CompPower __instance, ref string __result)
        {
            if (__instance is CompPowerTrader powerTrader && Common.Resources.CanConsumeResources(powerTrader) == false)
                __result = "On Standby";
        }
    }
}