using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class PatchWasteAtomiser : ICompatibilityPatchComponent<PatchWasteAtomiser>
    {
        public override string ComponentName => "Patch waste atomiser";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            var patches = new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<CompAtomizer>(nameof(CompAtomizer.CompTick)),
                    patch = GetMethod<PatchWasteAtomiser>(nameof(PostAtomizerTick)),
                    patchType = PatchType.Postfix,
                }
            };
            return patches;
        }

        private static void PostAtomizerTick(CompAtomizer __instance)
        {
            if (!(__instance.parent is Building_WastepackAtomizer inst))
            {
                return;
            }

            if (__instance.Empty)
            {
                Tables.DisableTable(inst);
            }
            else
            {
                Tables.EnableTable(inst);
            }
        }
    }
}