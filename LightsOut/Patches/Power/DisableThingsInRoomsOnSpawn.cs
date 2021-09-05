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

            foreach(Thing thing in room.ContainedAndAdjacentThings)
            {
                if(thing is Building building && ModResources.CanBeLight(building))
                {
                    CompPowerTrader powerTrader = building.PowerComp as CompPowerTrader;
                    ThingComp glower = ModResources.GetGlower(building);
                    if (powerTrader is null || glower is null) continue;

                    LightObject light = new LightObject(powerTrader, glower);
                    ModResources.DisableLight(light);
                }
            }
        }
    }
}
