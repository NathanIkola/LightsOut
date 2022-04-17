using LightsOut.Patches.Power;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace LightsOut.Patches.ModCompatibility.QuestionableEthics
{
    /// <summary>
    /// Disable the power draw on any vat if it is not working
    /// </summary>
    public class PatchDisablePowerDraw : ICompatibilityPatchComponent<PatchDisablePowerDraw>
    {
        public override string ComponentName => "Patch DisablePowerDraw for Vats";
        
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            PatchInfo patch = new PatchInfo
            {
                method = GetMethod<DisableBasePowerDrawOnGet>(nameof(DisableBasePowerDrawOnGet.Postfix)),
                patch = GetMethod<PatchDisablePowerDraw>(nameof(PrefixPatch)),
                patchType = PatchType.Prefix
            };

            return new List<PatchInfo>() { patch };
        }

        /// <summary>
        /// Memoized list of PropertyInfos for the different vats
        /// </summary>
        private static Dictionary<Type, PropertyInfo> VatCraftingPercentProperties { get; set; } = new Dictionary<Type, PropertyInfo>();

        /// <summary>
        /// Memoized list of whether or not things are vats
        /// to speed up subsequent calls
        /// </summary>
        private static Dictionary<CompPowerTrader, bool> MemoizedIsVat { get; } = new Dictionary<CompPowerTrader, bool>();

        /// <summary>
        /// Checks if a CompPowerTrader belongs to a vat,
        /// and if so checks if it is currently working.
        /// </summary>
        /// <param name="__0">The CompPowerTrader to check for</param>
        /// <returns><see langword="true"/> if the CompPowerTrader did not
        /// belong to a vat that was actively working,
        /// <see langword="false"/> if it belongs to an actively working vat</returns>
        private static bool PrefixPatch(CompPowerTrader __0)
        {
            if (__0 is null || __0.parent is null) return true;
            
            ThingWithComps parent = __0.parent;
            Type parentType = parent.GetType();

            if (!MemoizedIsVat.ContainsKey(__0))
                MemoizedIsVat.Add(__0, parentType.Name.Contains("Vat"));

            if (!MemoizedIsVat[__0]) return true;
            
            if (!VatCraftingPercentProperties.ContainsKey(parentType))
            {
                PropertyInfo craftingProgressPercent = parentType.GetProperty("CraftingProgressPercent");
                VatCraftingPercentProperties.Add(parentType, craftingProgressPercent);
            }

            MethodInfo craftingProgressPercentGetter = VatCraftingPercentProperties[parentType]?.GetGetMethod();
            if (craftingProgressPercentGetter is null) 
                return true;

            float craftingProgress;
            try
            {
                craftingProgress = (float)craftingProgressPercentGetter.Invoke(parent, new object[] { });
            }
            // crappy QE code throws an exception if you try to get
            // the crafting percent when it isn't crafting
            // on the bright side that means we know it isn't crafting
            catch (Exception) { return true; }
            
            // progress >0% and <100% is working
            if (craftingProgress > 0f && craftingProgress < 1f)
                return false;
            return true;
        }
    }
}