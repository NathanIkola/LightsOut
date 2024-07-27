using Verse;
using HarmonyLib;
using LightsOut.Common;
using LightsOut.ThingComps;
using System;
using RimWorld;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Disables Buildings as they spawn
    /// </summary>
    [HarmonyPatch(typeof(Building))]
    [HarmonyPatch(nameof(Building.SpawnSetup))]
    public class DisableBuildingsOnSpawn
    {
        /// <summary>
        /// Checks if a ThingWithComps needs to be disabled when it spawns
        /// </summary>
        /// <param name="__instance">This instance</param>
        public static void Postfix(ThingWithComps __instance)
        {
            if ((Tables.IsTable(__instance) || (Enterables.IsEnterable(__instance) && !Enterables.Occupied(__instance)) || Tables.IsTelevision(__instance)))
            {
                Tables.DisableTable(__instance);
            }
            else if (Common.Lights.CanBeLight(__instance))
            {
                Room room = Rooms.GetRoom(__instance);
                if (!(room is null) && Common.Lights.ShouldTurnOffAllLights(room, null) && !Common.Lights.KeepLightOn(__instance))
                    Common.Lights.DisableLight(__instance);
                else
                    Common.Lights.EnableLight(__instance);

                // return so that we don't remove the KeepOnComp from this
                return;
            }
            // some mods (SOS2) can force a Thing to despawn/respawn without triggering cleanup
            else if (!Resources.MemoizedThings.ContainsKey(__instance))
            {
                Resources.MemoizedThings.Add(__instance, Resources.ThingType.Unknown);
            }

            bool removed = false;
            uint attempts = 0;
            while(!removed)
            {
                try
                {
                    __instance.AllComps.RemoveAll(x => x is KeepOnComp);
                    removed = true;
                }
                catch (InvalidOperationException e)
                {
                    if (e.Message.ToLower().Contains("modified"))
                    {
                        if (++attempts > 100) removed = true;
                    }
                    else
                    {
                        DebugLogger.LogWarning($"(SpawnSetup): InvalidOperationException: {e.Message}", DebugMessageKeys.Patches + DebugMessageKeys.Comps);
                        removed = true;
                    }
                }
            }
            if (attempts > 1)
                DebugLogger.LogWarning($"(SpawnSetup): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.", DebugMessageKeys.Patches + DebugMessageKeys.Comps);
        }
    }
}