//************************************************
// Disable the light glow in a room any time a
// pawn goes to sleep
//************************************************

using System;
using System.Collections.Generic;
using HarmonyLib;
using LightsOut.Utility;
using RimWorld;
using Verse;
using Verse.AI;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(Toils_LayDown))]
    [HarmonyPatch(nameof(Toils_LayDown.LayDown))]
    public class DisableLightGlowOnSleep
    {
        public static void Postfix(Toil __result, bool canSleep)
        {
            if (!ModSettings.NightLights && ModSettings.FlickLights && canSleep)
            {
                __result.AddPreInitAction(() =>
                {
                    Pawn pawn = __result.actor;
                    Room room = pawn?.GetRoom();

                    if (!(room is null) && ModResources.AllPawnsSleeping(room, pawn) 
                        && pawn.jobs.curDriver.asleep)
                        ModResources.DisableAllLights(room);

                    __result.AddFinishAction(() => 
                    {
                        ModResources.EnableAllLights(pawn.GetRoom()); 
                    });
                });

                Action tickAction = __result.tickAction;

                __result.tickAction = () =>
                {
                    Pawn pawn = __result.actor;
                    // cache the sleeping status of the pawn
                    bool? asleep = pawn.jobs.curDriver?.asleep;
                    
                    tickAction();

                    // check if their status has changed
                    if(asleep != pawn.jobs.curDriver?.asleep)
                    {
                        // if the pawn was previously asleep
                        if(asleep == true)
                        {
                            // turn the lights back on
                            ModResources.EnableAllLights(pawn.GetRoom());
                        }
                        else if (asleep == false)
                        {
                            // turn the lights off
                            ModResources.DisableAllLights(pawn.GetRoom());
                        }
                    }
                };
            }
        }
    }
}