//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

namespace LightsOut.Patches.ModCompatibility.WallLights
{
    public class WallLightCompatibilityPatch : IModCompatibilityPatch
    {
        public WallLightCompatibilityPatch() : base()
        {
            new PatchIsInRoomForWallLights();
        }

        protected override string TypeNameToPatch { get => "WallLight"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }
    }
}