namespace LightsOut.Patches.ModCompatibility.SelfLitHydroponics
{
    /// <summary>
    /// Conditionally applies patches if the SelfLitHydroponics mod is installed
    /// </summary>
    public class SelfLitHydroponicsCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "SelfLitHydroponics";
        public override string TargetMod => "Self Lit Hydroponics";

        /// <summary>
        /// Adds the illegal light name required to stop the HydroponicsLight
        /// from being captured as a light
        /// </summary>
        public override void OnAfterPatchApplied()
        {
            Common.Lights.AddIllegalLightName("HydroponicsLight");
        }
    }
}