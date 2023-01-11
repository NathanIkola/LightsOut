using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LightsOut.Common
{
    public static class Enterables
    {
        private static HashSet<Type> enterables = new HashSet<Type>();

        public static bool IsEnterable(ThingWithComps t)
        {
            return enterables.Contains(t.GetType());
        }

        public static bool Occupied(ThingWithComps thing)
        {
            return thing is Building_Enterable ent && ent.SelectedPawn != null;
        }

        public static void RegisterEnterable(Type t)
        {
            DebugLogger.Assert(t.IsInstanceOfType(typeof(Building_Enterable)), "IsEnterable called with non-enterable");
            enterables.Add(t);
        }
    }
}