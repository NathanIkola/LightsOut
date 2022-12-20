using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchGrowthVat : ICompatibilityPatchComponent<PatchGrowthVat>
    {
        public override string ComponentName => "Patch for biotech growth vat support";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            var patches = new List<PatchInfo>();
            patches.Add(

                new PatchInfo
                {
                    method = GetMethod<Building_GrowthVat>("OnStop"),
                    patch = GetMethod<BiotechCompatibilityPatch>(nameof(BiotechCompatibilityPatch.Post_OnOffDeactivate)),
                    patchType = PatchType.Postfix,
                }
            );
            patches.Add(
                new PatchInfo
                {
                    method = GetMethod<Building_GrowthVat>("TryGrowEmbryo"),
                    patch = GetMethod<PatchGrowthVat>(nameof(CheckTryEnable)),
                    patchType = PatchType.Postfix
                }
            );
            patches.Add(
                new PatchInfo
                {
                    method = GetMethod<Building_GrowthVat>("TryAcceptPawn"),
                    patch = GetMethod<PatchGrowthVat>(nameof(CheckTryEnable)),
                    patchType = PatchType.Postfix
                }
            );
            
            patches.Add(
                new PatchInfo
                {
                    method = GetMethod(typeof(Tables), "IsTable"),
                    patch = GetMethod<PatchGrowthVat>(nameof(Post_IsTable)),
                    patchType = PatchType.Postfix
                }
            );
            return patches;
        }

        private static void Post_IsTable(ThingWithComps __0, ref bool __result)
        {
            __result = __result || __0 is Building_GrowthVat;
        }

        private static void CheckTryEnable(Building_GrowthVat __instance)
        {
            if (!IsOnStandby(__instance.PowerComp))
            {
                Tables.EnableTable(__instance);
            }
        }

        private static bool IsOnStandby(CompPower __0)
        {
            return !(__0?.parent is Building_GrowthVat vat) || (vat.selectedEmbryo == null && vat.SelectedPawn == null);
        }
    }
}