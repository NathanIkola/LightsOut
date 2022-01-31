//************************************************
// Detect if a pawn is at a research bench, then
// have the bench start consuming power if it
// is a valid bench
//************************************************

using HarmonyLib;
using LightsOut.Common;
using RimWorld;
using Verse;
using Verse.AI;

namespace LightsOut.Patches.Benches
{
    [HarmonyPatch(typeof(JobDriver))]
    [HarmonyPatch("Notify_PatherArrived")]
    public class TriggerOnPawnArrivedToBench
    {
        public static void Prefix(JobDriver __instance)
        {
            if (__instance is null) return;

            Pawn pawn = __instance.GetActor();

            if (__instance is JobDriver_DoBill billDriver)
                PawnIsAtTable(billDriver, pawn);
            else if (__instance is JobDriver_Research researchDriver)
                PawnIsAtResearchBench(researchDriver, pawn);
        }

        //****************************************
        // Check if the pawn is actually at the
        // worktable, then activate it if it
        // is a valid table
        //****************************************
        private static void PawnIsAtTable(JobDriver_DoBill billDriver, Pawn pawn)
        {
            IBillGiver giver = billDriver.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
            if(giver is Building_WorkTable table)
            {
                if (!Tables.IsTable(table)) return;

                CompPowerTrader powerTrader = table.PowerComp as CompPowerTrader;
                if (powerTrader is null) return;

                if (pawn.Position == table.InteractionCell)
                    ActivateBench(billDriver, table);
            }
        }

        //****************************************
        // Check if the pawn is actually at the
        // research bench, then activate it if
        // it is a valid bench
        //****************************************
        private static void PawnIsAtResearchBench(JobDriver_Research researchDriver, Pawn pawn)
        {
            // why is the ResearchBench property private
            var bench = researchDriver.job.targetA.Thing as Building_ResearchBench;
            if (bench is null || !Tables.IsTable(bench)) return;

            CompPowerTrader powerTrader = bench.PowerComp as CompPowerTrader;
            if (powerTrader is null) return;

            // the pawn needs to be in the correct place
            if(pawn.Position == bench.InteractionCell)
                ActivateBench(researchDriver, bench);
        }

        //****************************************
        // Actually activate the power switching
        // on the bench/table
        //****************************************
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