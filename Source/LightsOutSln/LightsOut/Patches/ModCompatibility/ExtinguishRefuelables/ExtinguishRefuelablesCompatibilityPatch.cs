using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.ExtinguishRefuelables
{
    /// <summary>
    /// The compatibility patch for Extinguish Refuelables that accounts
    /// for the fact that they wipe out the normal campfire overlap comp
    /// </summary>
    public class ExtinguishRefuelablesCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "ExtinguishRefuelables";
        public override string TargetMod => "Extinguish Refuelables";
        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new DisallowCustomOverlay(),
            };

            return components;
        }
    }
}