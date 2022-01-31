//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    public class AndroidsCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Androids";
        public override string TargetMod => "Androids";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchDisablePowerDraw(),
                new PatchInspectMessage(),
                new PatchIsTable()
            };

            return components;
        }
    }
}