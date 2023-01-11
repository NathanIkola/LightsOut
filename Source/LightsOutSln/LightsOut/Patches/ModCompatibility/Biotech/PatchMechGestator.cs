using System;
using System.Collections.Generic;
using System.Reflection;
using LightsOut.Common;
using LightsOut.Patches.Power;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    /// <summary>
    /// Support for the mech gestator
    /// </summary>
    public class PatchMechGestator : ICompatibilityPatchComponent<Building_MechGestator>
    {
        
        public override string ComponentName => "Patch for mech gestator power draw";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            var patches = new List<PatchInfo>
            {
                TablesHelper.OnPatch(
                    GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_StartGestation))),
                TablesHelper.OffPatch(
                    GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_AllGestationCyclesCompleted))),
                TablesHelper.OffPatch(
                    GetMethod<Building_MechGestator>(nameof(Building_MechGestator.EjectContentsAndRemovePawns)))
            };
            patches.Add(new PatchInfo
            {
                method = GetMethod<Building>(nameof(Building.SpawnSetup)),
                patch = GetMethod<PatchMechGestator>(nameof(AfterSpawn)),
                patchType = PatchType.Postfix,
            });
            
            return patches;
        }


        private static void UpdateWorking(Building_MechGestator gestator)
        {
            var consumes = Resources.CanConsumeResources(gestator) ?? true;
            if (gestator.ActiveBill?.State == FormingCycleState.Forming && !consumes)
            {
                Tables.EnableTable(gestator);
            }
            else if (consumes)
            {
                Tables.DisableTable(gestator);
            }
        }

        private static void AfterSpawn(Building __instance)
        {
            if (__instance is Building_MechGestator inst)
            {
                UpdateWorking(inst);
            }
        }
    }
}