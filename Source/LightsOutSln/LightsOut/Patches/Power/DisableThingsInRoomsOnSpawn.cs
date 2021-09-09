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

namespace LightsOut.Patches.Power
{
    using LightObject = KeyValuePair<CompPowerTrader, ThingComp>;

    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("FloodAndSetRooms")]
    public class DisableThingsInRoomsOnSpawn
    {
        public static void Postfix(Room __1)
        {
            Room room = __1;
            if (room is null) return;

            if (!ModResources.RoomIsEmpty(room, null)) return;
            
            bool done = false;
            uint attempts = 0;
            while(!done)
            {
                try
                {
                    foreach (Thing thing in room.ContainedAndAdjacentThings)
                    {
                        if (thing is Building building && ModResources.CanBeLight(building))
                        {
                            CompPowerTrader powerTrader = building.PowerComp as CompPowerTrader;
                            ThingComp glower = ModResources.GetGlower(building);
                            if (powerTrader is null || glower is null) continue;

                            LightObject light = new LightObject(powerTrader, glower);
                            ModResources.DisableLight(light);
                        }
                    }
                    done = true;
                }
                // this is thrown if another thread changes the room.ContainedAndAdjacentThings collection
                catch (InvalidOperationException) { ++attempts; }
            }
            if (attempts > 0)
                Log.Warning($"[LightsOut](FloodAndSetRooms): collection was unexpectedly modified {attempts} time(s). If this number is big please report it.");
        }
    }
}
