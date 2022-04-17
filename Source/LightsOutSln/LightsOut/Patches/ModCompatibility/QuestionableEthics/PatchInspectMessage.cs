using System;
using System.Collections.Generic;
using System.Reflection;
using LightsOut.Patches.Power;
using RimWorld;

namespace LightsOut.Patches.ModCompatibility.QuestionableEthics
{
    /// <summary>
    /// Fix the inspect message for vats
    /// </summary>
    public class PatchInspectMessage : ICompatibilityPatchComponent<AddStandbyInspectMessagePatch>
    {
        public override string ComponentName => "Patch AddStandbyInspectMessage for Questionable Ethics vats";

        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod<AddStandbyInspectMessagePatch>(nameof(AddStandbyInspectMessagePatch.Postfix)),
                patch = GetMethod<PatchInspectMessage>(nameof(PrefixPatch)),
                patchType = PatchType.Prefix
            };

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Memoized version of the method for getting the crafting progress
        /// </summary>
        /// <remarks>Also could have used the status member, but the docs say it's being deprecated</remarks>
        private static PropertyInfo m_craftingProgressPercent = null;

        /// <summary>
        /// Fixes the inspect message for vats
        /// </summary>
        /// <param name="__0">The CompPower to fix the message of</param>
        /// <returns><see langword="true"/> if the vat is working,
        /// <see langword="false"/> otherwise</returns>
        private static bool PrefixPatch(CompPowerTrader __0)
        {
            if (__0 is null || __0.parent is null) return true;

            if (__0.parent.GetType().Name.Contains("Vat"))
            {
                if (m_craftingProgressPercent is null)
                    m_craftingProgressPercent = __0.parent.GetType().GetProperty("CraftingProgressPercent");

                float craftingProgress;
                try
                {
                    craftingProgress = (float)m_craftingProgressPercent.GetGetMethod().Invoke(__0.parent, null);
                }
                // crappy QE code throws an exception if you try to get
                // the crafting percent when it isn't crafting
                // on the bright side that means we know it isn't crafting
                catch (Exception) { return true; }

                // progress >0% and <100% is working
                if (craftingProgress > 0f && craftingProgress < 1f) return false;
            }
            return true;
        }
    }
}