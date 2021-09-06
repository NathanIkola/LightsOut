using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightsOut.Patches.ModCompatibility.RimFridge
{
    public class RimFridgeCompatibilityPatch : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "RimFridge_Building"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            new PatchGetLightResources();
            return base.GetPatches();
        }
    }
}