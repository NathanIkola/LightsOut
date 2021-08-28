using LightsOut.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut.Patches.ModCompatibility.RimFridge
{
    public class PatchGetLightResources : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch => "ModResources";
        protected override bool TargetsMultipleTypes => false;
        protected override bool TypeNameIsExact => true;

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            PatchInfo correctLightResource = new PatchInfo();
            correctLightResource.method = typeof(ModResources).GetMethod("GetLightResources", BindingFlags);
            correctLightResource.patch = this.GetType().GetMethod("Prefix", BindingFlags);
            correctLightResource.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { correctLightResource };
        }

        private static bool Prefix(Building __0)
        {
            if (__0?.GetType().Name.Contains("RimFridge") ?? false)
                return false;
            return true;
        }
    }
}