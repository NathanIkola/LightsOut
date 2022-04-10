using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.SelfLitHydroponics
{
    /// <summary>
    /// Conditionally applies patches if the SelfLitHydroponics mod is installed
    /// </summary>
    public class SelfLitHydroponicsCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "SelfLitHydroponics";
        public override string TargetMod => "Self Lit Hydroponics";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            return new List<ICompatibilityPatchComponent>();
        }

        public override void OnAfterPatchApplied()
        {
            Common.Lights.AddIllegalLightName("HydroponicsLight");
        }
    }
}