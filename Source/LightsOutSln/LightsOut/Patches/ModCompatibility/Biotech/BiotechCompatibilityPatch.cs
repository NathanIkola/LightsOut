using System.Collections.Generic;
using System.Reflection;
using LightsOut.Patches.ModCompatibility.Ideology;
using LightsOut.Patches.Power;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class BiotechCompatibilityPatch : ICompatibilityPatch
    {
        
        public override string CompatibilityPatchName => "Biotech";
        public override string TargetMod => "Biotech";
        
        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchMechCharger(),
                new PatchMechGestator(),
            };

            return components;
        }

        public static List<PatchInfo> CustomStandbyPatches(MethodInfo onStandby)
        {
            return new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = typeof(DisableBasePowerDrawOnGet).GetMethod(nameof(DisableBasePowerDrawOnGet.Postfix)),
                    patch = onStandby,
                    patchType = PatchType.Prefix
                },
                new PatchInfo
                {
                    method = typeof(AddStandbyInspectMessagePatch).GetMethod(nameof(AddStandbyInspectMessagePatch.Postfix)),
                    patch = onStandby,
                    patchType = PatchType.Prefix
                }
            };
        }
        
    }
}