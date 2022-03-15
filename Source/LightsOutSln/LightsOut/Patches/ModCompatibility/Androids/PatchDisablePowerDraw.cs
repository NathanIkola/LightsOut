using LightsOut.Patches.Power;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LightsOut.Patches.ModCompatibility
{
    /// <summary>
    /// Disable the power draw on the Android Printer if it is NOT printing
    /// </summary>
    public class PatchDisablePowerDraw : ICompatibilityPatchComponent<PatchDisablePowerDraw>
    {
        public override string ComponentName => "Patch DisablePowerDraw for Android Printer";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo();
            patch.method = GetMethod<DisableBasePowerDrawOnGet>(nameof(DisableBasePowerDrawOnGet.Postfix));
            patch.patch = GetMethod<PatchDisablePowerDraw>(nameof(PrefixPatch));
            patch.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Memoized version of the method for getting the status
        /// </summary>
        private static MethodInfo m_pawnCrafterStatus = null;

        /// <summary>
        /// Memoized list of whether or not things are printers
        /// to speed up subsequent calls
        /// </summary>
        private static Dictionary<CompPowerTrader, bool> MemoizedIsPrinter { get; } = new Dictionary<CompPowerTrader, bool>();

        /// <summary>
        /// Checks if a CompPowerTrader belongs to a printer,
        /// and if so checks if it is currently printing.
        /// </summary>
        /// <param name="__0">The CompPowerTrader to check for</param>
        /// <returns><see langword="true"/> if the CompPowerTrader did not
        /// belong to a printer that was actively printing,
        /// <see langword="false"/> if it belongs to an actively printing printer</returns>
        private static bool PrefixPatch(CompPowerTrader __0)
        {
            if (__0 is null || __0.parent is null) return true;

            if (!MemoizedIsPrinter.ContainsKey(__0))
                MemoizedIsPrinter.Add(__0, __0.parent.GetType().Name == "Building_AndroidPrinter");

            if (!MemoizedIsPrinter[__0]) return true;

            if (m_pawnCrafterStatus is null)
                m_pawnCrafterStatus = GetMethod(__0.parent.GetType(), "PawnCrafterStatus");

            int status = (int)m_pawnCrafterStatus.Invoke(__0.parent, null);

            // status of 2 is "Printing"
            if (status == 2)
                return false;
            return true;
        }
    }
}