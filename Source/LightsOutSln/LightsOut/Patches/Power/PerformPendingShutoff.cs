using HarmonyLib;
using Verse;
using LightsOut.Common;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Performs the checking to turn buildings off after
    /// their specified delay period has expired
    /// </summary>
    [HarmonyPatch(typeof(TickManager))]
    [HarmonyPatch(nameof(TickManager.DoSingleTick))]
    public class PerformPendingShutoff
    {
        /// <summary>
        /// Hooks into the core game tick to decrement the ticks remaining
        /// for consumable resources
        /// </summary>
        public static void Prefix()
        {
            // don't need to check every single tick
            int curTick = GenTicks.TicksGame;
            if (curTick % ModSettings.TicksBetweenShutoffCheck == 0)
                while (Resources.NextTickToDisableBuilding() <= curTick)
                {
                    ThingWithComps thing = Resources.PendingShutoff.Dequeue().First;
                    if (Common.Lights.CanBeLight(thing))
                        Common.Lights.DisableLight(thing);
                    else
                        Resources.SetTicksRemaining(thing, 0);
                }
        }
    }
}
