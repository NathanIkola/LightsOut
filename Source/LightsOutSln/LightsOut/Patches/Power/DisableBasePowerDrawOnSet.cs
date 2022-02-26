//************************************************
// Set the power draw of a bench when it connects
// to a power net to make sure it draws the
// correct power
//************************************************

using RimWorld;
using HarmonyLib;
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
            bool? canConsumePower = Common.Power.CanConsumePower(__instance);

            if (canConsumePower == true)
            {
                if (Common.Tables.IsTable(__instance.parent as Building))
                    __instance.powerOutputInt *= ModSettings.ActivePowerDrawRate;
            }
            else if (canConsumePower == false)
            {
                if (Common.Lights.CanBeLight(__instance.parent as Building))
                    __instance.powerOutputInt = 0f;
                else
                    __instance.powerOutputInt *= ModSettings.StandbyPowerDrawRate;
            }
        }
    }
}