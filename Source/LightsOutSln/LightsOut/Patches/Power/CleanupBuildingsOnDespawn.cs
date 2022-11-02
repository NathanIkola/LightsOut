using Verse;
using HarmonyLib;
using LightsOut.Common;

namespace LightsOut.Patches.Power
{
    /// <summary>
    /// Cleans up buildings as they despawn
    /// </summary>
    [HarmonyPatch(typeof(Building))]
    [HarmonyPatch(nameof(Building.DeSpawn))]
    public class CleanupBuildingsOnDespawn
    {
        /// <summary>
        /// Removes this building from all cached areas in the mod
        /// </summary>
        /// <param name="__instance">This instance</param>
        public static void Postfix(ThingWithComps __instance)
        {
            Resources.RemoveCachedBuilding(__instance);
            Common.Lights.RemoveLight(__instance);
            Glowers.RemoveCachedGlower(__instance);
        }
    }
}