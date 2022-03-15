using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.VEWallLights
{
    /// <summary>
    /// Detects if VFE is installed and patches its Wall Lights if so
    /// </summary>
    public class VEWallLightCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "VE Wall Lights";
        public override string TargetMod => "Vanilla Factions Expanded - Ancients";
        
        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchGetRoomForVEWallLights()
            };

            return components;
        }
    }
}