//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.VEWallLights
{
    public class VEWallLightCompatibilityPatch : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "WallLight"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }
        protected override string PatchName { get => "VE Wall Lights Mod"; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            new PatchIsInRoomForVEWallLights();
            return base.GetPatches();
        }
    }
}