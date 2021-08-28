//************************************************
// Disable the power draw of a CompPower if
// it is currently disallowed from drawing power
//************************************************

using RimWorld;
using HarmonyLib;
using LightsOut.Utility;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(CompPowerTrader))]
    [HarmonyPatch("PowerOutput", MethodType.Getter)]
    public class DisablePowerDrawPatch
    {
        public static void Postfix(CompPowerTrader __instance, ref float __result)
        {
            if (__instance.PowerOn && ModResources.CanConsumePower(__instance) == false)
                __result = 0f;
        }
    }
}