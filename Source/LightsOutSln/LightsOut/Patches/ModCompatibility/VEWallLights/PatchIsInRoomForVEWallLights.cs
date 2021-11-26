//************************************************
// Patch the IsInRoom method in 
// ModResources to reflect the actual
// position of the Wall Light objects
//************************************************

using LightsOut.Utility;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.VEWallLights
{
    public class PatchIsInRoomForVEWallLights : IModCompatibilityPatch
    {
        protected override string TypeNameToPatch { get => "ModResources"; }
        protected override bool TargetsMultipleTypes { get => false; }
        protected override bool TypeNameIsExact { get => true; }
        protected override string PatchName { get => "VE Wall Lights Mod"; }

        protected override IEnumerable<PatchInfo> GetPatches()
        {
            PatchInfo getRoomPatch = new PatchInfo();
            getRoomPatch.method = typeof(ModResources).GetMethod("GetRoom", BindingFlags);
            getRoomPatch.patch = GetType().GetMethod("GetRoomPatch", BindingFlags);
            getRoomPatch.patchType = PatchType.Postfix;

            return new List<PatchInfo>() { getRoomPatch };
        }

        private static void GetRoomPatch(Building __0, ref Room __result)
        {
            if (__0 is null) return;
            if (__0.def.defName.StartsWith("VFEA_WallLamp"))
                __result = RegionAndRoomQuery.RoomAt(__0.Position + __0.Rotation.FacingCell, __0.Map);
        }
    }
}