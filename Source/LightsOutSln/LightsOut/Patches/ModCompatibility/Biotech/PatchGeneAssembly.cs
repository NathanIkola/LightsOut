﻿using System;
using System.Collections.Generic;
using System.Linq;
using LightsOut.Common;
using LightsOut.Patches.Power;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchGeneAssembly : ICompatibilityPatchComponent<PatchGeneAssembly>
    {
        public override string ComponentName => "Patch for gene assembly related things";
        private const string PROCESSOR_ID = "GeneProcessor";

        private static IEnumerable<Building> AttachedGeneProcessors(Building_GeneAssembler assembler)
        {
            return assembler.ConnectedFacilities.Where(f => f.def.defName == PROCESSOR_ID).Select(t => (Building)t);
        }
        
        private static void OnStart(Building_GeneAssembler __instance)
        {
            Tables.EnableTable(__instance);
            foreach (var processor in AttachedGeneProcessors(__instance))
            {
                Tables.EnableTable(processor);
            }
        }
        
        private static void OnFinish(Building_GeneAssembler __instance)
        {
            Tables.DisableTable(__instance);
            foreach (var p in AttachedGeneProcessors(__instance))
            {
                Tables.DisableTable(p);
            }
        }

        private static void Post_IsTable(ThingWithComps __0, ref bool __result)
        {
            __result = __result || __0 is Building_GeneAssembler || (__0 as Building)?.def.defName == PROCESSOR_ID;
        }
        
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            var patches = new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<Building_GeneAssembler>(nameof(Building_GeneAssembler.Start)),
                    patch = GetMethod<PatchGeneAssembly>(nameof(OnStart)),
                    patchType = PatchType.Prefix
                },
                new PatchInfo
                {
                    method = GetMethod<Building_GeneAssembler>(nameof(Building_GeneAssembler.Finish)),
                    patch = GetMethod<PatchGeneAssembly>(nameof(OnFinish)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod(typeof(Tables), nameof(Tables.IsTable)),
                    patch = GetMethod<PatchGeneAssembly>(nameof(Post_IsTable)),
                    patchType = PatchType.Postfix,
                },
            };
            return patches;
        }
    }
}