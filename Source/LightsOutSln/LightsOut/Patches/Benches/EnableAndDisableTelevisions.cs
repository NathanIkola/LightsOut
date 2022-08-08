using HarmonyLib;
using LightsOut.Common;
using RimWorld;
using Verse;
using Verse.AI;

namespace LightsOut.Patches.Benches
{
    [HarmonyPatch(typeof(JobDriver_WatchBuilding))]
    [HarmonyPatch(nameof(JobDriver_WatchBuilding.TryMakePreToilReservations))]
    public class EnableAndDisableTelevisions
    {
        /// <summary>
        /// If we successfully made pre-toil reservations, turn the TV on, and turn it off
        /// afterward if nobody else is watching
        /// </summary>
        /// <param name="__instance">The JobDriver_WatchBuilding being started</param>
        /// <param name="__result">Whether or not it should be starting</param>
        public static void Postfix(JobDriver_WatchBuilding __instance, bool __result)
        {
            if (__result && __instance is JobDriver_WatchTelevision)
            {
                if (!(__instance.job?.GetTarget(TargetIndex.A).Thing is ThingWithComps tv))
                    return;

                Pawn pawn = __instance.pawn;
                if (pawn is null)
                    return;

                // if the pawn hasn't arrived yet, then the NotifyPatherArrived patch will take care of it
                if (pawn.Position != __instance.job.GetTarget(TargetIndex.B).Cell)
                    return;

                // otherwise the NotifyPatherArrived patch doesn't fire, so this will take care of it
                Tables.EnableTable(tv);
                __instance.AddFinishAction(() =>
                {
                    if (!Tables.IsAnyoneElseWatching(tv, __instance.GetActor()))
                        Tables.DisableTable(tv);
                });
            }
        }
    }
}