//************************************************
// Disable the power draw of a CompPower if
// it is currently disallowed from drawing power
//************************************************

using RimWorld;
using HarmonyLib;
using LightsOut.Utility;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Power
{
    //[HarmonyPatch(typeof(CompPowerTrader))]
    //[HarmonyPatch("PowerOutput", MethodType.Getter)]
    public class DisablePowerDrawPatch
    {
        public static void Postfix(CompPowerTrader __instance, ref float __result)
        {
            /*
            if(__instance.PowerOn)
            {
                bool? canConsumePower = ModResources.CanConsumePower(__instance);

                if (canConsumePower is null)
                    return;

                if (canConsumePower == false)
                {
                    if (ModResources.CanBeLight(__instance.parent as Building))
                        __result = 0f;
                    else
                        __result *= ModSettings.StandbyPowerDrawRate;
                }
                else if(canConsumePower == true)
                {
                    if (ModResources.IsTable(__instance.parent as Building))
                        __result *= ModSettings.ActivePowerDrawRate;
                }
            }
            */
        }
    }
}