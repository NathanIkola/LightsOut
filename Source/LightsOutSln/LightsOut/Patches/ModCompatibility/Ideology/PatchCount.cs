//************************************************
// Detect if a ritual is ending
//************************************************

using LightsOut.Common;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Ideology
{
    public class PatchCount : ICompatibilityPatchComponent<RitualOutcomeComp_NumActiveLoudspeakers>
    {
        public override string ComponentName => "Patch Count for RitualOutcomeComp_NumActiveLoudspeakers";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo prefix = new PatchInfo();
            prefix.method = GetMethod<RitualOutcomeComp_NumActiveLoudspeakers>(nameof(RitualOutcomeComp_NumActiveLoudspeakers.Count));
            prefix.patch = GetMethod<PatchCount>(nameof(Prefix));
            prefix.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { prefix };
        }

        private static void Prefix(RitualOutcomeComp_NumActiveLoudspeakers __instance, LordJob_Ritual ritual)
        {
            TargetInfo selectedTarget = ritual.selectedTarget;
            if (selectedTarget.ThingDestroyed || !selectedTarget.HasThing)
                return;

            foreach (Thing thing in selectedTarget.Map.listerBuldingOfDefInProximity.GetForCell(
                selectedTarget.Cell, (float)__instance.maxDistance, ThingDefOf.Loudspeaker))
                Tables.DisableTable(thing as Building);
        }
    }
}