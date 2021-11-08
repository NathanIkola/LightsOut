//************************************************
// Set the power draw of a bench when it connects
// to a power net to make sure it draws the
// correct power
//************************************************

using RimWorld;
using HarmonyLib;
using LightsOut.Utility;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(CompPowerTrader))]
    [HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Setter)]
    public class DisableBasePowerDrawOnSet
    {
        public static void Postfix(CompPowerTrader __instance)
        {
            if(__instance is CompPowerTrader trader)
            {
                // this simply resets the power draw rate using the normal rules
                bool? canConsumePower = ModResources.CanConsumePower(__instance);
                
                if (canConsumePower == true)
                {
                    if(ModResources.IsTable(__instance.parent as Building))
                        __instance.powerOutputInt *= ModSettings.ActivePowerDrawRate;
                }
                else if (canConsumePower == false)
                {
                    if(ModResources.CanBeLight(__instance.parent as Building))
                        __instance.powerOutputInt = 0f;
                    else
                        __instance.powerOutputInt *= ModSettings.StandbyPowerDrawRate;
                }
            }
        }
    }
}