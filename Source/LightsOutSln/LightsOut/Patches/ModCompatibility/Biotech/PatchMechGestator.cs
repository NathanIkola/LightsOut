using System;
using System.Collections.Generic;
using System.Reflection;
using LightsOut.Common;
using LightsOut.Patches.Power;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchMechGestator : ICompatibilityPatchComponent<PatchMechGestator>
    {
        
        public override string ComponentName => "Patch for mech gestator power draw";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Tables.RegisterTable(typeof(Building_MechGestator));
            var patches = new List<PatchInfo>
            {
                TablesHelper.OnPatch(
                    GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_StartGestation))),
                TablesHelper.OffPatch(
                    GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_AllGestationCyclesCompleted)))
            };
            patches.Add(new PatchInfo
            {
                method = GetMethod<Building>(nameof(Building.SpawnSetup)),
                patch = GetMethod<PatchMechGestator>(nameof(AfterSpawn)),
                patchType = PatchType.Postfix,
            });
            return patches;
        }

        private static void AfterSpawn(Building __instance)
        {
            if (__instance is Building_MechGestator inst && inst.ActiveBill?.State == FormingCycleState.Forming)
            {
                Tables.EnableTable(__instance);
            }
        }
    }
}