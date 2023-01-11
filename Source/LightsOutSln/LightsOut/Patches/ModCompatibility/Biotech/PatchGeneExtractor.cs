using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchGeneExtractor : ICompatibilityPatchComponent<PatchGeneExtractor>
    {
        public override string ComponentName => "Patches for gene extractors compatibility";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Enterables.RegisterEnterable(typeof(Building_GeneExtractor));
            var patches = new List<PatchInfo>
            {
                TablesHelper.OffPatch(GetMethod<Building_GeneExtractor>("Finish")),
                TablesHelper.OffPatch(GetMethod<Building_GeneExtractor>("Cancel")),
            };
            return patches;
        }
    }
}