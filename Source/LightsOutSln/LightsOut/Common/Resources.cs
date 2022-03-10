//************************************************
// Goulash class of various resources
//************************************************

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LightsOut.Common
{
    [StaticConstructorOnStartup]
    public static class Resources
    {

        //****************************************
        // Callback to reset the resource draw
        // of a consumer
        //****************************************
        public delegate void ResetResourceDrawCallback();

        //****************************************
        // Add a thing to the resource-consumable
        // dictionary
        //
        // Returns: the previous value or null
        // if it was not in the dictionary before
        //****************************************
        public static bool? SetConsumesResources(ThingWithComps thing, bool? consumesResource)
        {
            if (thing is null) return null;
            bool? previous = CanConsumeResources(thing);
            if (consumesResource == previous)
                return previous;
            BuildingStatus[thing] = consumesResource;
            return previous;
        }

        //****************************************
        // Check if a trader is able to
        // consume resources
        //
        // Returns: whether or not the trader
        // can consume resources, or null if it is
        // not in the dictionary
        //****************************************
        public static bool? CanConsumeResources(ThingComp resourceTrader)
        {
            return CanConsumeResources(resourceTrader?.parent);
        }

        //****************************************
        // Check if a trader is able to
        // consume resources
        //
        // Returns: whether or not the trader
        // can consume resources, or null if it is
        // not in the dictionary
        //****************************************
        public static bool? CanConsumeResources(ThingWithComps thing)
        {
            if (thing is null) return null;
            if (IsCharged(thing)) return false;
            return BuildingStatus.TryGetValue(thing, null);
        }

        //****************************************
        // Check if a consumer is a 
        // rechargeable building
        //****************************************
        public static bool IsCharged(ThingWithComps thing)
        {
            if (thing is null) return false;
            CompRechargeable rechargeable = null;
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

        //****************************************
        // Get whether something is rechargeable
        //****************************************
        public static bool IsRechargeable(Building thing)
        {
            if (thing is null) return false;
            if (CompRechargeables.ContainsKey(thing))
                return CompRechargeables[thing] != null;

            CompRechargeable rechargeable = thing.GetComp<CompRechargeable>();
            CompRechargeables.Add(thing, rechargeable);
            return rechargeable != null;
        }

        //****************************************
        // The minimum amount of resource to draw
        //****************************************
        public static readonly float MinDraw = -1f / 100f;

        // keep track of all disabled Things
        public static Dictionary<ThingWithComps, bool?> BuildingStatus { get; } = new Dictionary<ThingWithComps, bool?>();

        // finally, time to start memoizing things
        private static Dictionary<ThingWithComps, CompRechargeable> CompRechargeables { get; } = new Dictionary<ThingWithComps, CompRechargeable>();
    }
}