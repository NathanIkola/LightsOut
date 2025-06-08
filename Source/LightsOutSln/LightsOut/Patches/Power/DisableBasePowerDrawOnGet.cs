using RimWorld;
using HarmonyLib;
using System;
using LightsOut.Common;
using LightsOutSettings = LightsOut.Boilerplate.ModSettings;

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
        /// <param name="__state">Whether or not the power trader can consume resources</param>
        public static void Postfix(CompPowerTrader __instance, ref float __result, bool? __state)
        {
            if (__state is null) 
                return;

            if (__state == false)
                __result = Math.Min(__result * LightsOutSettings.StandbyResourceDrawRate, Resources.MinDraw);
            else if (Tables.IsTable(__instance.parent))
                __result *= LightsOutSettings.ActiveResourceDrawRate;
        }

        /// <summary>
        /// Early quit for lights that don't need the rest of the power draw code,
        /// with an early cached lookup of Resources.CanConsumeResources
        /// </summary>
        /// <param name="__instance">The CompPowerTrader to adjust</param>
        /// <param name="__result">The resulting power draw of this <paramref name="__instance"/></param>
        /// <param name="__state">Whether or not the power trader can consume resources</param>
        /// <returns></returns>
        public static bool Prefix(CompPowerTrader __instance, ref float __result, ref bool? __state)
        {
            __state = Resources.CanConsumeResources(__instance);
            if (__state != false) { return true; }
            if (Common.Lights.CanBeLight(__instance.parent))
            {
                __result = Resources.MinDraw;
                return false;
            }

            return true;
        }
    }
}