using HarmonyLib;
using Verse;
using LightsOut.Common;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(TickManager))]
    [HarmonyPatch(nameof(TickManager.DoSingleTick))]
    public class DecrementTickCounts
    {
        /// <summary>
        /// Hooks into the core game tick to decrement the ticks remaining
        /// for consumable resources
        /// </summary>
        public static void Prefix()
        {
            // don't need to check every single tick
            if (GenTicks.TicksGame % ModSettings.TicksBetweenDecrement == 0)
                Resources.DecrementAllTicksRemaining();
        }


    }
}
