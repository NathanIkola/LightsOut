using System;
using System.Collections.Generic;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;
using System.Reflection;
using HarmonyLib;
using LightsOut.Patches.ModCompatibility.Androids;
using LightsOut.Patches.ModCompatibility.VEWallLights;
using LightsOut.Patches.ModCompatibility.WallLights;
using LightsOut.Patches.ModCompatibility.ModGlowers;
using LightsOut.Patches.ModCompatibility.Ideology;
using LightsOut.Patches.ModCompatibility.HelixienGas;
using LightsOut.Common;
using LightsOut.Patches.ModCompatibility.SelfLitHydroponics;
using LightsOut.Patches.ModCompatibility.QuestionableEthics;
using LightsOut.Patches.ModCompatibility.ErinsJapaneseFurniture;

namespace LightsOut.Patches.ModCompatibility
{
    /// <summary>
    /// The class that is in charge of applying compatibility patches
    /// </summary>
    public static class ModCompatibilityManager
    {
        /// <summary>
        /// The list of compatibility patches to apply
        /// </summary>
        static readonly List<ICompatibilityPatch> CompatibilityPatches = new List<ICompatibilityPatch>()
        {
            new AndroidsCompatibilityPatch(),
            new VEWallLightCompatibilityPatch(),
            new WallLightCompatibilityPatch(),
            new ModGlowerCompatibilityPatch(),
            new IdeologyCompatibilityPatch(),
            new HelixienGasCompatibilityPatch(),
            new SelfLitHydroponicsCompatibilityPatch(),
            new QECompatibilityPatch(),
            new ErinsJapaneseFurnitureCompatibilityPatch(),
        };

        /// <summary>
        /// The method that actually iterates over the patches and applies them
        /// </summary>
        public static void LoadCompatibilityPatches()
        {
            foreach (ICompatibilityPatch patch in CompatibilityPatches)
            {
                ApplyPatch(patch);
            }
        }

        /// <summary>
        /// Applies a whole, single compatibility patch
        /// </summary>
        /// <param name="patch">The patch to apply</param>
        private static void ApplyPatch(ICompatibilityPatch patch)
        {
            if (string.IsNullOrWhiteSpace(patch.CompatibilityPatchName))
            {
                DebugLogger.LogWarning($"encountered compatibility patch with empty name; skipping it.", DebugMessageKeys.Mods);
                return;
            }

            // only load this patch if the target mod is present and accounted for
            if (!string.IsNullOrEmpty(patch.TargetMod))
            {
                if (!LoadedModManager.RunningModsListForReading.Any(x => x.Name == patch.TargetMod))
                    return;
            }

            DebugLogger.LogInfo($"applying mod compatibility patch: {patch.CompatibilityPatchName}", DebugMessageKeys.Mods);
            patch.OnBeforePatchApplied();
            foreach (ICompatibilityPatchComponent component in patch.GetComponents())
            {
                component.OnBeforeComponentApplied();
                ApplyPatchComponent(component);
                component.OnAfterComponentApplied();
            }
            patch.OnAfterPatchApplied();
        }

        /// <summary>
        /// Applies a single compatibility component
        /// </summary>
        /// <param name="comp">The component to apply</param>
        private static void ApplyPatchComponent(ICompatibilityPatchComponent comp)
        {
            if (string.IsNullOrWhiteSpace(comp.ComponentName))
            {
                DebugLogger.LogWarning("encountered a compatibility component with an empty name; skipping it.", DebugMessageKeys.Mods);
                return;
            }

            if (string.IsNullOrWhiteSpace(comp.TypeNameToPatch))
            {
                DebugLogger.LogWarning($"encountered a compatibility component ({comp.ComponentName}) with an empty type to patch; skipping it.", DebugMessageKeys.Mods);
                return;
            }

            bool wasApplied = false;

            // get all patchable types from loaded mods
            IEnumerable<Type> typesToPatch = GetTypesToPatch();

            foreach (Type type in typesToPatch)
            {
                // rule out types
                if (comp.TypeNameIsExact && !type.Name.Equals(comp.TypeNameToPatch))
                    continue;
                else if (comp.CaseSensitive && !type.Name.Contains(comp.TypeNameToPatch))
                    continue;
                else if (!comp.CaseSensitive && !type.Name.ToLower().Contains(comp.TypeNameToPatch.ToLower()))
                    continue;

                if (!wasApplied)
                    DebugLogger.LogInfo($"    component applied: {comp.ComponentName}", DebugMessageKeys.Mods);
                wasApplied = true;

                foreach (PatchInfo patch in comp.GetPatches(type))
                {
                    if (patch.method is null && patch.methodName is null)
                    {
                        DebugLogger.LogWarning($"    encountered a component with a null method; skipping it.", DebugMessageKeys.Mods);
                        continue;
                    }

                    if (patch.patch is null)
                    {
                        DebugLogger.LogWarning($"    encountered a component with a null patch; skipping it.", DebugMessageKeys.Mods);
                        continue;
                    }

                    
                    switch (patch.patchType)
                    {
                        case PatchType.Prefix:
                            ModSettings.Harmony.Patch(GetMethod(type, patch), new HarmonyMethod(patch.patch));
                            break;
                        case PatchType.Postfix:
                            ModSettings.Harmony.Patch(GetMethod(type, patch), null, new HarmonyMethod(patch.patch));
                            break;
                        default:
                            DebugLogger.LogWarning($"    encountered an invalid patch type in component {comp.ComponentName}; skipping it.", DebugMessageKeys.Mods);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get all types from loaded mods
        /// </summary>
        /// <returns>A list of types to patch</returns>
        private static IEnumerable<Type> GetTypesToPatch()
        {
            List<Assembly> assemblies = new List<Assembly>() { Assembly.GetAssembly(typeof(Pawn)) };
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                assemblies.AddRange(mod.assemblies.loadedAssemblies);
            }

            List<Type> patchableTypes = new List<Type>();
            foreach(Assembly assembly in assemblies)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    if (types is null) continue;
                    patchableTypes.AddRange(types);
                } catch (ReflectionTypeLoadException) { }
            }

            return patchableTypes;
        }

        /// <summary>
        /// Gets the method out of a type when given
        /// a <see cref="PatchInfo"/> object
        /// </summary>
        /// <param name="type">The type to patch</param>
        /// <param name="patch">The patch being applied to <paramref name="type"/></param>
        /// <returns></returns>
        static MethodInfo GetMethod(Type type, PatchInfo patch)
        {
            if (patch.method != null) return patch.method;

            return ICompatibilityPatchComponent.GetMethod(type, patch.methodName);
        }
    }
}