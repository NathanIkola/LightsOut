//************************************************
// Adds support for modded glowers
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.ModGlowers
{
    public class ModGlowerCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Modded Glowers";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchShouldBeLitNow()
            };

            return components;
        }
    }
}
