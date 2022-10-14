using LightsOut.Common;
using LightsOut.Patches.ModCompatibility;
using LightsOut.ThingComps;
using RimWorld;
using System.Reflection;
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

            FieldInfo order = typeof(KeepOnGizmo).GetField("order", ICompatibilityPatchComponent.BindingFlags);
            int orderVal = 420;
            if (order is null)
            {
                PropertyInfo Order = typeof(KeepOnGizmo).GetProperty("Order", ICompatibilityPatchComponent.BindingFlags);
                if (Order is null)
                {
                    DebugLogger.LogWarning("Failed to get the order field/property for a gizmo");
                }
                else
                {
                    Order.SetValue(this, orderVal);
                }
            }
            else
            {
                order.SetValue(this, orderVal);
            }
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