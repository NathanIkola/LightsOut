using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.VanillaTemperatureExpanded
{
    /// <summary>
    /// Makes Proxy Heat respect LightsOut standby state.
    /// </summary>
    public class VanillaTemperatureExpandedCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Vanilla Temperature Expanded";
        public override string TargetMod => "Vanilla Temperature Expanded";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            return new List<ICompatibilityPatchComponent>()
            {
                new PatchTemperatureSourceActiveState()
            };
        }
    }
}
