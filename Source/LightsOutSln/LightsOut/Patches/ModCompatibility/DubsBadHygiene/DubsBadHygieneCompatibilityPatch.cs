namespace LightsOut.Patches.ModCompatibility.DubsBadHygiene
{
    /// <summary>
    /// Detects if Dubs Bad Hygiene is installed and patches it if so
    /// </summary>
    public class DubsBadHygieneCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "DubsBadHygiene";
        public override string TargetMod => "Dubs Bad Hygiene";

        /// <summary>
        /// Add "CeilingFan" to the list of illegal phrases for def names
        /// </summary>
        public override void OnAfterPatchApplied()
        {
            Common.Lights.AddIllegalLightName("CeilingFan");
        }
    }
}