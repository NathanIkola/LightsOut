using HarmonyLib;
using LightsOut.Common;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace LightsOut.Patches.Benches
{
    /// <summary>
    /// Detects when a Pawn gets to a bench to do work
    /// </summary>
    [HarmonyPatch(typeof(JobDriver))]
    [HarmonyPatch(nameof(JobDriver.Notify_PatherArrived))]
    public class TriggerOnPawnArrivedToBench
    {
        /// <summary>
        /// Check if a Pawn is arriving at an applicable bench or building
        /// </summary>
        /// <param name="__instance">The <see cref="JobDriver"/> that is arriving at its destination</param>
        public static void Prefix(JobDriver __instance)
        {
            if (__instance is null) return;

            Pawn pawn = __instance.GetActor();

            if (__instance is JobDriver_DoBill billDriver)
                PawnIsAtTable(billDriver, pawn);
            else if (__instance is JobDriver_Research researchDriver)
                PawnIsAtResearchBench(researchDriver, pawn);
            else if (__instance.job?.GetTarget(TargetIndex.A).Thing is Building tv && Tables.IsTelevision(tv))
                PawnIsAtTelevision(__instance, tv, pawn);
        }

        /// <summary>
        /// Check if a Pawn is at an applicable table
        /// </summary>
        /// <param name="jobDriver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="pawn">The pawn arriving to the table</param>
        private static void PawnIsAtTable(JobDriver_DoBill jobDriver, Pawn pawn)
        {
            IBillGiver giver = jobDriver.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
            if(giver is Building_WorkTable table)
            {
                if (!Tables.IsTable(table)) return;

                if (pawn.Position == table.InteractionCell)
                    ActivateBench(jobDriver, table);
            }
        }

        /// <summary>
        /// Check if a Pawn is at an applicable research bench
        /// </summary>
        /// <param name="researchDriver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="pawn">The pawn arriving to the research bench</param>
        private static void PawnIsAtResearchBench(JobDriver_Research researchDriver, Pawn pawn)
        {
            if (researchDriver.job.targetA.Thing is Building_ResearchBench bench)
            {
                if(pawn.Position == bench.InteractionCell)
                    ActivateBench(researchDriver, bench);
            }
        }

        /// <summary>
        /// Check if a Pawn is at a television
        /// </summary>
        /// <param name="driver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="pawn">The pawn arriving to the television</param>
        private static void PawnIsAtTelevision(JobDriver driver, Building tv, Pawn pawn)
        {
            if (tv is null || pawn is null)
                return;

            IEnumerable<IntVec3> watchArea = WatchBuildingUtility.CalculateWatchCells(tv.def, tv.Position, tv.Rotation, tv.Map);

            Tables.EnableTable(tv);

            driver.AddFinishAction(() =>
            {
                foreach (IntVec3 cell in watchArea)
                {
                    IEnumerable<Pawn> pawns = from thing in cell.GetThingList(tv.Map)
                                       where thing is Pawn
                                       select thing as Pawn;

                    foreach (Pawn p in pawns)
                        if (p != pawn && p.CurJob?.GetTarget(TargetIndex.A).Thing == tv)
                            return;
                }

                Tables.DisableTable(tv);
            });
        }

        /// <summary>
        /// Activate a bench and set it to deactivate when the Pawn finishes
        /// </summary>
        /// <param name="driver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="table">The building that is being turned on or off</param>
        private static void ActivateBench(JobDriver driver, Building table)
        {
            // activate the bench
            Tables.EnableTable(table);
            //ModResources.SetConsumesPower(powerTrader, true);
            // set the bench to turn off after the job is complete
            driver.AddFinishAction(() =>
            {
                Tables.DisableTable(table);
            });
        }
    }
}