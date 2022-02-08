//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.Ideology
{
    public class IdeologyCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Ideology";
        public override string TargetMod => "Ideology";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchCount(),
                new PatchPostSpawnSetup(),
                new PatchTick()
            };

            return components;
        }
    }
}