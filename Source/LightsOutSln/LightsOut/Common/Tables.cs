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
            if (Resources.MemoizedThings.ContainsKey(thing))
                return Resources.MemoizedThings[thing] == Resources.ThingType.Table;

            if (thing is null)
                return false;

            if (HasBlacklistedTableComp(thing))
                return false;

            bool isTable = ((thing is Building_WorkTable || thing is Building_ResearchBench));
            if (isTable)
                Resources.MemoizedThings.Add(thing, Resources.ThingType.Table);

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
        /// Check if anyone else is watching a television
        /// </summary>
        /// <param name="tv">The television to check</param>
        /// <param name="pawn">The pawn that is currently watching</param>
        /// <returns><see langword="true"/> if another pawn is watching <paramref name="tv"/>, <see langword="false"/> otherwise</returns>
        public static bool IsAnyoneElseWatching(ThingWithComps tv, Pawn pawn)
        {
            if (tv is null || !IsTelevision(tv))
                return false;

            IEnumerable<IntVec3> watchArea = WatchBuildingUtility.CalculateWatchCells(tv.def, tv.Position, tv.Rotation, tv.Map);
            foreach (IntVec3 cell in watchArea)
            {
                foreach (Thing thing in cell.GetThingList(tv.Map))
                    if (thing is Pawn p && p != pawn && p.CurJob?.GetTarget(Verse.AI.TargetIndex.A).Thing == tv)
                        return true;
            }

            return false;
        }
    }
}