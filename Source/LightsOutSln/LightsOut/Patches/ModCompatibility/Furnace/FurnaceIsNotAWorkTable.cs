//************************************************
// Sam_ used the Building_WorkTable_HeatPush
// thingClass when he made his furnaces, which
// was probably the right choice for him, but it
// makes this much more complicated, as I can't
// rule out that class without also getting rid
// of stoves, crematoriums, and smelters
//************************************************

using HarmonyLib;
using LightsOut.Common;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Furnace
{
    [HarmonyPatch(typeof(Tables))]
    [HarmonyPatch(nameof(Tables.IsTable))]
    public class FurnaceIsNotAWorkTable
    {
        public static void Postfix(Building thing, ref bool __result)
        {
            // if we were going to return true
            if (__result)
                // set result to false if this is a Furnace
                __result = !thing.def.defName.Contains("Furnace");
        }
    }
}
