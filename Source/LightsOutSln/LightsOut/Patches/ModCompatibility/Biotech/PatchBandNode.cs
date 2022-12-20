using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchBandNode : ICompatibilityPatchComponent<PatchBandNode>
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

        private static void Post_IsTable(ThingWithComps __0, ref bool __result)
        {
            __result = __result || (__0 is Building && __0.TryGetComp<CompBandNode>() != null);
        }
    }
}