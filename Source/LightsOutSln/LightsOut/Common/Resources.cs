using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LightsOut.Common
{
    /// <summary>
    /// Holds common function for resource-consuming things
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Resources
    {
        /// <summary>
        /// Set a <see cref="ThingWithComps"/>'s resource consumption status
        /// </summary>
        /// <param name="thing">The thing to set resource consumption for</param>
        /// <param name="consumesResource">Whether or not it can consume resources</param>
        /// <returns>The previous resource consumption status</returns>
        public static bool? SetConsumesResources(ThingWithComps thing, bool? consumesResource)
        {
            DebugLogger.AssertFalse(thing is null, "SetConsumesResources called on a null thing");
            if (thing is null) return null;
            bool? previous = CanConsumeResources(thing);
            if (consumesResource == previous)
                return previous;
            BuildingStatus[thing] = consumesResource;
            return previous;
        }

        /// <summary>
        /// Check if a specific resource trader is enabled or not
        /// </summary>
        /// <param name="resourceTrader">The resource trader to check</param>
        /// <returns>Whether or not <paramref name="resourceTrader"/> can
        /// consume resources, or <see langword="null"/> if it hasn't been set</returns>
        public static bool? CanConsumeResources(ThingComp resourceTrader)
        {
            return CanConsumeResources(resourceTrader?.parent);
        }

        /// <summary>
        /// Check if <paramref name="thing"/> can consume resources
        /// </summary>
        /// <param name="thing">The thing to check the status of</param>
        /// <returns>Whether or not <paramref name="thing"/> can
        /// consume resources, or <see langword="null"/> if it hasn't been set</returns>
        public static bool? CanConsumeResources(ThingWithComps thing)
        {
            DebugLogger.AssertFalse(thing is null, "CanConsumeResources called on a null thing");
            if (thing is null) return null;
            if (IsCharged(thing)) return false;
            return BuildingStatus.TryGetValue(thing, null);
        }

        /// <summary>
        /// Checks if <paramref name="thing"/> is charged
        /// </summary>
        /// <param name="thing">The thing to check the charge of</param>
        /// <returns>Whether or not <paramref name="thing"/> is charged</returns>
        public static bool IsCharged(ThingWithComps thing)
        {
            DebugLogger.AssertFalse(thing is null, "IsCharged called on a null thing");
            if (thing is null) return false;
            CompRechargeable rechargeable;
            if (CompRechargeables.ContainsKey(thing))
                rechargeable = CompRechargeables[thing];
            else
            {
                rechargeable = thing.GetComp<CompRechargeable>();
                CompRechargeables.Add(thing, rechargeable);
            }

            if (rechargeable is null) return false;
            return rechargeable.Charged;
        }

        /// <summary>
        /// Checks to see if <paramref name="building"/> is a
        /// rechargeable building
        /// </summary>
        /// <param name="building">The thing to check the rechargeable status of</param>
        /// <returns><see langword="true"/> if <paramref name="building"/> is
        /// rechargeable, <see langword="false"/> otherwise</returns>
        public static bool IsRechargeable(Building building)
        {
            DebugLogger.AssertFalse(building is null, "IsRechargeable called on a null building");
            if (building is null) return false;
            if (CompRechargeables.ContainsKey(building))
                return CompRechargeables[building] != null;

            CompRechargeable rechargeable = building.GetComp<CompRechargeable>();
            CompRechargeables.Add(building, rechargeable);
            return rechargeable != null;
        }

        /// <summary>
        /// The minimum amount of a resource to draw. 
        /// This being set properly allows buildings to
        /// respond to loss of power correctly and prevents
        /// a bug with Pawns pathing to unpowered benches repeatedly
        /// </summary>
        public static readonly float MinDraw = -1f / 100f;

        /// <summary>
        /// The structure that holds the info on whether or not
        /// a building is disabled
        /// </summary>
        public static Dictionary<ThingWithComps, bool?> BuildingStatus { get; } = new Dictionary<ThingWithComps, bool?>();

        /// <summary>
        /// A memoized list of the CompRechargeables to prevent repeated comp lookups
        /// </summary>
        private static Dictionary<ThingWithComps, CompRechargeable> CompRechargeables { get; } = new Dictionary<ThingWithComps, CompRechargeable>();
    }
}