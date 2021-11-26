//************************************************
// Fix the inspect message for the Android Printer
//************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LightsOut.Patches.ModCompatibility;
using LightsOut.Patches.Power;
using RimWorld;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    public class PatchInspectMessage : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch => "AddStandbyInspectMessagePatch";
        protected override bool TargetsMultipleTypes => false;
        protected override bool TypeNameIsExact => true;
        protected override string PatchName { get => "Androids Mod"; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            PatchInfo disablePowerPatch = new PatchInfo();
            disablePowerPatch.method = typeof(AddStandbyInspectMessagePatch).GetMethod("Postfix", BindingFlags);
            disablePowerPatch.patch = this.GetType().GetMethod("PostfixPatch", BindingFlags);
            disablePowerPatch.patchType = PatchType.Prefix;

            return new List<PatchInfo>() { disablePowerPatch };
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