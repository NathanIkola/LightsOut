//************************************************
// Turns off a Loudspeaker when the signal
// is received
//************************************************

using Verse;
using RimWorld;
using LightsOut.Common;

namespace LightsOut.ThingComps
{
    public class LoudspeakerTurnOffComp : ThingComp
    {
        public LoudspeakerTurnOffComp(ThingWithComps parent) 
            : base() 
        {
            this.parent = parent;
        }

        public override void Notify_SignalReceived(Signal signal)
        {
            if (signal.tag == "RitualStarted")
                Tables.EnableTable(parent as Building);
        }
    }
}