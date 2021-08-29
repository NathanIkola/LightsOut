//************************************************
// Shut off the lights when the pawn goes to bed
//************************************************

using HarmonyLib;
using LightsOut.Utility;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(JobDriver))]
    [HarmonyPatch("Notify_PatherArrived")]
    public class DisableLightsOnSleep
    {
        public static void Postfix(JobDriver __instance)
        {
            Pawn pawn = __instance.GetActor();

            if (pawn is null || ModSettings.NightLights) return;
            if (pawn.CurJobDef != JobDefOf.LayDown) return;

            Room room = pawn.GetRoom();
            List<Pawn> pawnsInThisRoom = ModResources.GetPawnsInRoom(room);
            
            // if any pawns are in the room but NOT laying down, exit
            foreach(Pawn otherPawn in pawnsInThisRoom)
                if (otherPawn != pawn && otherPawn.Awake()) return;

            // disable all of the lights in the room
            DetectPawnRoomChange.DisableAllLights(room);

            // disable them again when the pawn wakes up
            __instance.AddFinishAction(() =>
            {
                DetectPawnRoomChange.EnableAllLights(room);
            });
        }
    }
}
