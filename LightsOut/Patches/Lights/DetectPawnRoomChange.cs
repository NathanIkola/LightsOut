//************************************************
// Detect if a pawn is entering or leaving a
// room and activate or deactivate any lights
// as necessary
//************************************************

using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using LightsOut.Utility;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Tick")]
    public class DetectPawnRoomChange
    {
        public static void Prefix(Pawn __instance, ref Room __state)
        {
            __state = __instance.GetRoom();
        }

        public static void Postfix(Pawn __instance, Room __state)
        {
            // animals PLEASE don't turn on my lights
            if (!__instance.RaceProps.Humanlike) return;

            Room newRoom = __instance.GetRoom();

            // if there was a room change and the room is larger than 1 cell
            if(newRoom != __state)
            {
                if(ModResources.RoomIsEmpty(__state, __instance))
                    DisableAllLights(__state);

                EnableAllLights(newRoom);
            }
        }

        //****************************************
        // Disable all lights in a room
        //****************************************
        private static void DisableAllLights(Room room)
        {
            if (room is null) return;

            foreach(Thing t in room.ContainedAndAdjacentThings)
            {
                if (t is Building thing)
                {
                    KeyValuePair<CompPowerTrader, ThingComp>? light;
                    if ((light = ModResources.GetLightResources(thing)) is null) continue;

                    if (!ModResources.IsInRoom(thing, room)) continue;

                    CompPowerTrader powerTrader = light?.Key;
                    ThingComp glower = light?.Value;

                    ModResources.SetConsumesPower(powerTrader, false);
                    ModResources.SetCanGlow(glower, false);
                }
            }
        }

        //****************************************
        // Enable all lights in a room
        //****************************************
        private static void EnableAllLights(Room room)
        {
            if (room is null) return;

            foreach(Thing t in room.ContainedAndAdjacentThings)
            {
                if (t is Building thing)
                {
                    KeyValuePair<CompPowerTrader, ThingComp>? light;
                    if ((light = ModResources.GetLightResources(thing)) is null) continue;

                    if (!ModResources.IsInRoom(thing, room)) continue;

                    CompPowerTrader powerTrader = light?.Key;
                    ThingComp glower = light?.Value;

                    ModResources.SetConsumesPower(powerTrader, true);
                    ModResources.SetCanGlow(glower, true);
                }
            }
        }
    }
}