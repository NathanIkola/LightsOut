//************************************************
// Disable all compatible building as soon
// as they spawn 
//************************************************

using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using LightsOut.Utility;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(Building))]
    [HarmonyPatch("SpawnSetup")]
    public class DisableBuildingsOnSpawn
    {
        public static void Postfix(Building __instance)
        {
            KeyValuePair<CompPowerTrader, ThingComp>? light;
            if (ModResources.IsTable(__instance))
                ModResources.SetConsumesPower(__instance.PowerComp as CompPowerTrader, false);
            else if((light = ModResources.GetLightResources(__instance)) != null)
            {
                CompPowerTrader powerTrader = light?.Key;
                ThingComp glower = light?.Value;

                if (ModResources.RoomIsEmpty(ModResources.GetRoom(__instance), null))
                {
                    ModResources.SetConsumesPower(powerTrader, false);
                    ModResources.SetCanGlow(glower, false);
                }
                else
                {
                    ModResources.SetConsumesPower(powerTrader, true);
                    ModResources.SetCanGlow(glower, true);
                }
            }
        }
    }
}