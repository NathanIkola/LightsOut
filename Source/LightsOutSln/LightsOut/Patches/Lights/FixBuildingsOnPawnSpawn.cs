//************************************************
// Squash a bug I've neglected regarding what
// happens when a pawn spawns in. Without this
// patch, if a pawn spawns in the middle of
// using a bench, the bench will still be in
// standby since the pather never "arrived"
// since the pawn never pathed to it
//
// Additionally, it will kill the lights after
// the pawn spawns so that the load state
// matches what you'd expect
//************************************************

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LightsOut.Patches.ModCompatibility;
using LightsOut.Common;
using Verse;
using Verse.AI;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.SpawnSetup))]
    public class FixBuildingsOnPawnSpawn
    {
        public static void Postfix(Pawn __instance)
        {
            JobDriver driver = __instance.jobs?.curDriver;
            if (driver is null)
                return;

            // get the current toil out of the driver
            PropertyInfo curToil = typeof(JobDriver).GetProperty("CurToil", ICompatibilityPatchComponent.BindingFlags);
            Toil toil = (Toil)curToil.GetValue(driver);
            if (toil is null)
                return;

            // turn the power on the bench back on if the pawn is using it
            if (driver is JobDriver_DoBill doBill
                && doBill.job.GetTarget(TargetIndex.A).Thing is Building building
                && Tables.IsTable(building))
            {
                toil.AddPreTickAction(() => 
                {
                    if (!(building.PowerComp is null)
                        && Common.Resources.CanConsumeResources(building.PowerComp) == false)
                    {
                        Tables.EnableTable(building);
                    }
                });

                toil.AddFinishAction(() => 
                { 
                    if (!(building.PowerComp is null))
                        Tables.DisableTable(building); 
                });
            }
            else if(driver.asleep && ModSettings.FlickLights && !ModSettings.NightLights)
            {
                // ask the pawn to nicely turn off the lights when they start sleeping
                toil.AddPreTickAction(() =>
                {
                    Pawn pawn = toil.actor;

                    // otherwise we know we are sleepy
                    if (!Sleepers.ContainsKey(pawn))
                    {
                        Sleepers.Add(pawn, true);
                        Room room = pawn.GetRoom();

                        if (room.OutdoorsForWork)
                            return;

                        if (Common.Lights.ShouldTurnOffAllLights(room, pawn))
                        {
                            Common.Lights.DisableAllLights(room);
                        }

                        // this call to pawn.GetRoom() is strictly required in case the
                        // regions get dirtied before they wake up so that it gets the
                        // room they actually wake up in not the one they went to sleep in
                        toil.AddFinishAction(() => { Common.Lights.EnableAllLights(pawn.GetRoom()); });
                    }
                });
            }
        }

        private static Dictionary<Pawn, bool> Sleepers = new Dictionary<Pawn, bool>();
    }
}