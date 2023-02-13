using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LightsOut.Common
{
    /// <summary>
    /// Helper functions for enterable buildings support
    /// </summary>
    public static class Enterables
    {
        private static HashSet<Type> enterables = new HashSet<Type>();

        /// <summary>
        /// Is a thing registered as an enterable we care about?
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsEnterable(ThingWithComps t)
        {
            return enterables.Contains(t.GetType());
        }

        public static bool Occupied(ThingWithComps thing)
        {
            return thing is Building_Enterable ent && ent.SelectedPawn != null;
        }

        /// <summary>
        /// Register type as an enterable that we care about
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterEnterable(Type t)
        {
            DebugLogger.Assert(t.IsSubclassOf(typeof(Building_Enterable)), "IsEnterable called with non-enterable");
            enterables.Add(t);
        }
    }
}