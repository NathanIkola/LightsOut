using LightsOut.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace LightsOut.Patches.ModCompatibility.HelixienGas
{
    /// <summary>
    /// Patches the inspect message for gas resource traders
    /// </summary>
    public class PatchInspectMessage : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "CompGasTrader";
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;
        public override bool CaseSensitive => true;
        public override string ComponentName => "Patch CompInspectStringExtra for CompGasTrader";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            MethodInfo method = GetMethod(type, "CompInspectStringExtra");
            if (method is null)
                return new List<PatchInfo>();

            PatchInfo patch = new PatchInfo();
            patch.method = method;
            patch.patch = GetMethod<PatchInspectMessage>(nameof(InspectMessagePatch));
            patch.patchType = PatchType.Postfix;

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Replaces the return value of CompInspectStringExtra if this 
        /// resource trader is disabled
        /// </summary>
        /// <param name="__instance">The CompGasTrader being tested</param>
        /// <param name="__result">The string to be displayed in the inspect panel</param>
        private static void InspectMessagePatch(ThingComp __instance, ref string __result)
        {
            if (Resources.CanConsumeResources(__instance) == false)
                __result = "LightsOut_OnStandby".Translate();
        }
    }
}