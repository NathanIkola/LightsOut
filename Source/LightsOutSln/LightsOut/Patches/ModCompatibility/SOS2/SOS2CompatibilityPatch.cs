using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.SOS2
{
    /// <summary>
    /// Detects if SOS2 is installed and patches it if so
    /// </summary>
    public class SOS2CompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "SOS2";
        public override string TargetMod => "Save Our Ship 2";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new FixLightsAfterMovingShip(),
            };

            return components;
        }
    }
}
