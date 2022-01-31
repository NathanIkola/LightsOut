//************************************************
// Goulash class of various resources
//************************************************

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LightsOut.Common
{
    [StaticConstructorOnStartup]
    public static class Power
    {
        //****************************************
        // Add a CompPower to the power-consumable
        // dictionary
        //
        // Returns: the previous value or null
        // if it was not in the dictionary before
        //****************************************
        public static bool? SetConsumesPower(CompPower powerComp, bool? consumesPower)
        {
            if (powerComp == null) return null;
            bool? previous = CanConsumePower(powerComp);
            BuildingStatus[powerComp.parent] = consumesPower;

            // make sure to reset the power status
            if (!(consumesPower is null) && powerComp is CompPowerTrader trader)
                trader.PowerOutput = -trader.Props.basePowerConsumption;

            return previous;
        }

        //****************************************
        // Check if a CompPower is able to
        // consume power
        //
        // Returns: whether or not the CompPower
        // can consume power, or null if it is
        // not in the dictionary
        //****************************************
        public static bool? CanConsumePower(CompPower powerComp)
        {
            if (powerComp == null) return null;
            if (IsCharged(powerComp.parent)) return false;
            return BuildingStatus.TryGetValue(powerComp.parent, null);
        }

        //****************************************
        // Check if a power consumer is a 
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

        // keep track of all disabled Things
        public static Dictionary<ThingWithComps, bool?> BuildingStatus { get; } = new Dictionary<ThingWithComps, bool?>();

        // finally, time to start memoizing things
        private static Dictionary<ThingWithComps, CompRechargeable> CompRechargeables { get; } = new Dictionary<ThingWithComps, CompRechargeable>();
    }
}