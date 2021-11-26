//************************************************
// An interface that gives me the ability to
// make specific mod compatibility patches that
// only take effect if the mod they attempt to
// patch is present in the game
//************************************************

using System;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using System.Reflection;
using LightsOut.Boilerplate;

namespace LightsOut.Patches.ModCompatibility
{
    public enum PatchType
    {
        Prefix,
        Postfix
    }

    public struct PatchInfo
    {
        public MethodInfo method;
        // this can be used if you can't reflect the method
        public string methodName;
        public MethodInfo patch;
        public PatchType patchType;
    }

    public abstract class IModCompatibilityPatch
    {
        // the typical binding flags we are going to want to use
        public readonly static BindingFlags BindingFlags = BindingFlags.Public 
                                                | BindingFlags.NonPublic 
                                                | BindingFlags.Instance 
                                                | BindingFlags.Static 
                                                | BindingFlags.FlattenHierarchy;

        public IModCompatibilityPatch()
        {
            if (String.IsNullOrEmpty(TypeNameToPatch))
            {
                Log.Error("TypeNameToPatch was null!");
                return;
            }

            List<Type> typesToPatch = GetTypesToPatch();
            if (typesToPatch.Count > 1 && !TargetsMultipleTypes)
            {
                string msg = $"Compatibility Patch for type \"{TypeNameToPatch}\" might be too general, found: ";
                foreach (Type type in typesToPatch)
                    msg = msg + type.Name + "; ";
                Log.Warning(msg);
            }

            if (typesToPatch.Count == 0) return;
            else if (typesToPatch.Count == 1)
                Log.Message($"[LightsOut] patching 1 mod for \"{PatchName} Patch - {TypeNameToPatch}\"");
            else
                Log.Message($"[LightsOut] patching {typesToPatch.Count} mods for \"{PatchName} Patch - {TypeNameToPatch}\"");

            foreach (Type type in typesToPatch)
            {
                foreach (PatchInfo patch in GetPatches())
                {
                    switch (patch.patchType)
                    {
                        case PatchType.Prefix:
                            HarmonyPatches.Harmony.Patch(GetMethod(type, patch), new HarmonyMethod(patch.patch));
                            break;
                        case PatchType.Postfix:
                            HarmonyPatches.Harmony.Patch(GetMethod(type, patch), null, new HarmonyMethod(patch.patch));
                            break;
                        default:
                            Log.Warning($"Invalid PatchType encountered in patch \"{PatchName}\" for type {TypeNameToPatch}: {patch.patchType}");
                            break;
                    }
                }
                TypesExplicitlyPatched.Add(type);
            }
        }

        private List<Type> GetTypesToPatch()
        {
            Assembly mscorlib = Assembly.GetAssembly(typeof(int));

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> typesToPatch = new List<Type>();

            foreach (Assembly asm in assemblies)
            {
                if (asm == mscorlib) continue;

                Type[] types = new Type[] { };
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException) { continue; }

                foreach (Type type in types)
                {
                    if (!TypeNameIsExact)
                    {
                        if (type.Name.Contains(TypeNameToPatch))
                        {
                            typesToPatch.Add(type);
                        }
                    }
                    else
                    {
                        if (type.Name.Equals(TypeNameToPatch))
                        {
                            typesToPatch.Add(type);
                        }
                    }
                }
            }

            return typesToPatch;
        }

        //****************************************
        // Raw dog the method out of a type
        //****************************************
        protected static MethodInfo GetMethod(Type type, PatchInfo patch)
        {
            if (patch.method != null) return patch.method;

            return GetMethod(type, patch.methodName);
        }

        //****************************************
        // Really raw dog it out using a string
        //****************************************
        protected static MethodInfo GetMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags);
        }

        //****************************************
        // Returns the list of patch infos that
        // should be applied when this
        // compatibility layer is loaded
        //****************************************
        protected virtual IEnumerable<PatchInfo> GetPatches()
        {
            return new List<PatchInfo>();
        }

        // the list of types that we have already patched
        public static List<Type> TypesExplicitlyPatched { get; } = new List<Type>();

        // the name of the type that this compatibility class is trying to patch
        protected abstract string TypeNameToPatch { get; }
        // bypass the warning for multiple types if that's the point of this patch
        protected abstract bool TargetsMultipleTypes { get; }
        // specify that you know the exact name you want to patch
        protected abstract bool TypeNameIsExact { get; }
        // give info about the actual patch we are doing
        protected abstract string PatchName { get; }
    }
}
