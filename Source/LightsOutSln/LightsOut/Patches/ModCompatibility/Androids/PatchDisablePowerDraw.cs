//************************************************
// Disable the power draw on the Android Printer
// if it is NOT printing
//************************************************

using LightsOut.Patches.Power;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace LightsOut.Patches.ModCompatibility
{
    public class PatchDisablePowerDraw : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "DisablePowerDrawPatch"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            PatchInfo disablePowerPatch = new PatchInfo();
            disablePowerPatch.method = typeof(DisablePowerDrawPatch).GetMethod("Postfix", BindingFlags);
            disablePowerPatch.patch = this.GetType().GetMethod("PostfixPatch", BindingFlags);
            disablePowerPatch.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { disablePowerPatch };
        }

        private static bool PostfixPatch(CompPowerTrader __0)
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