//************************************************
// Adds support for Helixien gas benches
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.HelixienGas
{
    public class HelixienGasCompatibilityPatch : ICompatibilityPatch
    {
        public override string CompatibilityPatchName => "Helixien Gas Buildings";
        public override string TargetMod => "Vanilla Furniture Expanded - Power";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchGasConsumption(),
                new PatchInspectMessage(),
                new PatchGetRoomForVPEGasWallLights()
            };

            return components;
        }
    }
}