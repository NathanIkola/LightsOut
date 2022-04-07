//************************************************
// Holds all of the common table operations
//************************************************

using LightsOut.Defs;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Common
{
    /// <summary>
    /// Holds common table operations
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Tables
    {
        /// <summary>
        /// Provides a way to disable a table
        /// </summary>
        /// <param name="table">The table to disable</param>
        public static void DisableTable(ThingWithComps table)
        {
            DebugLogger.AssertFalse(table is null, "DisableTable called on a null table");
            if (table is null) return;
            ThingComp glower = Glowers.GetGlower(table);
            if (!(glower is null))
                Glowers.DisableGlower(glower);
            else
                Resources.SetConsumesResources(table, false);
        }

        /// <summary>
        /// Provides a way to enable a table
        /// </summary>
        /// <param name="table">The table to enable</param>
        public static void EnableTable(ThingWithComps table)
        {
            DebugLogger.AssertFalse(table is null, "EnableTable called on a null table");
            if (table is null) return;
            ThingComp glower = Glowers.GetGlower(table);
            if (!(glower is null))
                Glowers.EnableGlower(glower);
            else
                Resources.SetConsumesResources(table, true);
        }

        /// <summary>
        /// Checks if the specified <paramref name="thing"/> can possibly be a table or not
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if it can be a table, <see langword="false"/> if not</returns>
        public static bool IsTable(ThingWithComps thing)
        {
            DebugLogger.AssertFalse(thing is null, "IsTable called on a null thing");
            if (MemoizedIsTable.ContainsKey(thing))
                return MemoizedIsTable[thing];

            if (thing is null)
            {
                MemoizedIsTable.Add(thing, false);
                return false;
            }

            if (HasBlacklistedTableComp(thing))
            {
                MemoizedIsTable.Add(thing, false);
                return false;
            }

            bool isTable = ((thing is Building_WorkTable || thing is Building_ResearchBench));
            MemoizedIsTable.Add(thing, isTable);
            return isTable;
        }

        /// <summary>
        /// Checks to see if the provided <see cref="ThingWithComps"/> contains an illegal comp
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if it contains an illegal comp, <see langword="false"/> if not</returns>
        public static bool HasBlacklistedTableComp(ThingWithComps thing)
        {
            if (thing.TryGetComp<CompFireOverlay>() is null
                && thing.TryGetComp<CompDarklightOverlay>() is null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks to see if a building is a television
        /// </summary>
        /// <param name="thing">The <see cref="ThingWithComps"/> to check</param>
        /// <returns><see langword="true"/> if <paramref name="thing"/> is a television, <see langword="false"/> if not</returns>
        public static bool IsTelevision(ThingWithComps thing)
        {
            return thing.def == CustomThingDefs.TubeTelevision
                || thing.def == CustomThingDefs.FlatscreenTelevision
                || thing.def == CustomThingDefs.MegascreenTelevision;
        }

        /// <summary>
        /// Memoized list of results from the IsTable function since that doesn't change.
        /// Used to speed up subsequent calls to IsTable
        /// </summary>
        private static Dictionary<ThingWithComps, bool> MemoizedIsTable { get; } = new Dictionary<ThingWithComps, bool>();
    }
}