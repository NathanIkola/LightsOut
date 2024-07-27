using LightsOut.Common;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.SOS2
{
    /// <summary>
    /// Patch MoveShip to refresh lights after a move
    /// </summary>
    public class FixLightsAfterMovingShip : ICompatibilityPatchComponent
    {
        public override string TypeNameToPatch => "ShipInteriorMod2";

        public override bool TargetsMultipleTypes => false;

        public override bool TypeNameIsExact => true;

        public override string ComponentName => "Patch MoveShip to turn off lights after moving";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod(type, "MoveShip"),
                patch = GetMethod<FixLightsAfterMovingShip>(nameof(Postfix)),
                patchType = PatchType.Postfix
            };

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Turns off all lights by room after a ship move occurs
        /// </summary>
        public static void Postfix()
        {
            List<Room> roomsToCheck = new List<Room>();

            // loop over all lights and split them up by room so we only have to check a room once
            // you can't do all the work in this loop because it would add to the Resources dictionary
            // and throw an exception
            foreach (var kv in Resources.BuildingStatus)
            {
                ThingWithComps thing = kv.Key;
                if (thing is null)
                    continue;

                if (!Common.Lights.CanBeLight(thing)) 
                    continue;

                Room room = Rooms.GetRoom(thing);
                if (room is null)
                    continue;

                if (!roomsToCheck.Contains(room))
                {
                    roomsToCheck.Add(room);
                }
            }
            // loop over the rooms we found and enable/disable things as necessary
            foreach (Room room in roomsToCheck)
            {
                if (Common.Lights.ShouldTurnOffAllLights(room, null))
                    Common.Lights.DisableAllLights(room);
                else
                    Common.Lights.EnableAllLights(room);
            }
        }
    }
}