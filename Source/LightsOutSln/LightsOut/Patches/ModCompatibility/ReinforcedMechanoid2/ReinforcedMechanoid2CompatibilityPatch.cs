using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.ReinforcedMechanoid2
{
    /// <summary>
    /// Add support for the Reinforced Mechanoid 2 and Gestalt Engine mods
    /// </summary>
    public class ReinforcedMechanoid2CompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Reinforced Mechanoid 2";

        public override string TargetMod => "Reinforced Mechanoid 2 (Continued)";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>
            {
                new KeepGestaltEngineOn()
            };

            return components;
        }
    }
}