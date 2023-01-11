using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    /// <summary>
    /// Support for the growth vat
    /// </summary>
    public class PatchGrowthVat : ICompatibilityPatchComponent<Building_GrowthVat>
    {
        public override string ComponentName => "Patch for biotech growth vat support";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Tables.RegisterTable(typeof(Building_GrowthVat));
            var patches = new List<PatchInfo>();
            patches.Add(
                TablesHelper.OffPatch(GetMethod<Building_GrowthVat>("OnStop"))
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
            
            return patches;
        }

        private static void CheckTryEnable(Building_GrowthVat __instance)
        {
            if (!IsOnStandby(__instance))
            {
                Tables.EnableTable(__instance);
            }
        }

        private static bool IsOnStandby(Building_GrowthVat vat)
        {
            return vat.selectedEmbryo == null && vat.SelectedPawn == null;
        }
    }
}