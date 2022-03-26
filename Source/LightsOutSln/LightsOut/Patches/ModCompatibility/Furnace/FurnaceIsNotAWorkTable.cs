using HarmonyLib;
using LightsOut.Common;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Furnace
{
    /// <summary>
    /// Fixes the Furnace mod to not get detected as a table
    /// </summary>
    [HarmonyPatch(typeof(Tables))]
    [HarmonyPatch(nameof(Tables.IsTable))]
    public class FurnaceIsNotAWorkTable
    {
        /// <summary>
        /// Hijacks the IsTable method to rule out the Furnace
        /// </summary>
        /// <param name="thing">The ThingWithComps to check</param>
        /// <param name="__result">False if it's a furnace, unchanged otherwise</param>
        public static void Postfix(ThingWithComps thing, ref bool __result)
        {
            // if we were going to return true
            if (__result)
                // set result to false if this is a Furnace
                __result = !thing.def.defName.Contains("Furnace");
        }
    }
}