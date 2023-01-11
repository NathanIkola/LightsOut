using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    public class BiotechCompatibilityPatch : ICompatibilityPatch
    {
        
        public override string CompatibilityPatchName => "Biotech";
        public override string TargetMod => "Biotech";

        public override IEnumerable<ICompatibilityPatchComponent> GetComponents()
        {
            List<ICompatibilityPatchComponent> components = new List<ICompatibilityPatchComponent>()
            {
                new PatchMechCharger(),
                new PatchMechGestator(),
                new PatchGeneAssembly(),
                new PatchGrowthVat(),
                new PatchBandNode(),
                new PatchGeneExtractor(),
                new PatchSubscoreScanner(),
                new PatchWasteAtomiser(),
            };

            return components;
        }
    }
}