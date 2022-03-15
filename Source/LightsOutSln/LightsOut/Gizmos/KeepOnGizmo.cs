using LightsOut.ThingComps;
using RimWorld;
using Verse;

namespace LightsOut.Gizmos
{
    /// <summary>
    /// The gizmo for the KeepOnComp
    /// (the thing you click to keep a light on)
    /// </summary>
    public class KeepOnGizmo : Command_Toggle
    {
        /// <summary>
        /// Constructor that sets up the gizmo
        /// </summary>
        /// <param name="parentComp">The <see cref="KeepOnComp"/> this gizmo belongs to</param>
        public KeepOnGizmo(KeepOnComp parentComp)
        {
            ParentComp = parentComp;
            defaultLabel = "LightsOut_Gizmos_KeepOnLabel".Translate();
            defaultDesc = "LightsOut_Gizmos_KeepOnTooltip".Translate();
            icon = Widgets.GetIconFor(ThingDefOf.StandingLamp);
            isActive = () => ParentComp.KeepOn;
            toggleAction = () => { ToggleAction(); };
            order = 420;
        }

        /// <summary>
        /// The action to take when the gizmo is toggled
        /// </summary>
        private void ToggleAction()
        {
            ParentComp.KeepOn = !ParentComp.KeepOn;
        }

        /// <summary>
        /// The <see cref="KeepOnComp"/> this gizmo belongs to
        /// </summary>
        public KeepOnComp ParentComp { get; set; }
    }
}