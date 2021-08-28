//************************************************
// RemoveCryptoSleepSicknessPatch.cs
//
// Removes cryptosleep sickness from pawns
// but shouldn't be active in release mode
//************************************************

using HarmonyLib;
using Verse;

#if DEBUG
namespace LightsOut.Patches
{
    //Removes Cryptosleep sickness from newly spawned pawns because its annoying
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("TickRare")]
    public class RemoveCryptoSleepSicknessPatch
    {
        public static void Postfix(Pawn __instance)
        {
            if(__instance.RaceProps.Humanlike && __instance.Position.x != -1000)
            {
                var comp = __instance.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("CryptosleepSickness"))?.TryGetComp<HediffComp_Disappears>();

                if(comp != null)
                {
                    comp.ticksToDisappear = 0;
                }
            }
        }
    }
}
#endif