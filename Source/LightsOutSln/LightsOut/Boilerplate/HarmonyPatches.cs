//************************************************
// HarmonyPatches.cs
//
// Patch driver for my RimWorld mod
//************************************************

using HarmonyLib;
using Verse;

namespace LightsOut.Boilerplate
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //Harmony.PatchAll();
        }

        public static Harmony Harmony { get; } = new Harmony("LightsOut");
    }
}