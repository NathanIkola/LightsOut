using RimWorld;
using HarmonyLib;
using ModSettings = LightsOut.Boilerplate.ModSettings;
using System;
using LightsOut.Common;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Modifies the power output of CompPowerTraders based on
    /// the enabled/disabled status of the building it's attributed to
    /// </summary>
    [HarmonyPatch(typeof(CompPowerTrader))]
    [HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Getter)]
    public class DisableBasePowerDrawOnGet
    {
        /// <summary>
        /// Determines if the power output of this CompPowerTrader should be adjusted
        /// </summary>
        /// <param name="__instance">The CompPowerTrader to adjust</param>
        /// <param name="__result">The resulting power draw of this <paramref name="__instance"/></param>
        public static void Postfix(CompPowerTrader __instance, ref float __result)
        {
            bool? canConsumePower = Resources.CanConsumeResources(__instance);

            if (canConsumePower == true)
            {
                if (Common.Tables.IsTable(__instance.parent))
                    __result *= ModSettings.ActiveResourceDrawRate;
            }
            else if (canConsumePower == false)
            {
                if (Common.Lights.CanBeLight(__instance.parent))
                    __result = Resources.MinDraw;
                else
                    __result = Math.Min(__result * ModSettings.StandbyResourceDrawRate, Resources.MinDraw);
            }
        }
    }
}