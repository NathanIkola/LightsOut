//************************************************
// Disable the light glow in a room any time a
// pawn goes to sleep
//************************************************

using System;
using HarmonyLib;
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

                    if (!(room is null) && Common.Lights.ShouldTurnOffAllLights(room, pawn)
                        && pawn.jobs.curDriver.asleep)
                        Common.Lights.DisableAllLights(room);

                    __result.AddFinishAction(() => 
                    {
                        Common.Lights.EnableAllLights(pawn.GetRoom());
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
                            Common.Lights.EnableAllLights(pawn.GetRoom());
                        }
                        else if (asleep == false)
                        {
                            // turn the lights off
                            Common.Lights.DisableAllLights(pawn.GetRoom());
                        }
                    }
                };
            }
        }
    }
}