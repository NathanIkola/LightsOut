using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LightsOut.Patches.ModCompatibility.ExtinguishRefuelables
{
    /// <summary>
    /// Account for the fact that this mod completely wipes the existing
    /// campfire overlay
    /// </summary>
    public class DisallowCustomOverlay : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => nameof(Common.Tables);
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => false;
        public override string ComponentName => "Patch HasBlacklistedTableComp to account for custom overlay comp";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod(typeof(Common.Tables), "HasBlacklistedTableComp"),
                patch = GetMethod<DisallowCustomOverlay>(nameof(PostfixPatch)),
                patchType = PatchType.Postfix
            };

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Check for the FireOverlayExtinguishable comp
        /// </summary>
        /// <param name="__0">The Thing being tested</param>
        /// <param name="__result">The result of the existing check</param>
        private static void PostfixPatch(ThingWithComps __0, ref bool __result)
        {
            if (!__result && __0.AllComps.Any(comp => comp.GetType().Name.Contains("FireOverlayExtinguishable")))
            {
                __result = true;
            }
        }
    }
}