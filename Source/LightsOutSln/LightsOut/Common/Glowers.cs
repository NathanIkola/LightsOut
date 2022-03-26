using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace LightsOut.Common
{
    /// <summary>
    /// Holds common glower operations
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Glowers
    {
        /// <summary>
        /// Sets whether a glowable building is able to glow or not
        /// </summary>
        /// <param name="glower">The glower to enable or disable</param>
        /// <param name="canGlow">Whether or not this glower can glow</param>
        /// <returns>The previous value, or <see langword="null"/> if it wasn't 
        /// in the dictionary before</returns>
        private static bool? SetCanGlow(ThingComp glower, bool? canGlow)
        {
            if (glower is null) return false;

            bool? previous = Resources.SetConsumesResources(glower.parent, canGlow);

            UpdateGlower(glower);

            return previous;
        }

        /// <summary>
        /// Marks a glower as enabled
        /// </summary>
        /// <param name="glower">The glower to enable</param>
        /// <returns>The previous value, or <see langword="null"/> if it wasn't 
        /// in the dictionary before</returns>
        public static bool? EnableGlower(ThingComp glower)
        {
            DebugLogger.AssertFalse(glower is null, "EnableGlower was called on a null glower");
            return SetCanGlow(glower, true);
        }

        /// <summary>
        /// Marks a glower as disabled
        /// </summary>
        /// <param name="glower">The glower to disable</param>
        /// <returns>The previous value, or <see langword="null"/> if it wasn't 
        /// in the dictionary before</returns>
        public static bool? DisableGlower(ThingComp glower)
        {
            DebugLogger.AssertFalse(glower is null, "DisableGlower was called on a null glower");
            return SetCanGlow(glower, false);
        }

        /// <summary>
        /// Determine whether or not <paramref name="glower"/> is enabled
        /// </summary>
        /// <param name="glower">The glower to check</param>
        /// <returns><see langword="true"/> if the glower is enabled, 
        /// <see langword="false"/> if it's disabled, 
        /// and <see langword="null"/> if it hasn't been set</returns>
        public static bool? CanGlow(ThingComp glower)
        {
            DebugLogger.AssertFalse(glower is null, "CanGlow was called on a null glower");
            if (glower is null) return false;
            return Resources.CanConsumeResources(glower.parent);
        }

        /// <summary>
        /// Gets the glower comp from a building
        /// </summary>
        /// <param name="thing">The <see cref="Building"/> to get the comp from</param>
        /// <returns>The glower if present, otherwise <see langword="null"/></returns>
        public static ThingComp GetGlower(ThingWithComps thing)
        {
            DebugLogger.AssertFalse(thing is null, "GetGlower called on a null building");
            if (thing is null) return null;
            if (CachedGlowers.ContainsKey(thing))
                return CachedGlowers[thing];

            try
            {
                ThingComp glower = thing.AllComps.Last(x => CompGlowers.Contains(x.GetType()));
                CachedGlowers.Add(thing, glower);
                return glower;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Update a glower's lit status
        /// </summary>
        /// <param name="glower">The glower to update</param>
        private static void UpdateGlower(ThingComp glower)
        {
            DebugLogger.AssertFalse(glower is null, "UpdateGlower was called on a null glower");
            if (glower is null) return;
            if (glower is CompGlower compGlower)
                compGlower.UpdateLit(compGlower.parent.Map);
            else
                TryUpdateGenericGlower(glower);
        }

        /// <summary>
        /// Attempt to update a generic glower's lit status
        /// </summary>
        /// <param name="glower">The glower to update</param>
        private static void TryUpdateGenericGlower(ThingComp glower)
        {
            if (glower is null) return;
            ThingWithComps thing = glower.parent;
            MethodInfo updateLit = glower.GetType().GetMethod("UpdateLit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            DebugLogger.AssertFalse(updateLit is null, $"Could not find UpdateLit method on {glower.GetType().FullName}", true);

            if (updateLit != null)
            {
                try
                {
                    if (updateLit.GetParameters().Length == 1 && updateLit.GetParameters()[0].ParameterType == typeof(Map))
                        updateLit.Invoke(glower, new object[] { thing.Map });
                    else if (updateLit.GetParameters().Length == 0)
                        updateLit.Invoke(glower, null);
                    else
                        DebugLogger.LogWarning($"Too many arguments for UpdateLit on {glower.GetType().FullName}");
                }
                catch (Exception e)
                {
                    DebugLogger.LogErrorOnce($"Having trouble updating a generic glower: {glower.GetType().FullName}: {e}");
                }
            }
        }

        /// <summary>
        /// A memoized list of glowers to speed up subsequent calls to GetGlower
        /// </summary>
        private static Dictionary<ThingWithComps, ThingComp> CachedGlowers { get; } = new Dictionary<ThingWithComps, ThingComp>();

        /// <summary>
        /// A list of valid glower types (can be added to by mod compatibility patches as needed)
        /// </summary>
        public static List<Type> CompGlowers { get; } = new List<Type>() { typeof(CompGlower) };
    }
}