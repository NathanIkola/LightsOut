//************************************************
// A component of a compatibility patch
// i.e. the part that actually does the work
//************************************************

using System;
using System.Collections.Generic;
using System.Reflection;

namespace LightsOut.Patches.ModCompatibility
{
    //********************************************
    // Supported patch types
    //********************************************
    public enum PatchType
    {
        Prefix,
        Postfix
    }

    //********************************************
    // Holds information about a particular
    // patch (basically just what harmony needs)
    //********************************************
    public struct PatchInfo
    {
        public MethodInfo method;
        public string methodName;
        public MethodInfo patch;
        public PatchType patchType;
    }

    public abstract class ICompatibilityPatchComponent
    {
        // the name of the type this component patches
        public abstract string TypeNameToPatch { get; }

        // whether this component is intended to target a single type
        public abstract bool TargetsMultipleTypes { get; }

        // specify that the type name is exact (i.e. doesn't just contain the type name)
        public abstract bool TypeNameIsExact { get; }

        // specify that the search should be case-sensitive
        public virtual bool CaseSensitive => true;

        // the name of this component
        public abstract string ComponentName { get; }

        // return the applicable list of patches
        public abstract IEnumerable<PatchInfo> GetPatches(Type type);

        // return the specified method
        public static MethodInfo GetMethod<TypeName>(string methodName)
        {
            return GetMethod(typeof(TypeName), methodName);
        }

        // return the specified method
        public static MethodInfo GetMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags);
        }

        // the typical binding flags we are going to want to use
        public readonly static BindingFlags BindingFlags = BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Instance
                            | BindingFlags.Static
                            | BindingFlags.FlattenHierarchy;
    }

    //********************************************
    // A more convenient way to patch things
    // if you have the concrete type
    //********************************************
    public abstract class ICompatibilityPatchComponent<TypeName> : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => typeof(TypeName).Name;
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;
    }
}