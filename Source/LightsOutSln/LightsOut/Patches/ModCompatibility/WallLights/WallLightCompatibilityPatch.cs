using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.WallLights
{
    /// <summary>
    /// Detects if Wall Light is installed and patches it if so
    /// </summary>
    public class WallLightCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Wall Lights";
        public override string TargetMod => "Wall Light";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchGetRoomForWallLights()
            };

            return components;
        }
    }
}