using Verse;

namespace LightsOut.Boilerplate
{
    /// <summary>
    /// Initializes the mod after all other mods have loaded
    /// </summary>
    [StaticConstructorOnStartup]
    public static class LateInitializeMod
    {
        /// <summary>
        /// Causes the late initialization of the mod, after all other mods have loaded
        /// </summary>
        static LateInitializeMod()
        {
            LightsOutMod.InitializeMod();
        }
    }
}