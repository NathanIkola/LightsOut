using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.GLWallLights
{
    /// <summary>
    /// Detects if Gloomy Furniture is installed and patches its Wall Lights if so
    /// </summary>
    public class GLWallLightCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "GL Wall Lights";
        public override string TargetMod => "GloomyFurniture";
        
        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchGetRoomForGLWallLights()
            };

            return components;
        }
    }
}
