//************************************************
// Second attempt at a compatibility system 
// that hopefully doesn't suck like the first
// one kinda did
//************************************************

using System.Collections.Generic;

namespace LightsOut.Patches.ModCompatibility
{
    public abstract class ICompatibilityPatch
    {
        //****************************************
        // Returns a list of compatibility
        // components to apply
        //****************************************
        public abstract IEnumerable<ICompatibilityPatchComponent> GetComponents();

        //****************************************
        // The mod whose presence causes this
        // patch to take effect, or null to
        // always apply it
        //****************************************
        public virtual string TargetMod => null;

        //****************************************
        // The name of this compatibility patch
        //****************************************
        public abstract string CompatibilityPatchName { get; }
    }
}