using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LightsOut.Common;
using LightsOut.Patches.Power;
using RimWorld;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    /// <summary>
    /// Disable power draw on mech recharger if not in use by mech
    /// </summary>
    public class PatchMechRecharger : ICompatibilityPatchComponent<PatchMechRecharger>
    {
        public override string ComponentName => "Patch for mech recharger power draw";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Glowers.CompGlowers.Add(type);
            return new List<PatchInfo>{new PatchInfo{method = GetMethod<DisableBasePowerDrawOnGet>(nameof(DisableBasePowerDrawOnGet.Postfix)), patch = GetMethod<PatchMechRecharger>(nameof(Pre_DontAdjustPower)), patchType = PatchType.Prefix}};
        }

        private static FieldInfo IsAttachedToMech = null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="__0"></param>
        /// <returns></returns>
        private static bool Pre_DontAdjustPower(CompPowerTrader __0)
        {
            if (__0?.parent is null)
            {
                return true;
            }

            if (__0.parent.GetType().Name != "Building_MechRecharger")
            {
                return true;
            }

            if (IsAttachedToMech == null)
            {
                IsAttachedToMech = AccessTools.Field(__0.parent.GetType(), "IsAttachedToMech");
            }

            return (bool)IsAttachedToMech.GetValue(__0.parent);
        }
    }
}