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
        public override string TypeNameToPatch => "Glow";
        public override bool TargetsMultipleTypes => true;
        public override bool TypeNameIsExact => false;
        public override bool CaseSensitive => false;
        public override string ComponentName => "Patch ShouldBeLitNow for modded glowers";

        /// <summary>
        /// Attempts to get the ShouldBeLitNow property specific to certain mods
        /// </summary>
        /// <param name="type">The type to try and pull it from</param>
        /// <returns>The property if found, null otherwise</returns>
        private PropertyInfo GetShouldBeLitNowModded(Type type)
        {
            PropertyInfo getter = type.GetProperty("shouldBeLitNow", BindingFlags); // ?? some irritating mod
            if (getter is null)
                getter = type.GetProperty("_ShouldBeLitNow", BindingFlags); // Dubs Bad Hygiene because they suck
            return getter;
        }

        /// <summary>
        /// Attempts to get the vanilla ShouldBeLitNow property
        /// </summary>
        /// <param name="type">The typ to try and pull it from</param>
        /// <returns>The property if found, null otherwise</returns>
        private PropertyInfo GetShouldBeLitNowVanilla(Type type)
        {
            return type.GetProperty("ShouldBeLitNow", BindingFlags);
        }

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            if (type.Assembly == Assembly.GetAssembly(typeof(Pawn)))
                return new List<PatchInfo>();

            PropertyInfo getter = GetShouldBeLitNowModded(type) ?? GetShouldBeLitNowVanilla(type);

            // if this type doesn't have it, return no patches
            if (getter is null)
                return new List<PatchInfo>();

            if (getter.DeclaringType != getter.ReflectedType)
            {
                DebugLogger.LogInfo($"       failed to patch \"{type.Namespace} - {type.Name}\" to count as a glower; it does not define any known ShouldBeLitNow properties", DebugMessageKeys.Mods);
                return new List<PatchInfo>();
            }

            PatchInfo patch = new PatchInfo
            {
                method = getter.GetMethod,
                patch = GetMethod<DisableLightGlowPatch>(nameof(DisableLightGlowPatch.Postfix)),
                patchType = PatchType.Postfix
            };

            // add it to the list of allowed glowers
            DebugLogger.LogInfo($"       patching \"{type.Namespace} - {type.Name}\" to count as a glower", DebugMessageKeys.Mods);
            Glowers.CompGlowers.Add(type);

            return new List<PatchInfo>() { patch };
        }
    }
}