//************************************************
// Fix the inspect message for the Android Printer
//************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using LightsOut.Patches.Power;
using RimWorld;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    public class PatchInspectMessage : ICompatibilityPatchComponent<AddStandbyInspectMessagePatch>
    {
        public override string ComponentName => "Patch AddStandbyInspectMessage for Android Printer";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo();
            patch.method = GetMethod<DisableBasePowerDrawOnGet>("Postfix");
            patch.patch = GetMethod<PatchDisablePowerDraw>("PrefixPatch");
            patch.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { patch };
        }

        private static bool PostfixPatch(CompPower __0)
        {
            if (__0.parent.GetType().Name == "Building_AndroidPrinter")
            {
                MethodInfo pawnCrafterStatus = GetMethod(__0.parent.GetType(), "PawnCrafterStatus");
                int status = (int)pawnCrafterStatus.Invoke(__0.parent, null);

                // status of 2 is "Printing"
                if (status == 2) return false;
            }
            return true;
        }
    }
}