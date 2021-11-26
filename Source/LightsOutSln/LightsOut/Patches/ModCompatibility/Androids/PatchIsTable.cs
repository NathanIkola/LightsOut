//************************************************
// Stop the pawns from flicking the
// Android Printer off when they leave the room
//************************************************

using LightsOut.Patches.Power;
using LightsOut.Utility;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    public class PatchIsTable : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "ModResources"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }
        protected override string PatchName { get => "Androids Mod"; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            PatchInfo correctIsTablePatch = new PatchInfo();
            correctIsTablePatch.method = typeof(ModResources).GetMethod("IsTable", BindingFlags);
            correctIsTablePatch.patch = this.GetType().GetMethod("IsTablePatch", BindingFlags);
            correctIsTablePatch.patchType = PatchType.Postfix;

            return new List<PatchInfo>() { correctIsTablePatch };
        }

        private static void IsTablePatch(Building __0, ref bool __result)
        {
            if (__0 is null) return;
            if (__0.GetType().Name == "Building_AndroidPrinter")
                __result = true;
        }
    }
}