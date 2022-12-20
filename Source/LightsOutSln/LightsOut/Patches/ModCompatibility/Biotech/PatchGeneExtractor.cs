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
            var patches = new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<Building_GeneExtractor>("SelectPawn"),
                    patch = GetMethod<PatchGeneExtractor>(nameof(AfterSelectPawn)),
                    patchType = PatchType.Postfix,
                },
                
                new PatchInfo
                {
                    method = GetMethod<Building_GeneExtractor>("Finish"),
                    patch = GetMethod<PatchGeneExtractor>(nameof(AfterStop)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod<Building_GeneExtractor>("Cancel"),
                    patch = GetMethod<PatchGeneExtractor>(nameof(AfterStop)),
                    patchType = PatchType.Postfix,
                },
                
                new PatchInfo
                {
                    method = GetMethod<Building>(nameof(Building.SpawnSetup)),
                    patch = GetMethod<PatchGeneExtractor>(nameof(AfterSpawn)),
                    patchType = PatchType.Postfix,
                },
                BiotechCompatibilityPatch.IsTablePatch(GetMethod<PatchGeneExtractor>(nameof(Post_IsTable))),
            };
            return patches;
        }

        private static void Post_IsTable(ThingWithComps __0, ref bool __result)
        {
            __result = __result || __0 is Building_GeneExtractor;
        }

        private static void AfterStop(Building_GeneExtractor __instance)
        {
            Tables.DisableTable(__instance);
        }
        
        private static void AfterSpawn(Building __instance)
        {
            if (__instance is Building_GeneExtractor inst && inst.SelectedPawn != null)
            {
                Tables.EnableTable(__instance);
            }
        }

        private static void AfterSelectPawn(Building_GeneExtractor __instance)
        {
            if (__instance.SelectedPawn == null)
            {
                Tables.DisableTable(__instance);
            }
            else
            {
                Tables.EnableTable(__instance);
            }
        }
    }
}