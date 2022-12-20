using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LightsOut.Common;
using LightsOut.Patches.Power;
using RimWorld;
using Verse;

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
            return new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<DisableBasePowerDrawOnGet>(nameof(DisableBasePowerDrawOnGet.Postfix)),
                    patch = GetMethod<PatchMechRecharger>(nameof(Pre_DontAdjustPower)),
                    patchType = PatchType.Prefix
                },
                new PatchInfo
                {
                    method = GetMethod(typeof(Tables), nameof(Tables.IsTable)),
                    patch = GetMethod<PatchMechRecharger>(nameof(Post_IsTable)),
                    patchType = PatchType.Postfix
                },
                new PatchInfo
                {
                    method = GetMethod<AddStandbyInspectMessagePatch>(nameof(AddStandbyInspectMessagePatch.Postfix)),
                    patch = GetMethod<PatchMechRecharger>(nameof(Pre_AddStandbyInspect)),
                    patchType = PatchType.Prefix
                },
            };
        }

        private static PropertyInfo IsAttachedToMech = null;

        /// <summary>
        ///
        /// </summary>
        /// <param name="__0"></param>
        /// <returns></returns>
        private static bool Pre_DontAdjustPower(CompPowerTrader __0)
        {
            if (__0?.parent == null || !IsCharger(__0.parent))
            {
                return true;
            }
            return ChargerInUse(__0.parent);
        }

        private static bool IsCharger(ThingWithComps thing)
        {
            return thing?.GetType().Name == "Building_MechCharger";
        }

        private static bool ChargerInUse(ThingWithComps thing)
        {
            if (IsAttachedToMech == null)
            {
                IsAttachedToMech = thing.GetType().GetProperty("IsAttachedToMech", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (bool)IsAttachedToMech.GetValue(thing);
        }

        private static void Post_IsTable(ThingWithComps __0, ref bool __result)
        {
            __result = __result || IsCharger(__0);
        }

        private static bool Pre_AddStandbyInspect(ThingWithComps __0)
        {
            if (!IsCharger(__0))
            {
                return true;
            }
            return ChargerInUse(__0);
        }
    }
}