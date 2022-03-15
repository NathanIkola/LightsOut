using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using LightsOut.Common;

namespace LightsOut.Patches.ModCompatibility.Ideology
{
    /// <summary>
    /// Checks every 30 ticks if a new Loudspeaker gets
    /// added to the ritual. Catches Loudspeakers if they are built
    /// during a ritual for some reason.
    /// </summary>
    public class PatchTick : ICompatibilityPatchComponent<RitualOutcomeComp_NumActiveLoudspeakers>
    {
        public override string ComponentName => "Patch Tick for RitualOutcomeComp_NumActiveLoudspeakers";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo prefix = new PatchInfo();
            prefix.method = GetMethod<RitualOutcomeComp_NumActiveLoudspeakers>(nameof(RitualOutcomeComp_NumActiveLoudspeakers.Tick));
            prefix.patch = GetMethod<PatchTick>(nameof(Prefix));
            prefix.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { prefix };
        }

        /// <summary>
        /// Enables all Loudspeakers in-range every 30 ticks
        /// </summary>
        /// <param name="__instance">The Loudspeaker comp to check</param>
        /// <param name="ritual">The currently running ritual</param>
        private static void Prefix(RitualOutcomeComp_NumActiveLoudspeakers __instance, LordJob_Ritual ritual)
        {
            if (_tick++ != 30)
                return;

            _tick = 0;
            TargetInfo selectedTarget = ritual.selectedTarget;
            if (selectedTarget.ThingDestroyed || !selectedTarget.HasThing)
                return;

            foreach (Thing thing in selectedTarget.Map.listerBuldingOfDefInProximity.GetForCell(
                selectedTarget.Cell, (float)__instance.maxDistance, ThingDefOf.Loudspeaker))
                Tables.EnableTable(thing as Building);
        }

        private static int _tick = 0;
    }
}