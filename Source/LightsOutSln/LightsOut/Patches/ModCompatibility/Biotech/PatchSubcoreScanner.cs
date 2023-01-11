using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchSubcoreScanner : ICompatibilityPatchComponent<Building_SubcoreScanner>
    {
        public override string ComponentName => "Patch for subcore scanners";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Enterables.RegisterEnterable(typeof(Building_SubcoreScanner));
            var patches = new List<PatchInfo>
            {
                TablesHelper.OffPatch(GetMethod<Building_SubcoreScanner>(nameof(Building_SubcoreScanner.EjectContents)))
            };
            return patches;
        }
    }
}