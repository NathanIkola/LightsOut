using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchSubscoreScanner : ICompatibilityPatchComponent<PatchSubscoreScanner>
    {
        public override string ComponentName => "Patch for subscore scanners";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Tables.RegisterTable(typeof(Building_SubcoreScanner));
            var patches = new List<PatchInfo>
            {
                TablesHelper.OffPatch(GetMethod<Building_SubcoreScanner>(nameof(Building_SubcoreScanner.EjectContents)))
            };
            return patches;
        }
    }
}