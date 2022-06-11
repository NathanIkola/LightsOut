namespace LightsOut.Patches.ModCompatibility.ErinsJapaneseFurniture
{
    /// <summary>
    /// Detects if Erin's Japanese Furniture is installed and
    /// patches it if so
    /// </summary>
    public class ErinsJapaneseFurnitureCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Erin's Japanese Furniture";
        public override string TargetMod => "Erin's Japanese Furniture";

        /// <summary>
        /// Adds the legal light name to the Lights class
        /// </summary>
        public override void OnAfterPatchApplied()
        {
            Common.Lights.AddLightNameRequirement("Andon");
        }
    }
}