using System.Collections.Generic;
using System.Reflection;
using LightsOut.Common;
using LightsOut.Patches.ModCompatibility.Ideology;
using LightsOut.Patches.Power;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class BiotechCompatibilityPatch : ICompatibilityPatch
    {
        
        public override string CompatibilityPatchName => "Biotech";
        public override string TargetMod => "Biotech";
        
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
        
        public static List<PatchInfo> CustomStandbyPatches(MethodInfo onStandby)
        {
            return new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = typeof(DisableBasePowerDrawOnGet).GetMethod(nameof(DisableBasePowerDrawOnGet.Postfix)),
                    patch = onStandby,
                    patchType = PatchType.Prefix
                },
                new PatchInfo
                {
                    method = typeof(AddStandbyInspectMessagePatch).GetMethod(nameof(AddStandbyInspectMessagePatch.Postfix)),
                    patch = onStandby,
                    patchType = PatchType.Prefix
                }
            };
        }
        
    }
}