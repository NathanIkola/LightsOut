using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PatchMechCharger : ICompatibilityPatchComponent<Building_MechCharger>
    {
        public override string ComponentName => "Patch for mech recharger power draw";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Tables.RegisterTable(typeof(Building_MechCharger));
            var patches = new List<PatchInfo>
            {
                TablesHelper.OnPatch(GetMethod<Building_MechCharger>("StartCharging")),
                TablesHelper.OffPatch(
                    GetMethod<Building_MechCharger>("StopCharging"))
            };
            
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

        /// <summary>
        /// Extra check on startup to reenable the charger on spawn if a mech is currently using it
        /// </summary>
        /// <param name="__instance"></param>
        private static void AfterSpawn(Building __instance)
        {
            if (!(__instance is Building_MechCharger ch))
            {
                return;
            }
            if (IsAttachedToMech == null)
            {
                IsAttachedToMech = AccessTools.Property(typeof(Building_MechCharger), "IsAttachedToMech");
            }

            if ((bool)IsAttachedToMech.GetValue(ch))
            {
                Tables.EnableTable(__instance);
            }
        }
    }
}