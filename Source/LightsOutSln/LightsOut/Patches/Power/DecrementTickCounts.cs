using HarmonyLib;
using Verse;
using LightsOut.Common;

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
            Resources.DecrementAllTicksRemaining();
        }
    }
}
