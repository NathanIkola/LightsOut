//************************************************
// Holds all of the common table operations
//************************************************

using LightsOut.Defs;
using RimWorld;
using System.Collections.Generic;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Common
{
    [StaticConstructorOnStartup]
    public static class Tables
    {
        //****************************************
        // Does the hard work of disabling
        // a worktable
        //****************************************
        public static void DisableTable(Building table)
        {
            if (table is null) return;
            ThingComp glower = Glowers.GetGlower(table);
            if (!(glower is null) && ModSettings.StandbyPowerDrawRate == 0f)
                Glowers.SetCanGlow(glower, false);
            else
                Resources.SetConsumesResources(table, false);
        }

        //****************************************
        // Does the hard work of enabling
        // a worktable
        //****************************************
        public static void EnableTable(Building table)
        {
            if (table is null) return;
            ThingComp glower = Glowers.GetGlower(table);
            if (!(glower is null) && ModSettings.StandbyPowerDrawRate == 0f)
                Glowers.SetCanGlow(glower, true);
            else
                Resources.SetConsumesResources(table, true);
        }

        //****************************************
        // Check if a table has a comp on the
        // disallowed list
        //****************************************
        public static bool IsTable(Building thing)
        {
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

        //****************************************
        // Detects blacklisted comps on tables
        //
        // Used to be a list, but that has much
        // worse performance implications
        //****************************************
        public static bool HasBlacklistedTableComp(ThingWithComps thing)
        {
            if (thing.TryGetComp<CompFireOverlay>() is null)
            {
                return false;
            }
            return true;
        }

        //****************************************
        // Detects if a building is a television
        //****************************************
        public static bool IsTelevision(Building thing)
        {
            return thing.def == MyThingDefOf.TubeTelevision
                || thing.def == MyThingDefOf.FlatscreenTelevision
                || thing.def == MyThingDefOf.MegascreenTelevision;
        }

        private static Dictionary<Building, bool> MemoizedIsTable { get; } = new Dictionary<Building, bool>();
    }
}