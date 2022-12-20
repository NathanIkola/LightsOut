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
            var patches =
                BiotechCompatibilityPatch.CustomStandbyPatches(GetMethod<PatchMechCharger>(nameof(IsOnStandby)));
            patches.Add(
                new PatchInfo
                {
                    method = GetMethod(typeof(Tables), nameof(Tables.IsTable)),
                    patch = GetMethod<PatchMechCharger>(nameof(Post_IsTable)),
                    patchType = PatchType.Postfix
                }
            );
            patches.AddRange(BiotechCompatibilityPatch.CustomOnOffPatches(GetMethod<Building_MechCharger>("StartCharging"),GetMethod<Building_MechCharger>("StopCharging")));
            return patches;
        }

        private static PropertyInfo IsAttachedToMech = null;

        private static bool IsCharger(ThingWithComps thing)
        {
            return thing is Building_MechCharger;
        }

        private static bool ChargerInUse(ThingWithComps thing)
        {
            if (IsAttachedToMech == null)
            {
                IsAttachedToMech = typeof(Building_MechCharger).GetProperty("IsAttachedToMech",
                    BindingFlags.NonPublic | BindingFlags.Instance);
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
        private static bool IsOnStandby(CompPower __0)
        {
            return !IsCharger(__0?.parent) || !ChargerInUse(__0.parent);
        }
    }
}