using LightsOut.Common;
using System;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Patches.ModCompatibility.GLWallLights
{
    /// <summary>
    /// Patch the IsInRoom method to reflect the actual Room a Wall Light is in
    /// </summary>
    public class PatchGetRoomForVEWallLights : ICompatibilityPatchComponent
    {
        public override string ComponentName => "Patch GetRoom for GL Wall Lights";
        public override string TypeNameToPatch => nameof(Rooms);
        public override bool TargetsMultipleTypes => false;
        public override bool TypeNameIsExact => true;

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod(typeof(Rooms), nameof(Rooms.GetRoom)),
                patch = GetMethod<PatchGetRoomForGLWallLights>(nameof(GetRoomPatch)),
                patchType = PatchType.Postfix
            };

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Check if <paramref name="__0"/> is a Wall Light, and if
        /// so, change the room that GetRoom returns to be the correct one
        /// </summary>
        /// <param name="__0">The ThingWithComps to check</param>
        /// <param name="__result">The Room that <paramref name="__0"/> is actually in</param>
        private static void GetRoomPatch(ThingWithComps __0, ref Room __result)
        {
            if (__0 is null) return;
            if (__0.def.defName.StartsWith("GL_Wall")
                && (__0.Map?.regionAndRoomUpdater?.Enabled ?? false))
                __result = RegionAndRoomQuery.RoomAt(__0.Position + __0.Rotation.FacingCell, __0.Map);
        }
    }
}
