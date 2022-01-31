//************************************************
// The class in charge of consuming all of the
// mod compatibility patches
//************************************************

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

namespace LightsOut.Patches.ModCompatibility
{
    public static class ModCompatibilityManager
    {
        //****************************************
        // The compatibility patches to apply
        //****************************************
        static readonly List<ICompatibilityPatch> CompatibilityPatches = new List<ICompatibilityPatch>()
        {
            new AndroidsCompatibilityPatch(),
            new VEWallLightCompatibilityPatch(),
            new WallLightCompatibilityPatch(),
            new ModGlowerCompatibilityPatch()
        };

        //****************************************
        // Actually do the loading for the
        // mods in the above list
        //****************************************
        public static void LoadCompatibilityPatches()
        {
            foreach (ICompatibilityPatch patch in CompatibilityPatches)
            {
                ApplyPatch(patch);
            }
        }

        //****************************************
        // Apply a whole compatibility patch
        //****************************************
        private static void ApplyPatch(ICompatibilityPatch patch)
        {
            if (string.IsNullOrWhiteSpace(patch.CompatibilityPatchName))
            {
                Log.Error($"[LightsOut] encountered compatibility patch with empty name; skipping it.");
                return;
            }

            // only load this patch if the target mod is present and accounted for
            if (!string.IsNullOrEmpty(patch.TargetMod))
            {
                if (!LoadedModManager.RunningModsListForReading.Any(x => x.Name == patch.TargetMod))
                    return;
            }

            Log.Message($"[LightsOut] applying mod compatibility patch: {patch.CompatibilityPatchName}");

            foreach (ICompatibilityPatchComponent component in patch.GetComponents())
            {
                ApplyPatchComponent(component);
            }
        }

        //****************************************
        // Apply a single compatibility component
        //****************************************
        private static void ApplyPatchComponent(ICompatibilityPatchComponent comp)
        {
            if (string.IsNullOrWhiteSpace(comp.ComponentName))
            {
                Log.Error("[LightsOut] encountered a compatibility component with an empty name; skipping it.");
                return;
            }

            if (string.IsNullOrWhiteSpace(comp.TypeNameToPatch))
            {
                Log.Error($"[LightsOut] encountered a compatibility component ({comp.ComponentName}) with an empty type to patch; skipping it.");
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
                    Log.Message($"[LightsOut]    component applied: {comp.ComponentName}");
                wasApplied = true;

                foreach (PatchInfo patch in comp.GetPatches(type))
                {
                    if (patch.method is null && patch.methodName is null)
                    {
                        Log.Warning($"[LightsOut]    encountered a component with a null method; skipping it.");
                        continue;
                    }

                    if (patch.patch is null)
                    {
                        Log.Warning($"[LightsOut]    encountered a component with a null patch; skipping it.");
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
                            Log.Warning($"[LightsOut]    encountered an invalid patch type in component {comp.ComponentName}; skipping it.");
                            break;
                    }
                }
            }
        }

        //****************************************
        // Gets all types from loaded mods
        //****************************************
        private static IEnumerable<Type> GetTypesToPatch()
        {
            List<Assembly> assemblies = new List<Assembly>();
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

        //****************************************
        // Raw dog the method out of a type
        //****************************************
        static MethodInfo GetMethod(Type type, PatchInfo patch)
        {
            if (patch.method != null) return patch.method;

            return GetMethod(type, patch.methodName);
        }

        //****************************************
        // Really raw dog it out using a string
        //****************************************
        static MethodInfo GetMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName, ICompatibilityPatchComponent.BindingFlags);
        }
    }
}