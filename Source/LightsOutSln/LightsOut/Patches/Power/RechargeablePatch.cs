//************************************************
// Patch the neural supercharger
//************************************************

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(CompRechargeable))]
    [HarmonyPatch(nameof(CompRechargeable.CompTick))]
    public class RechargeablePatch
    {
        public static void Prefix(CompRechargeable __instance, CompPowerTrader ___compPowerCached)
        {
            if (___compPowerCached is null) return;
            
            float powerDraw = ___compPowerCached.Props.basePowerConsumption;
            if(powerDraw > 0)
            {
                if (__instance.Charged) 
                    powerDraw *= ModSettings.StandbyPowerDrawRate;
                else 
                    powerDraw *= ModSettings.ActivePowerDrawRate;
                ___compPowerCached.powerOutputInt = powerDraw;
            }
        }
    }
}