//************************************************
// Patch the IsInRoom method in 
// ModResources to reflect the actual
// position of the Wall Light objects
//************************************************

using LightsOut.Common;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.VEWallLights
{
    public class PatchGetRoomForVEWallLights : ICompatibilityPatchComponent
    {
        public override string ComponentName => "Patch GetRoom for VE Wall Lights";
        public override string TypeNameToPatch => "Rooms";
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo();
            patch.method = GetMethod(typeof(Rooms), "GetRoom");
            patch.patch = GetMethod<PatchGetRoomForVEWallLights>("GetRoomPatch");
            patch.patchType = PatchType.Postfix;

            return new List<PatchInfo>() { patch };
        }

        private static void GetRoomPatch(Building __0, ref Room __result)
        {
            if (__0 is null) return;
            if (__0.def.defName.StartsWith("VFEA_WallLamp") && (__0.Map?.regionAndRoomUpdater?.Enabled ?? false))
                __result = RegionAndRoomQuery.RoomAt(__0.Position + __0.Rotation.FacingCell, __0.Map);
        }
    }
}