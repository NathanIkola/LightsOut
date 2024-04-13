using LightsOut.Common;
using LightsOut.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Ideology
{
    /// <summary>
    /// Enable or disable speakers as they spawn in case
    /// they spawn in the middle of a ritual
    /// </summary>
    public class PatchPostSpawnSetup : ICompatibilityPatchComponent<CompLoudspeaker>
    {
        public override string ComponentName => "Patch PostPawnSetup for Loudspeaker";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo postfix = new PatchInfo
            {
                method = GetMethod<ThingComp>(nameof(ThingComp.PostSpawnSetup)),
                patch = GetMethod<PatchPostSpawnSetup>(nameof(Postfix)),
                patchType = PatchType.Postfix
            };

            return new List<PatchInfo>() { postfix };
        }

        private static void Postfix(ThingComp __instance)
        {
            if (__instance is CompLoudspeaker speaker)
            {
                speaker.parent.AllComps.Add(new LoudspeakerTurnOffComp(speaker.parent));
                if (!speaker.Active)
                    Tables.DisableTable(speaker.parent);
                else
                    Tables.EnableTable(speaker.parent);
            }
        }
    }
}