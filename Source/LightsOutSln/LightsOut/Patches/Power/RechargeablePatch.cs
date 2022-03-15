using HarmonyLib;
using RimWorld;
using ModSettings = LightsOut.Boilerplate.ModSettings;
using CPower = LightsOut.Common.Resources;
using System;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Parches rechargeable objects to only draw power 
    /// when they are actively charging
    /// </summary>
    [HarmonyPatch(typeof(CompRechargeable))]
    [HarmonyPatch(nameof(CompRechargeable.CompTick))]
    public class RechargeablePatch
    {
        /// <summary>
        /// Checks if a CompRechargeable is charged, and modifies its 
        /// power draw rate if not
        /// </summary>
        /// <param name="__instance">The CompRechargeable to check</param>
        /// <param name="___compPowerCached">The cahced CompPower object for <paramref name="__instance"/></param>
        public static void Prefix(CompRechargeable __instance, CompPowerTrader ___compPowerCached)
        {
            if (___compPowerCached is null) return;
            
            float powerDraw = -___compPowerCached.Props.basePowerConsumption;
            if(powerDraw > 0)
            {
                if (__instance.Charged) 
                    powerDraw *= ModSettings.StandbyResourceDrawRate;
                else 
                    powerDraw *= ModSettings.ActiveResourceDrawRate;
                ___compPowerCached.powerOutputInt = Math.Min(powerDraw, CPower.MinDraw);
            }
        }
    }
}