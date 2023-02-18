namespace LightsOut.Patches.ModCompatibility.MiningLight
{
    /// <summary>
    /// Conditionally applies patches if the MiningCo. MiningHelmet mod is installed
    /// </summary>
    public class MiningLightCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "MiningHelmet";
        public override string TargetMod => "MiningCo. MiningHelmet";

        /// <summary>
        /// Adds the illegal light name required to stop the MiningLight
        /// from being captured as a light
        /// </summary>
        public override void OnAfterPatchApplied()
        {
            Common.Lights.AddIllegalLightName("MiningLight");
        }
    }
}