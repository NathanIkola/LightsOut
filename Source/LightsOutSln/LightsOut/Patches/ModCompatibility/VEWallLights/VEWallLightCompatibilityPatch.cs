//************************************************
// Detect if the WallLight mod is installed
// and perform the necessary compatibility
// patching if it is present
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.VEWallLights
{
    public class VEWallLightCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "VE Wall Lights";
        public override string TargetMod => "Vanilla Factions Expanded - Ancients";
        
        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchIsInRoomForVEWallLights()
            };

            return components;
        }
    }
}