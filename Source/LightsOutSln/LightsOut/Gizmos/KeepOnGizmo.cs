//************************************************
// Shows the button in a light that decides if
// it is always on or subject to the light 
// flicking by pawns
//************************************************

using LightsOut.ThingComps;
using RimWorld;
using Verse;

namespace LightsOut.Gizmos
{
    public class KeepOnGizmo : Command_Toggle
    {
        public KeepOnGizmo(KeepOnComp parentComp)
        {
            ParentComp = parentComp;
            defaultLabel = "Keep On";
            defaultDesc = "Prevent pawns from turning this light off.";
            icon = Widgets.GetIconFor(ThingDefOf.StandingLamp);
            isActive = () => ParentComp.KeepOn;
            toggleAction = () => { ToggleAction(); };
            order = 420;
        }

        //****************************************
        // Toggle the enabled-ness of this comp
        // and then enable/disable the light
        // as necessary
        //****************************************
        private void ToggleAction()
        {
            ParentComp.KeepOn = !ParentComp.KeepOn;
        }

        public KeepOnComp ParentComp { get; set; }
    }
}