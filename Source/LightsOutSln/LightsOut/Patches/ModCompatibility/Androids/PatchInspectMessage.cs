using System;
using System.Collections.Generic;
using System.Reflection;
using LightsOut.Patches.Power;
using RimWorld;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    /// <summary>
    /// Fix the inspect message for the Android Printer
    /// </summary>
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

        /// <summary>
        /// Memoized version of the method for getting the status
        /// </summary>
        private static MethodInfo m_pawnCrafterStatus = null;

        /// <summary>
        /// Fixes the inspect message for the Android Printer
        /// </summary>
        /// <param name="__0">The CompPower to fix the message of</param>
        /// <returns><see langword="true"/> if the printer is not printing,
        /// <see langword="false"/> otherwise</returns>
        private static bool PostfixPatch(CompPower __0)
        {
            if (__0.parent.GetType().Name == "Building_AndroidPrinter")
            {
                if (m_pawnCrafterStatus is null)
                    m_pawnCrafterStatus = GetMethod(__0.parent.GetType(), "PawnCrafterStatus");

                int status = (int)m_pawnCrafterStatus.Invoke(__0.parent, null);

                // status of 2 is "Printing"
                if (status == 2) return false;
            }
            return true;
        }
    }
}