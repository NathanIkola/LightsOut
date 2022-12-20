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
            return new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<DisableBasePowerDrawOnGet>(nameof(DisableBasePowerDrawOnGet.Postfix)),
                    patch = GetMethod<PatchMechGestator>(nameof(IsOnStandby)),
                    patchType = PatchType.Prefix
                },
                new PatchInfo
                {
                    method = GetMethod<AddStandbyInspectMessagePatch>(nameof(AddStandbyInspectMessagePatch.Postfix)),
                    patch = GetMethod<PatchMechCharger>(nameof(IsOnStandby)),
                    patchType = PatchType.Prefix
                },
            };
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