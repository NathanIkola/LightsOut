//************************************************
// Re-enable the lights when a pawn wakes up
//************************************************

using HarmonyLib;
using LightsOut.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    //[HarmonyPatch(typeof(Pawn_JobTracker))]
    //[HarmonyPatch("CheckForJobOverride")]
    public class EnableLightsOnWake
    {
        private static Dictionary<Pawn, bool> PawnWasWokenUp { get; set; } = new Dictionary<Pawn, bool>();

        public static void Prefix(Pawn_JobTracker __instance, ref JobDriver_LayDown __state)
        {
            __state = __instance.curDriver as JobDriver_LayDown;
        }

        public static void Postfix(Pawn_JobTracker __instance, JobDriver_LayDown __state)
        {
            if((__instance.curDriver is JobDriver_LayDown layDown) && __instance.curDriver == __state)
            {
                if (layDown.asleep && PawnWasWokenUp.TryGetValue(layDown.pawn, true))
                {
                    PawnWasWokenUp.SetOrAdd(layDown.pawn, false);
                }

                if(!layDown.asleep && !PawnWasWokenUp.TryGetValue(layDown.pawn, false))
                {
                    Room room = layDown.pawn.GetRoom();
                    DetectPawnRoomChange.EnableAllLights(room);
                    PawnWasWokenUp.SetOrAdd(layDown.pawn, true);
                }
            }
        }
    }
}