using Verse;
using RimWorld;
using LightsOut.Common;

namespace LightsOut.ThingComps
{
    /// <summary>
    /// Turns on Loudspeakers when a ritual starts
    /// </summary>
    public class LoudspeakerTurnOffComp : ThingComp
    {
        /// <summary>
        /// Initializes the comp
        /// </summary>
        /// <param name="parent">The Thing that owns this comp</param>
        public LoudspeakerTurnOffComp(ThingWithComps parent) 
            : base() 
        {
            this.parent = parent;
        }

        /// <summary>
        /// Checks when a <see cref="Signal"/> is received
        /// so that it can detect when a ritual has started
        /// </summary>
        /// <param name="signal">The signal received</param>
        public override void Notify_SignalReceived(Signal signal)
        {
            if (signal.tag == "RitualStarted")
                Tables.EnableTable(parent as Building);
        }
    }
}