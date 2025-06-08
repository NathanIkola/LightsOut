﻿using HarmonyLib;
using Verse;
using LightsOut.Common;
using System.Collections.Generic;
using LightsOutSettings = LightsOut.Boilerplate.ModSettings;

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
            Dictionary<Room, bool?> rooms = new Dictionary<Room, bool?>();

            // don't need to check every single tick
            int curTick = GenTicks.TicksGame;
            if (curTick % LightsOutSettings.TicksBetweenShutoffCheck == 0)
                while (Resources.NextTickToDisableBuilding() <= curTick)
                {
                    ThingWithComps thing = Resources.PendingShutoff.Dequeue().First;
                    if (Common.Lights.CanBeLight(thing))
                    {
                        Room room = Rooms.GetRoom(thing);
                        // can't make a decision about this, leave it on
                        if (room is null) 
                            continue;

                        bool? shouldShutoff = rooms.TryGetValue(room, null);
                        if (shouldShutoff == null)
                        {
                            shouldShutoff = Common.Lights.ShouldTurnOffAllLights(room, null);
                            rooms.Add(room, shouldShutoff);
                        }
                        if (shouldShutoff == true)
                            Common.Lights.DisableLight(thing);
                    }
                    else
                        Resources.SetTicksRemaining(thing, 0);
                }
        }
    }
}
