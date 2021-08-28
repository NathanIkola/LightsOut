//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    public class AndroidsCompatibilityPatch : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "Building_AndroidPrinter"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            new PatchDisablePowerDraw();
            new PatchIsTable();
            new PatchInspectMessage();
            return base.GetPatches();
        }
    }
}