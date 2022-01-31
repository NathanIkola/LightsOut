//************************************************
// Holds all of the common room operations
//************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace LightsOut.Common
{
    [StaticConstructorOnStartup]
    public static class Glowers
    {
        //****************************************
        // Add a glower to the glowable dictionary
        //
        // Returns: the previous value or null
        // if it was not in the dictionary before
        //****************************************
        public static bool? SetCanGlow(ThingComp glower, bool? canGlow)
        {
            if (glower is null) return false;
            bool? previous = CanGlow(glower);
            Power.BuildingStatus[glower.parent] = canGlow;

            // only update if the state has changed
            if (previous is null || canGlow is null || previous != canGlow)
                UpdateGlower(glower);

            return previous;
        }

        //****************************************
        // Add a glower to the glowable dictionary
        //
        // Returns: whether or not the glower
        // is able to glow, or null if it is
        // not in the dictionary
        //****************************************
        public static bool? CanGlow(ThingComp glower)
        {
            if (glower is null) return false;
            return Power.BuildingStatus.TryGetValue(glower.parent, null);
        }

        //****************************************
        // Get the glower (if present) from a
        // building, or return null if not present
        //****************************************
        public static ThingComp GetGlower(Building thing)
        {
            if (thing is null) return null;
            if (CachedGlowers.ContainsKey(thing))
                return CachedGlowers[thing];

            try
            {
                ThingComp glower = thing.AllComps.First(x => CompGlowers.Contains(x.GetType()));
                CachedGlowers.Add(thing, glower);
                return glower;
            }
            catch (Exception) { return null; }
        }

        //****************************************
        // Update any glower
        //****************************************
        private static void UpdateGlower(ThingComp glower)
        {
            if (glower is null) return;
            if (glower is CompGlower compGlower)
                compGlower.UpdateLit(compGlower.parent.Map);
            else
                TryUpdateGenericGlower(glower);
        }

        //****************************************
        // Try to update a generic glower
        //****************************************
        private static void TryUpdateGenericGlower(ThingComp glower)
        {
            if (glower is null) return;
            ThingWithComps thing = glower.parent;
            MethodInfo updateLit = glower.GetType().GetMethod("UpdateLit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateLit != null)
            {
                try
                {
                    if (updateLit.GetParameters().Length == 1 && updateLit.GetParameters()[0].ParameterType == typeof(Map))
                        updateLit.Invoke(glower, new object[] { thing.Map });
                    else if (updateLit.GetParameters().Length == 0)
                        updateLit.Invoke(glower, null);
                }
                catch (Exception e)
                {
                    Log.Warning($"[LightsOut] Having trouble updating a generic glower: {glower.GetType()}: {e}");
                }
            }
        }

        private static Dictionary<Building, ThingComp> CachedGlowers { get; } = new Dictionary<Building, ThingComp>();
        public static List<Type> CompGlowers { get; } = new List<Type>() { typeof(CompGlower) };
    }
}
