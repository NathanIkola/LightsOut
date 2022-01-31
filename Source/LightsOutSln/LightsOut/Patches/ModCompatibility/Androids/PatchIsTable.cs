﻿//************************************************
// Stop the pawns from flicking the
// Android Printer off when they leave the room
//************************************************

using LightsOut.Common;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Androids
{
    public class PatchIsTable : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "Tables";
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;
        public override string ComponentName => "Patch IsTable to accept the Android Printer";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo();
            patch.method = GetMethod(typeof(Tables), "IsTable");
            patch.patch = GetMethod<PatchIsTable>("Postfix");
            patch.patchType = PatchType.Postfix;

            return new List<PatchInfo>() { patch };
        }

        private static void Postfix(Building __0, ref bool __result)
        {
            if (__0 is null) return;
            if (__0.GetType().Name == "Building_AndroidPrinter")
                __result = true;
        }
    }
}