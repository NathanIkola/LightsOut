//************************************************
// Disable all compatible building as soon
// as they spawn 
//************************************************

using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using LightsOut.Utility;
using LightsOut.ThingComps;

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
            {
                ModResources.DisableTable(__instance);
            }
            else if ((light = ModResources.GetLightResources(__instance)) != null)
            {
                if (ModResources.RoomIsEmpty(ModResources.GetRoom(__instance), null)
                    || ModResources.AllPawnsSleeping(ModResources.GetRoom(__instance), null))
                    ModResources.DisableLight(light);
                else
                    ModResources.EnableLight(light);
            }
            // rechargeables should probably be enabled by default
            else if (ModResources.IsRechargeable(__instance))
            {
                CompRechargeable rechargeable = __instance.GetComp<CompRechargeable>();
                if (rechargeable.Charged)
                    ModResources.DisableTable(__instance);
                else
                    ModResources.EnableTable(__instance);
            }
        }
    }
}