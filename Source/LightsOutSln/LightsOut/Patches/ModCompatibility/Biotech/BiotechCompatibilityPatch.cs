using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LightsOut.Common;
using LightsOut.Patches.ModCompatibility.Ideology;
using LightsOut.Patches.Power;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class BiotechCompatibilityPatch : ICompatibilityPatch
    {
        
        public override string CompatibilityPatchName => "Biotech";
        public override string TargetMod => "Biotech";

        private static List<Type> tableLikes = new List<Type>();

        public static void RegisterEnterableTableLike<T>()
        {
            tableLikes.Add(typeof(T));
        }

        private static void Post_IsTable(ThingWithComps __0, ref bool __result)
        {
            __result = __result || tableLikes.Any(t => __0.GetType().IsInstanceOfType(t));
        }

        private static void AfterSpawn(Building __instance)
        {
            if (__instance is Building_Enterable inst && tableLikes.Any(t => __instance.GetType().IsInstanceOfType(t)) && inst.SelectedPawn != null)
            {
                Tables.EnableTable(__instance);
            }
        }

        private class TableLikePatch : ICompatibilityPatchComponent<TableLikePatch>
        {
            public override string ComponentName => "TableLikes";
            public override IEnumerable<PatchInfo> GetPatches(Type type)
            {
                return new List<PatchInfo>
                {
                    IsTablePatch(GetMethod<BiotechCompatibilityPatch>(nameof(Post_IsTable))),
                    new PatchInfo
                    {
                        method = GetMethod<Building>(nameof(Building.SpawnSetup)),
                        patch = GetMethod<BiotechCompatibilityPatch>(nameof(AfterSpawn)),
                        patchType = PatchType.Postfix,
                    }
                };
            }
        }

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchMechCharger(),
                new PatchMechGestator(),
                new PatchGeneAssembly(),
                new PatchGrowthVat(),
                new PatchBandNode(),
                new PatchGeneExtractor(),
                new PatchSubscoreScanner(),
                new PatchWasteAtomiser(),
                new TableLikePatch(),
            };

            return components;
        }

        public static void Pre_OnOffActivate(ThingWithComps __instance)
        {
            Tables.EnableTable(__instance);
        }
        
        public static void Post_OnOffDeactivate(ThingWithComps __instance)
        {
            Tables.DisableTable(__instance);
        }
        
        public static PatchInfo IsTablePatch(MethodInfo post)
        {
            return new PatchInfo
            {
                method = typeof(Tables).GetMethod(nameof(Tables.IsTable)),
                patch = post,
                patchType = PatchType.Postfix
            };
        }

        public static List<PatchInfo> CustomOnOffPatches(MethodInfo activate, MethodInfo deactivate)
        {
            return new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = activate,
                    patch = typeof(BiotechCompatibilityPatch).GetMethod(nameof(Pre_OnOffActivate)),
                    patchType = PatchType.Prefix
                },
                new PatchInfo
                {
                    method = deactivate,
                    patch = typeof(BiotechCompatibilityPatch).GetMethod(nameof(Post_OnOffDeactivate)),
                    patchType = PatchType.Postfix,
                }
            };
        }
    }
}