//************************************************
// Holds declared ThingDefs
//************************************************

using RimWorld;
using Verse;

namespace LightsOut.Defs
{
    [DefOf]
    public static class MyThingDefOf
    {
        static MyThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MyThingDefOf));
        }

        public static ThingDef TubeTelevision;
        public static ThingDef FlatscreenTelevision;
        public static ThingDef MegascreenTelevision;
    }
}