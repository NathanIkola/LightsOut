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
    public class PatchMechCharger : ICompatibilityPatchComponent<PatchMechCharger>
    {
        public override string ComponentName => "Patch for mech recharger power draw";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            return new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<DisableBasePowerDrawOnGet>(nameof(DisableBasePowerDrawOnGet.Postfix)),
                    patch = GetMethod<PatchMechCharger>(nameof(ShouldAdjustPower)),
                    patchType = PatchType.Prefix
                },
                new PatchInfo
                {
                    method = GetMethod(typeof(Tables), nameof(Tables.IsTable)),
                    patch = GetMethod<PatchMechCharger>(nameof(Post_IsTable)),
                    patchType = PatchType.Postfix
                },
                new PatchInfo
                {
                    method = GetMethod<AddStandbyInspectMessagePatch>(nameof(AddStandbyInspectMessagePatch.Postfix)),
                    patch = GetMethod<PatchMechCharger>(nameof(Pre_AddStandbyInspect)),
                    patchType = PatchType.Prefix
                },
            };
        }

        private static PropertyInfo IsAttachedToMech = null;

        /// <summary>
        /// returns true if comp is not a charger or if it is not in use
        /// </summary>
        /// <param name="__0"></param>
        /// <returns></returns>
        private static bool ShouldAdjustPower(CompPowerTrader __0)
        {
            return !IsCharger(__0?.parent) || !ChargerInUse(__0.parent);
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

        /// <summary>
        /// Returns false if given an in-use charger (skips standby adding the inspect label)
        /// </summary>
        /// <param name="__0"></param>
        /// <returns></returns>
        private static bool Pre_AddStandbyInspect(CompPower __0)
        {
            return !IsCharger(__0?.parent) || !ChargerInUse(__0.parent);
        }
    }
}