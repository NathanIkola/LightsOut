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
            var patches = BiotechCompatibilityPatch.CustomStandbyPatches(GetMethod<PatchMechGestator>(nameof(IsOnStandby)));
            patches.Add(new PatchInfo
            {
                method = GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_StartGestation)),
                patch = GetMethod<PatchMechGestator>(nameof(Pre_StartGestation)),
                patchType = PatchType.Prefix,
            });

            patches.Add(new PatchInfo
            {
                method = GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_AllGestationCyclesCompleted)),
                patch = GetMethod<PatchMechGestator>(nameof(Post_GestationDone)),
                patchType = PatchType.Postfix,
            });
            return patches;
        }

        private static void Pre_StartGestation(Building_MechGestator __instance)
        {
            Tables.EnableTable(__instance);
        }
        
        private static void Post_GestationDone(Building_MechGestator __instance)
        {
            Tables.DisableTable(__instance);
        }

        private static bool IsOnStandby(CompPower __0)
        {
            return !IsGestator(__0?.parent) || !GestatorInUse(__0.parent);
        }

        private static bool IsGestator(ThingWithComps thing)
        {
            return thing is Building_MechGestator;
        }
        
        private static bool GestatorInUse(ThingWithComps thing)
        {
            var state = ((Building_MechGestator)thing).ActiveBill?.State;
            return state == FormingCycleState.Forming;
        }
    }
}