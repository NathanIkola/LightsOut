using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.ColonyManager
{
    /// <summary>
    /// Add support for the Colony Manager mod
    /// </summary>
    public class ColonyManagerCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Colony Manager";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>
            {
                new KeepAIManagerOn()
            };

            return components;
        }
    }
}