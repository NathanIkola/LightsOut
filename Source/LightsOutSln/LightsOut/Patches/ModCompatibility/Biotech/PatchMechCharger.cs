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
            var patches = BiotechCompatibilityPatch.CustomOnOffPatches(GetMethod<Building_MechCharger>("StartCharging"),
                GetMethod<Building_MechCharger>("StopCharging"));
            
            patches.Add(
                new PatchInfo
                {
                    method = GetMethod(typeof(Tables), nameof(Tables.IsTable)),
                    patch = GetMethod<PatchMechCharger>(nameof(Post_IsTable)),
                    patchType = PatchType.Postfix
                }
            );
            patches.Add(
                new PatchInfo
                {
                    method = GetMethod(typeof(Building), nameof(Building.SpawnSetup)),
                    patch = GetMethod<PatchMechCharger>(nameof(AfterSpawn)),
                    patchType = PatchType.Postfix
                }
            );
            return patches;
        }

        private static PropertyInfo IsAttachedToMech = null;

        private static void AfterSpawn(Building __instance)
        {
            if (IsAttachedToMech == null)
            {
                IsAttachedToMech = AccessTools.Property(typeof(Building_MechCharger), "IsAttachedToMech");
            }

            if (__instance is Building_MechCharger ch && (bool)IsAttachedToMech.GetValue(ch))
            {
                Tables.EnableTable(__instance);
            }
        }

        private static bool IsCharger(ThingWithComps thing)
        {
            return thing is Building_MechCharger;
        }

        private static void Post_IsTable(ThingWithComps __0, ref bool __result) {
            __result = __result || IsCharger(__0);
        }
    }
}