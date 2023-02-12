using System.Reflection;
using HarmonyLib;
using LightsOut.Patches.ModCompatibility;
using Verse;
using PatchInfo = LightsOut.Patches.ModCompatibility.PatchInfo;

namespace LightsOut.Common
{
    public static class TablesHelper
    {
        private static void PatchTableOff(ThingWithComps __instance)
        {
            Tables.DisableTable(__instance);
        }

        private static void PatchTableOn(ThingWithComps __instance)
        {
            Log.Message("enable table ");
            Tables.EnableTable(__instance);
        }

        public static PatchInfo OffPatch(MethodInfo target)
        {
            return new PatchInfo
            {
                patch = AccessTools.Method(typeof(TablesHelper), nameof(PatchTableOff)),
                method = target,
                patchType = PatchType.Postfix,
            };
        }
        
        public static PatchInfo OnPatch(MethodInfo target)
        {
            return new PatchInfo
            {
                patch = AccessTools.Method(typeof(TablesHelper), nameof(PatchTableOn)),
                method = target,
                patchType = PatchType.Postfix,
            };
        }
    }
}