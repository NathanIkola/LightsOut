using LightsOut.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LightsOut.Patches.ModCompatibility.ReinforcedMechanoid2
{
    /// <summary>
    /// Ensures that the Gestalt Engine does not get turned off (since it's not manned)
    /// </summary>
    public class KeepGestaltEngineOn : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => nameof(Tables);
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;
        public override string ComponentName => "Patch IsTable to ignore Gestalt Engine";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod(typeof(Tables), nameof(Tables.IsTable)),
                patch = GetMethod<KeepGestaltEngineOn>(nameof(Postfix)),
                patchType = PatchType.Postfix
            };

            return new List<PatchInfo>() { patch };
        }

        public static void Postfix(ThingWithComps thing, ref bool __result)
        {
            if (__result)
                __result = !thing.def.defName.Contains("RM_GestaltEngine");
        }
    }
}