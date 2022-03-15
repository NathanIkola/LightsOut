using RimWorld;
using Verse;

namespace LightsOut.Defs
{
    /// <summary>
    /// The custom list of ThingDefs the mod needs to work
    /// </summary>
    [DefOf]
    public static class CustomThingDefs
    {
        /// <summary>
        /// Constructor that makes sure that the ThingDefs get
        /// initialized on startup
        /// </summary>
        static CustomThingDefs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CustomThingDefs));
        }

        public static ThingDef TubeTelevision;
        public static ThingDef FlatscreenTelevision;
        public static ThingDef MegascreenTelevision;
    }
}