//************************************************
// Patches the inspect message of the gas
// resource traders
//************************************************

using LightsOut.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace LightsOut.Patches.ModCompatibility.HelixienGas
{
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

        private static void InspectMessagePatch(ThingComp __instance, ref string __result)
        {
            if (Resources.CanConsumeResources(__instance) == false)
                __result = "On Standby";
        }
    }
}