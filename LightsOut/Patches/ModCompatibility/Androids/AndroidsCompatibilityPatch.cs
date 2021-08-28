//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

namespace LightsOut.Patches.ModCompatibility.Androids
{
    public class AndroidsCompatibilityPatch : IModCompatibilityPatch
    {
        public AndroidsCompatibilityPatch() : base()
        {
            new PatchDisablePowerDraw();
            new PatchIsTable();
            new PatchInspectMessage();
        }

        protected override string TypeNameToPatch { get => "Building_AndroidPrinter"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }
    }
}