using LightsOut.Common;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.QuestionableEthics
{
    /// <summary>
    /// The Pawn cloning vat isn't set up as a WorkTable, so make it count here
    /// </summary>
    public class PatchIsTable : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "Tables";
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;
        public override string ComponentName => "Patch IsTable to accept the Pawn Clone vat";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod(typeof(Tables), "IsTable"),
                patch = GetMethod<PatchIsTable>(nameof(Postfix)),
                patchType = PatchType.Postfix
            };

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Checks if the ThingWithComps is a Pawn cloning vat
        /// and fixes IsTable to recognize it
        /// </summary>
        /// <param name="__0">The ThingWithComps to check</param>
        /// <param name="__result">The result of IsTable</param>
        private static void Postfix(ThingWithComps __0, ref bool __result)
        {
            if (__0 is null) return;
            if (__0.GetType().Name == "Building_PawnVatGrower")
                __result = true;
        }
    }
}