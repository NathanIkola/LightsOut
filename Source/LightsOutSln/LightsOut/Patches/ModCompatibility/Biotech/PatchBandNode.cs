using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    /// <summary>
    /// Support for band nodes: Nodes will be on standby when unbound
    /// </summary>
    public class PatchBandNode : ICompatibilityPatchComponent<CompBandNode>
    {
        public override string ComponentName => "Patches for band node support";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            var patches = new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<CompBandNode>(nameof(CompBandNode.PostSpawnSetup)),
                    patch = GetMethod<PatchBandNode>(nameof(TunedToUpdated)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod<CompBandNode>(nameof(CompBandNode.TuneTo)),
                    patch = GetMethod<PatchBandNode>(nameof(TunedToUpdated)),
                    patchType = PatchType.Postfix,
                },
            };
            return patches;
        }

        private static void TunedToUpdated(CompBandNode __instance)
        {
            if (__instance.tunedTo == null && __instance.tuningTo == null)
            {
                Tables.DisableTable(__instance.parent);
            }
            else
            {
                Tables.EnableTable(__instance.parent);
            }
        }
    }
}