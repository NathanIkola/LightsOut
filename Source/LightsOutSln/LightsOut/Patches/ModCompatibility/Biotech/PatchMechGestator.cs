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
            var patches = BiotechCompatibilityPatch.CustomOnOffPatches(
                GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_StartGestation)),
                GetMethod<Building_MechGestator>(nameof(Building_MechGestator.Notify_AllGestationCyclesCompleted)));
            return patches;
        }
    }
}