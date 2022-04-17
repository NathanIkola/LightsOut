using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.QuestionableEthics
{
    /// <summary>
    /// Conditionally applies patches if the Questionable Ethics mod is installed
    /// </summary>
    public class QECompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Questionable Ethics";
        public override string TargetMod => "Questionable Ethics Enhanced";

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