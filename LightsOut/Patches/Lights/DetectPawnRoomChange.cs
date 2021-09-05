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
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    using LightObject = KeyValuePair<CompPowerTrader, ThingComp>;

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

                if(ModResources.RoomIsEmpty(newRoom, __instance))
                    EnableAllLights(newRoom);
            }
        }

        //****************************************
        // Disable all lights in a room
        //****************************************
        public static void DisableAllLights(Room room)
        {
            if (room is null || !ModSettings.FlickLights) return;

            foreach(Thing t in room.ContainedAndAdjacentThings)
            {
                if (t is Building thing)
                {
                    LightObject? light = ModResources.GetLightResources(thing);

                    if (light is null || !ModResources.IsInRoom(thing, room)) continue;

                    ModResources.DisableLight(light);
                }
            }
        }

        //****************************************
        // Enable all lights in a room
        //****************************************
        public static void EnableAllLights(Room room)
        {
            if (room is null) return;

            foreach(Thing t in room.ContainedAndAdjacentThings)
            {
                if (t is Building thing)
                {
                    LightObject? light = ModResources.GetLightResources(thing);

                    if (light is null || !ModResources.IsInRoom(thing, room)) continue;

                    ModResources.EnableLight(light);
                }
            }
        }
    }
}