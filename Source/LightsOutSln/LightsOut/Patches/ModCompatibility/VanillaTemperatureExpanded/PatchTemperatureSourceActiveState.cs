using LightsOut.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace LightsOut.Patches.ModCompatibility.VanillaTemperatureExpanded
{
    /// <summary>
    /// Extends Proxy Heat's active-state check with LightsOut's resource-consumption state.
    /// </summary>
    public class PatchTemperatureSourceActiveState : ICompatibilityPatchComponent
    {
        public override string ComponentName => "Patch Proxy Heat active state";
        public override string TypeNameToPatch => "CompTemperatureSource";
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PropertyInfo property = type.GetProperty("ShouldBeActive", BindingFlags);
            if (property?.GetMethod is null)
                return new List<PatchInfo>();

            return new List<PatchInfo>()
            {
                new PatchInfo
                {
                    method = property.GetMethod,
                    patch = GetMethod<PatchTemperatureSourceActiveState>(nameof(ShouldBeActivePostfix)),
                    patchType = PatchType.Postfix
                }
            };
        }

        /// <summary>
        /// Turns off a Proxy Heat source whenever LightsOut has put its parent on standby.
        /// A null state belongs to a building unmanaged by LightsOut and is left unchanged.
        /// </summary>
        private static void ShouldBeActivePostfix(ThingComp __instance, ref bool __result)
        {
            if (Resources.CanConsumeResources(__instance.parent) == false)
                __result = false;
        }
    }
}
