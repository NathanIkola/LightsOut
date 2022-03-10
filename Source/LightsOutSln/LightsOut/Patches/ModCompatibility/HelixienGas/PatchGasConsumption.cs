//************************************************
// Patches the GasConsumption getter for
// CompGasTrader
//************************************************

using LightsOut.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using ModSettings = LightsOut.Boilerplate.ModSettings;
using Verse;

namespace LightsOut.Patches.ModCompatibility.HelixienGas
{
    public class PatchGasConsumption : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "CompGasTrader";
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;
        public override bool CaseSensitive => true;
        public override string ComponentName => "Patch GasConsumption for CompGasTrader";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PropertyInfo property = type.GetProperty("GasConsumption", BindingFlags);
            if (property is null)
                return new List<PatchInfo>();

            PatchInfo patch = new PatchInfo();
            patch.method = property.GetMethod;
            patch.patch = GetMethod<PatchGasConsumption>(nameof(GasConsumptionPatch));
            patch.patchType = PatchType.Postfix;

            return new List<PatchInfo>() { patch };
        }

        private static void GasConsumptionPatch(ThingComp __instance, ref float __result)
        {
            bool? canConsumeGas = Resources.CanConsumeResources(__instance);

            if (canConsumeGas == true)
            {
                if (Tables.IsTable(__instance.parent as Building))
                    __result *= ModSettings.ActivePowerDrawRate;
            }
            else if (canConsumeGas == false)
            {
                if (Common.Lights.CanBeLight(__instance.parent as Building))
                    __result = Resources.MinDraw;
                else
                    __result = Math.Min(__result * ModSettings.StandbyPowerDrawRate, Resources.MinDraw);
            }
        }
    }
}