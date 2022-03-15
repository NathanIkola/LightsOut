using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.ModGlowers
{
    /// <summary>
    /// Add support for modded glowers
    /// </summary>
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
