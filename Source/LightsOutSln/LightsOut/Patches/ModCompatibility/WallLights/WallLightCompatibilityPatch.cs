//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.WallLights
{
    public class WallLightCompatibilityPatch : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "WallLight"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            new PatchIsInRoomForWallLights();
            return base.GetPatches();
        }
    }
}