using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using LightsOutSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    /// <summary>
    /// Disables lights when Pawns go to sleep
    /// </summary>
    [HarmonyPatch(typeof(Toils_LayDown))]
    [HarmonyPatch(nameof(Toils_LayDown.LayDown))]
    public class DisableLightGlowOnSleep
    {
        /// <summary>
        /// Hijacks the toil creation code to turn off the lights when
        /// a Pawn goes to sleep and turn it back on again when they
        /// wake up
        /// </summary>
        /// <param name="__result">The toil to inject code into</param>
        /// <param name="canSleep">Whether or not the Pawn is allowed to sleep</param>
        public static void Postfix(Toil __result, bool canSleep)
        {
            if (__result is null) { return; }
            if (!LightsOutSettings.NightLights && canSleep)
            {
                __result.AddPreInitAction(() =>
                {
                    Pawn pawn = __result.actor;
                    if (pawn?.RaceProps?.Animal ?? false) return;
                    Room room = pawn.GetRoom();

                    if (!(room is null) && Common.Lights.ShouldTurnOffAllLights(room, pawn)
                        && pawn.jobs.curDriver.asleep)
                        Common.Lights.DisableAllLights(room, false);

                    __result.AddFinishAction(() => 
                    {
                        Common.Lights.EnableAllLights(pawn.GetRoom());
                    });
                });

                Action tickAction = __result.tickAction;

                __result.tickAction = () =>
                {
                    Pawn pawn = __result.actor;
                    if (pawn is null) { return; }
                    // cache the sleeping status of the pawn
                    bool? asleep = pawn.jobs.curDriver?.asleep;
                    
                    tickAction();

                    // check if their status has changed
                    if (pawn.RaceProps.Animal) return;
                    Room room = pawn.GetRoom();
                    if (room is null) { return; }
                    bool shouldTurnOffLights = Common.Lights.ShouldTurnOffAllLights(room, pawn);
                    if(asleep != pawn.jobs.curDriver?.asleep)
                    {
                        // if the pawn was previously asleep
                        if(asleep == true || !shouldTurnOffLights)
                        {
                            // turn the lights back on
                            Common.Lights.EnableAllLights(room);
                        }
                        else if (asleep == false && shouldTurnOffLights)
                        {
                            // turn the lights off
                            Common.Lights.DisableAllLights(room, false);
                        }
                    }
                };
            }
        }
    }
}