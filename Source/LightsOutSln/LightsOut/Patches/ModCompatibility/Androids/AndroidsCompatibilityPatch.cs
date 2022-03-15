using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    /// <summary>
    /// Conditionally applies patches if the Androids mod is installed
    /// </summary>
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