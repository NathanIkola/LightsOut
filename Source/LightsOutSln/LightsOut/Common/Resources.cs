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

            // convert bool? into its int? counterpart
            int? ticks = null;
            if (consumesResource == true) ticks = -1;
            else if (consumesResource == false) ticks = 0;

            ticks = SetTicksRemaining(thing, ticks);
            if (ticks is null) return null;

            return ticks != 0;
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
            int? ticksRemaining = GetTicksRemaining(thing);
            bool canConsumeResources = ticksRemaining != 0;

            if (canConsumeResources == false || (IsRechargeable(thing) && IsCharged(thing)))
                return false;

            return true;
        }

        /// <summary>
        /// Check how many ticks <paramref name="thing"/> has
        /// until it can no longer consume resources
        /// </summary>
        /// <param name="thing">The thing to check</param>
        /// <returns>The number of ticks until <paramref name="thing"/> can
        /// no longer consume resources</returns>
        public static int? GetTicksRemaining(ThingWithComps thing)
        {
            DebugLogger.AssertFalse(thing is null, "GetTicksRemaining called on a null thing");
            if (thing is null) return null;
            return BuildingStatus.TryGetValue(thing, null);
        }

        /// <summary>
        /// Sets the number of <paramref name="ticksRemaining"/> for the specified <paramref name="thing"/>
        /// </summary>
        /// <param name="thing">The thing to set the ticks for</param>
        /// <param name="ticksRemaining">The number of ticks to set it to</param>
        /// <returns>The previous value</returns>
        public static int? SetTicksRemaining(ThingWithComps thing, int? ticksRemaining)
        {
            DebugLogger.AssertFalse(thing is null, "SetTicksRemaining called on a null thing");
            if (thing is null) return null;

            int? previous = GetTicksRemaining(thing);
            if (ticksRemaining == previous) return previous;

            BuildingStatus[thing] = ticksRemaining;
            return previous;
        }

        /// <summary>
        /// Decrement the number of ticks remaining for the specified <paramref name="thing"/>
        /// </summary>
        /// <param name="thing">The thing to check</param>
        /// <returns>The new number of ticks remaining</returns>
        public static int? DecrementTicksRemaining(ThingWithComps thing)
        {
            DebugLogger.AssertFalse(thing is null, "DecrementTicksRemaining called on a null thing");
            if (thing is null) return null;
            
            int? ticksRemaining = GetTicksRemaining(thing);
            if (ticksRemaining is null) return null;

            if (ticksRemaining > 0)
            {
                if (ticksRemaining == 1 && Lights.CanBeLight(thing))
                    Lights.DisableLight(thing, --ticksRemaining);
                else
                    SetTicksRemaining(thing, --ticksRemaining);
            }

            return ticksRemaining;
        }

        /// <summary>
        /// Decrements all positive tick counts in the tick dictionary
        /// </summary>
        /// <remarks>Be careful to only call this once per tick</remarks>
        public static void DecrementAllTicksRemaining()
        {
            ThingWithComps[] things = new ThingWithComps[BuildingStatus.Keys.Count];
            BuildingStatus.Keys.CopyTo(things, 0);

            foreach (ThingWithComps thing in things)
            {
                if (GetTicksRemaining(thing) > 0)
                    DecrementTicksRemaining(thing);
            }
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
        public static bool IsRechargeable(ThingWithComps building)
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
        public static Dictionary<ThingWithComps, int?> BuildingStatus { get; } = new Dictionary<ThingWithComps, int?>();

        /// <summary>
        /// A memoized list of the CompRechargeables to prevent repeated comp lookups
        /// </summary>
        private static Dictionary<ThingWithComps, CompRechargeable> CompRechargeables { get; } = new Dictionary<ThingWithComps, CompRechargeable>();

        public enum ThingType
        {
            Unknown,
            Table,
            Light
        }

        /// <summary>
        /// A single repository of the cached Thing Types
        /// </summary>
        public static Dictionary<ThingWithComps, ThingType> MemoizedThings { get; } = new Dictionary<ThingWithComps, ThingType>();
    }
}