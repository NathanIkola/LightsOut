//************************************************
// Conditionally patch the RimFridge to not turn
// off as a light (which I think is no longer
// actually necessary)
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.RimFridge
{
    public class RimFridgeCompatibilityPatch : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "RimFridge_Building"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }
        protected override string PatchName { get => "RimFridge Mod"; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            new PatchGetLightResources();
            return base.GetPatches();
        }
    }
}