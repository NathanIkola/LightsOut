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

                // return so that we don't disable the KeepOnComp
                return;
            }
            // some mods (SOS2) can force a Thing to despawn/respawn without triggering cleanup
            else if (!Resources.MemoizedThings.ContainsKey(__instance))
            {
                Resources.MemoizedThings.Add(__instance, Resources.ThingType.Unknown);
            }

            KeepOnComp keepOnComp = __instance.TryGetComp<KeepOnComp>();
            if (keepOnComp != null) { keepOnComp.Disabled = true; }
        }
    }
}