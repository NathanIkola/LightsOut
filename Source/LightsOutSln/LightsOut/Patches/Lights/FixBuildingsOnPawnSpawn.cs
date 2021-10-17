﻿//************************************************
// Squash a bug I've neglected regarding what
// happens when a pawn spawns in. Without this
// patch, if a pawn spawns in the middle of
// using a bench, the bench will still be in
// standby since the pather never "arrived"
// since the pawn never pathed to it
//
// Additionally, it will kill the lights after
// the pawn spawns so that the load state
// matches what you'd expect
//************************************************

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LightsOut.Patches.ModCompatibility;
using LightsOut.Utility;
using Verse;
using Verse.AI;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.Patches.Lights
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.SpawnSetup))]
    public class FixBuildingsOnPawnSpawn
    {
        public static void Postfix(Pawn __instance)
        {
            JobDriver driver = __instance.jobs?.curDriver;
            if (driver is null)
                return;

            // get the current toil out of the driver
            PropertyInfo curToil = typeof(JobDriver).GetProperty("CurToil", IModCompatibilityPatch.BindingFlags);
            Toil toil = (Toil)curToil.GetValue(driver);
            if (toil is null)
                return;

            // turn the power on the bench back on if the pawn is using it
            if (driver is JobDriver_DoBill doBill
                && doBill.BillGiver is Building building
                && ModResources.IsTable(building))
            {
                toil.AddPreTickAction(() => 
                {
                    if(!(bool)ModResources.CanConsumePower(building.PowerComp))
                        ModResources.EnableTable(building); 
                });

                toil.AddFinishAction(() => { ModResources.DisableTable(building); });
            }
            else if(driver.asleep && !ModSettings.NightLights)
            {
                // ask the pawn to nicely turn off the lights when they start sleeping
                toil.AddPreTickAction(() =>
                {
                    Pawn pawn = toil.actor;

                    // otherwise we know we are sleepy
                    if (!Sleepers.ContainsKey(pawn))
                    {
                        Sleepers.Add(pawn, true);
                        Room room = pawn.GetRoom();

                        if (room.OutdoorsForWork)
                            return;

                        if (ModResources.RoomIsEmpty(room, pawn)
                        || (ModResources.AllPawnsSleeping(room, pawn)))
                        {
                            ModResources.DisableAllLights(room);
                        }
                        toil.AddFinishAction(() => { ModResources.EnableAllLights(room); });
                    }
                });
            }
        }

        private static Dictionary<Pawn, bool> Sleepers = new Dictionary<Pawn, bool>();
    }
}