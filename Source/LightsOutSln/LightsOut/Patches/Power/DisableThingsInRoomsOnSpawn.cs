//************************************************
// Disable all compatible things in a room when
// the room finishes being made, such as when
// loading a save
//************************************************

using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using LightsOut.Utility;
using System;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Power
{
    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("FloodAndSetRooms")]
    public class DisableThingsInRoomsOnSpawn
    {
        public static void Postfix(Room __1)
        {
            Room room = __1;
            if (room is null) return;

            if (ModSettings.FlickLights && (ModResources.RoomIsEmpty(room, null)
                || (!ModSettings.NightLights && ModResources.AllPawnsSleeping(room, null))))
            {
                ModResources.DisableAllLights(room);
            }
            else
            {
                ModResources.EnableAllLights(room);
            }
        }
    }
}