//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.WallLights
{
    public class WallLightCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Wall Lights";
        public override string TargetMod => "Wall Light";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchIsInRoomForWallLights()
            };

            return components;
        }
    }
}