namespace LightsOut.Patches.ModCompatibility.DubsBadHygiene
{
    public class DubsBadHygieneCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "DubsBadHygiene";
        public override string TargetMod => "Dubs Bad Hygiene";

        public override void OnAfterPatchApplied()
        {
            Common.Lights.AddIllegalLightName("CeilingFan");
        }
    }
}