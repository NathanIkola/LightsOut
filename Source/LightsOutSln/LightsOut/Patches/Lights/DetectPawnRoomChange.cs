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
using System;

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
            if (!__instance.RaceProps.Humanlike || !ModSettings.FlickLights) return;

            Room newRoom = __instance.GetRoom();

            // if there was a room change
            if(newRoom != __state)
            {
                if(ModResources.RoomIsEmpty(__state, __instance)
                    || (!ModSettings.NightLights && ModResources.AllPawnsSleeping(__state, __instance)))
                    ModResources.DisableAllLights(__state);

                if(ModResources.RoomIsEmpty(newRoom, __instance)
                    || (!ModSettings.NightLights && ModResources.AllPawnsSleeping(newRoom, __instance)))
                    ModResources.EnableAllLights(newRoom);
            }
        }
    }
}