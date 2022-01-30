//************************************************
// Disable all compatible building as soon
// as they spawn 
//************************************************

using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using LightsOut.Common;
using LightsOut.ThingComps;
using System;

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
                Room room = ModResources.GetRoom(__instance);
                if (!(room is null) && ModResources.ShouldTurnOffAllLights(room, null))
                    ModResources.DisableLight(light);
                else
                    ModResources.EnableLight(light);

                // return so that we don't remove the KeepOnComp from this
                return;
            }

            bool removed = false;
            uint attempts = 0;
            while(!removed)
            {
                try
                {
                    __instance.AllComps.RemoveAll(x => x is KeepOnComp);
                    removed = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) removed = true;
                    }
                    else
                    {
                        Log.Warning($"[LightsOut](SpawnSetup): InvalidOperationException: {e.Message}");
                        removed = true;
                    }
                }
            }
            if (attempts > 1)
                Log.Warning($"[LightsOut](SpawnSetup): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");
        }
    }
}