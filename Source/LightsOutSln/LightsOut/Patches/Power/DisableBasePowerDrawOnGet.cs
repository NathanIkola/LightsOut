//************************************************
// Set the power draw of a bench when it connects
// to a power net to make sure it draws the
// correct power
//************************************************

using RimWorld;
using HarmonyLib;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;
using System;
using LightsOut.Common;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(CompPowerTrader))]
    [HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Getter)]
    public class DisableBasePowerDrawOnGet
    {
        public static void Postfix(CompPowerTrader __instance, ref float __result)
        {
            bool? canConsumePower = Resources.CanConsumeResources(__instance);

            if (canConsumePower == true)
            {
                if (Common.Tables.IsTable(__instance.parent as Building))
                    __result *= ModSettings.ActivePowerDrawRate;
            }
            else if (canConsumePower == false)
            {
                if (Common.Lights.CanBeLight(__instance.parent as Building))
                    __result = Resources.MinDraw;
                else
                    __result = Math.Min(__result * ModSettings.StandbyPowerDrawRate, Resources.MinDraw);
            }
        }
    }
}