using System.Collections.Generic;
using LightsOut.Patches.ModCompatibility.Ideology;

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
                new PatchMechCharger()
            };

            return components;
        }
        
    }
}