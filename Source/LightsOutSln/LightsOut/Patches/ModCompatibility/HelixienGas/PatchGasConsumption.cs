using LightsOut.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using LightsOutSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.ModCompatibility.HelixienGas
{
    /// <summary>
    /// Patches the GasConsumption getter for CompGasTrader
    /// </summary>
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

            PatchInfo patch = new PatchInfo
            {
                method = property.GetMethod,
                patch = GetMethod<PatchGasConsumption>(nameof(GasConsumptionPatch)),
                patchType = PatchType.Postfix
            };

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Checks if a gas-consuming building is able to consume power, and disable it if not
        /// </summary>
        /// <param name="__instance">The CompGasTrader to check</param>
        /// <param name="__result">The amount of gas this building pulls</param>
        private static void GasConsumptionPatch(ThingComp __instance, ref float __result)
        {
            bool? canConsumeGas = Resources.CanConsumeResources(__instance);

            if (canConsumeGas == true)
            {
                if (Tables.IsTable(__instance.parent))
                    __result *= LightsOutSettings.ActiveResourceDrawRate;
            }
            else if (canConsumeGas == false)
            {
                if (Common.Lights.CanBeLight(__instance.parent))
                    __result = Resources.MinDraw;
                else
                    __result = Math.Min(__result * LightsOutSettings.StandbyResourceDrawRate, Resources.MinDraw);
            }
        }
    }
}