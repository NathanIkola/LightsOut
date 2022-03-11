﻿//************************************************
// Fix the speaker on spawn
//************************************************

using LightsOut.Common;
using LightsOut.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace LightsOut.Patches.ModCompatibility.Ideology
{
    public class PatchPostSpawnSetup : ICompatibilityPatchComponent<CompLoudspeaker>
    {
        public override string ComponentName => "Patch PostPawnSetup for Loudspeaker";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo postfix = new PatchInfo();
            postfix.method = GetMethod<CompLoudspeaker>(nameof(CompLoudspeaker.PostSpawnSetup));
            postfix.patch = GetMethod<PatchPostSpawnSetup>(nameof(Postfix));
            postfix.patchType = PatchType.Postfix;

            return new List<PatchInfo>() { postfix };
        }

        private static void Postfix(CompLoudspeaker __instance)
        {
            __instance.parent.AllComps.Add(new LoudspeakerTurnOffComp(__instance.parent));
            if (!__instance.Active)
                Tables.DisableTable(__instance.parent as Building);
            else
                Tables.EnableTable(__instance.parent as Building);
        }
    }
}