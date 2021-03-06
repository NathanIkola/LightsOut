using LightsOut.Common;
using LightsOut.Patches.Lights;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace LightsOut.Patches.ModCompatibility.ModGlowers
{
    /// <summary>
    /// Patches ShouldBeLitNow for modded glowers
    /// </summary>
    public class PatchShouldBeLitNow : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "Glower";
        public override bool TargetsMultipleTypes => true;
        public override bool TypeNameIsExact => false;
        public override bool CaseSensitive => false;
        public override string ComponentName => "Patch ShouldBeLitNow for modded glowers";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            if (type.Assembly == Assembly.GetAssembly(typeof(Pawn)))
                return new List<PatchInfo>();

            // try and get our getters from the type passed in (it should either be PascalCase or camelCase)
            PropertyInfo getter = type.GetProperty("shouldBeLitNow", BindingFlags) ?? type.GetProperty("ShouldBeLitNow", BindingFlags);
            // if this type doesn't have it, return no patches
            if (getter is null)
                return new List<PatchInfo>();

            PatchInfo patch = new PatchInfo
            {
                method = getter.GetMethod,
                patch = GetMethod<DisableLightGlowPatch>(nameof(DisableLightGlowPatch.Postfix)),
                patchType = PatchType.Postfix
            };

            DebugLogger.LogInfo($"       patching \"{type.Namespace} - {type.Name}\" to count as a glower");
            // add it to the list of allowed glowers
            Glowers.CompGlowers.Add(type);

            return new List<PatchInfo>() { patch };
        }
    }
}