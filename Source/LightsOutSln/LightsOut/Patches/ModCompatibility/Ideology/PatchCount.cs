using LightsOut.Common;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Ideology
{
    /// <summary>
    /// Detect if a ritual is ending. Count is fired at the end of a ritual.
    /// </summary>
    public class PatchCount : ICompatibilityPatchComponent<RitualOutcomeComp_NumActiveLoudspeakers>
    {
        public override string ComponentName => "Patch Count for RitualOutcomeComp_NumActiveLoudspeakers";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo prefix = new PatchInfo
            {
                method = GetMethod<RitualOutcomeComp_NumActiveLoudspeakers>(nameof(RitualOutcomeComp_NumActiveLoudspeakers.Count)),
                patch = GetMethod<PatchCount>(nameof(Prefix)),
                patchType = PatchType.Prefix
            };

            return new List<PatchInfo>() { prefix };
        }

        /// <summary>
        /// Check if any Loudspeakers are within range of the ritual, and if
        /// so disable them since the ritual is ending.
        /// </summary>
        /// <param name="__instance">The loudspeaker presence comp being checked</param>
        /// <param name="ritual">The ritual being performed</param>
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