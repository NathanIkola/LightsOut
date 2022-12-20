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
                    patch = GetMethod<PatchMechGestator>(nameof(ShouldAdjustPower)),
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

        private static PropertyInfo GestatingMech;
        

        private static bool ShouldAdjustPower(CompPowerTrader __0)
        {
            return !IsGestator(__0?.parent) || !GestatorInUse(__0.parent);
        }
        
        private static bool IsOnStandby(CompPower __0)
        {
            return !IsGestator(__0?.parent) || !GestatorInUse(__0.parent);
        }

        private static bool IsGestator(ThingWithComps thing)
        {
            return thing?.GetType().Name == "Building_MechGestator";
        }
        
        private static bool GestatorInUse(ThingWithComps thing)
        {
            if (GestatingMech == null)
            {
                GestatingMech = thing.GetType().GetProperty("GestatingMech");
            }

            return (Pawn)GestatingMech.GetValue(thing) != null;
        }
    }
}