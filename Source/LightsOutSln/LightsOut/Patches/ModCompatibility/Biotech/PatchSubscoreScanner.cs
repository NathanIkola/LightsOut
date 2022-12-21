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
            BiotechCompatibilityPatch.RegisterEnterableTableLike<Building_SubcoreScanner>();
            var patches = new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<Building_SubcoreScanner>(nameof(Building_SubcoreScanner.EjectContents)),
                    patch = GetMethod<BiotechCompatibilityPatch>(nameof(BiotechCompatibilityPatch.Post_OnOffDeactivate)),
                    patchType = PatchType.Postfix,
                },
            };
            return patches;
        }
    }
}