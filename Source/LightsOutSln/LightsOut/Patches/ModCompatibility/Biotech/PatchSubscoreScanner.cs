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
            var patches = new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<Building_Enterable>("SelectPawn"),
                    patch = GetMethod<PatchSubscoreScanner>(nameof(AfterSelectPawn)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod<Building_SubcoreScanner>(nameof(Building_SubcoreScanner.EjectContents)),
                    patch = GetMethod<BiotechCompatibilityPatch>(nameof(BiotechCompatibilityPatch.Post_OnOffDeactivate)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod<Building_SubcoreScanner>(nameof(Building_SubcoreScanner.SpawnSetup)),
                    patch = GetMethod<PatchSubscoreScanner>(nameof(AfterSpawn)),
                    patchType = PatchType.Postfix,
                },
                BiotechCompatibilityPatch.IsTablePatch(GetMethod<PatchSubscoreScanner>(nameof(Post_IsTable)))
            };
            return patches;
        }

        private static void AfterSpawn(Building_SubcoreScanner __instance)
        {
            if (__instance.Occupant != null)
            {
                Tables.EnableTable(__instance);
            }
            else
            {
                Tables.DisableTable(__instance);
            }
        }
        private static void Post_IsTable(ThingWithComps __0, ref bool __result)
        {
            __result = __result || __0 is Building_SubcoreScanner;
        }

        private static void AfterSelectPawn(Building_Enterable __instance)
        {
            if (__instance is Building_SubcoreScanner && __instance.SelectedPawn != null)
            {
                Tables.EnableTable(__instance);
            }
        }
    }
}