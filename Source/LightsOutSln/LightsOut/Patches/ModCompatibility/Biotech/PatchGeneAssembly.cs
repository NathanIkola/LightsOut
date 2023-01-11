using System;
using System.Collections.Generic;
using System.Linq;
using LightsOut.Common;
using LightsOut.Patches.Power;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    /// <summary>
    /// Support for gene assembly buildings
    /// </summary>
    public class PatchGeneAssembly : ICompatibilityPatchComponent<Building_GeneAssembler>
    {
        public override string ComponentName => "Patch for gene assembly related things";
        private const string GeneProcessorDefName = "GeneProcessor";

        private static IEnumerable<Building> AttachedGeneProcessors(Building_GeneAssembler assembler)
        {
            return assembler.ConnectedFacilities.Where(f => f.def.defName == GeneProcessorDefName).Select(t => (Building)t);
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
        
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            Tables.RegisterTable(typeof(Building_GeneAssembler));
            Tables.RegisterTable(GeneProcessorDefName);
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
            };
            return patches;
        }
    }
}