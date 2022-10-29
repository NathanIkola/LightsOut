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
    /// <summary>
    /// Enables tables when Pawns spawn, allowing Pawns in the middle of a job
    /// to enable their benches
    /// </summary>
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.SpawnSetup))]
    public class FixBuildingsOnPawnSpawn
    {
        /// <summary>
        /// After a Pawn spawns, it checks if their current job involves a bench
        /// and enables it if so
        /// </summary>
        /// <param name="__instance"></param>
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
                        && Resources.CanConsumeResources(building) == false)
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
            else if(driver.asleep && !ModSettings.NightLights)
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
                        toil.AddFinishAction(() => 
                        {
                            Sleepers.Remove(pawn);
                            Common.Lights.EnableAllLights(pawn.GetRoom());
                        });
                    }
                });
            }
        }

        /// <summary>
        /// List of sleeping Pawns (TODO: evaluate if this is doing anything)
        /// </summary>
        private static readonly Dictionary<Pawn, bool> Sleepers = new Dictionary<Pawn, bool>();
    }
}