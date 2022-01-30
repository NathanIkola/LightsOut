//************************************************
// Disable the power draw on the Android Printer
// if it is NOT printing
//************************************************

using LightsOut.Patches.Power;
using LightsOut.Common;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace LightsOut.Patches.ModCompatibility
{
    public class PatchDisablePowerDraw : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "CompPowerTrader"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }
        protected override string PatchName { get => "Androids Mod"; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            PatchInfo disablePowerPatch = new PatchInfo();
            disablePowerPatch.method = typeof(DisableBasePowerDrawOnSet).GetMethod("Postfix", BindingFlags);
            disablePowerPatch.patch = this.GetType().GetMethod("PrefixPatch", BindingFlags);
            disablePowerPatch.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { disablePowerPatch };
        }

        private static MethodInfo m_pawnCrafterStatus = null;

        private static Dictionary<CompPowerTrader, bool> MemoizedIsPrinter { get; } = new Dictionary<CompPowerTrader, bool>();

        private static bool PrefixPatch(CompPowerTrader __0)
        {
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