using System;
using System.Collections.Generic;
using LightsOut.Gizmos;
using LightsOut.Common;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.ThingComps
{
    /// <summary>
    /// Allows a light to be tagged as "Always On" so that it is
    /// unaffected by light flicking
    /// </summary>
    public class KeepOnComp : ThingComp
    {
        /// <summary>
        /// Default constructor that creates
        /// the correct Gizmo for this comp
        /// </summary>
        public KeepOnComp() : base()
        {
            Gizmo = new KeepOnGizmo(this);
        }

        /// <summary>
        /// The backing field to KeepOn
        /// </summary>
        private bool m_keepOn = false;

        /// <summary>
        /// Whether or not this light is being kept on
        /// </summary>
        public bool KeepOn
        {
            get { return m_keepOn; }
            set
            {
                m_keepOn = value;

                try
                {
                    Building building = parent as Building;
                    Room room = Rooms.GetRoom(building);

                    DebugLogger.AssertFalse(parent is null, "KeepOnComp found a null parent");
                    DebugLogger.AssertFalse(building is null, $"A KeepOnComp can was applied to {parent?.def?.defName}, which wasn't a Building.");
                    DebugLogger.AssertFalse(room is null, $"A KeepOnComp was applied to {parent?.def?.defName}, which had no associated Room.");

                    if (value || room.OutdoorsForWork || !Lights.ShouldTurnOffAllLights(room, null))
                        Lights.EnableLight(building);
                    else
                        Lights.DisableLight(building);
                }
                catch(Exception e)
                {
                    DebugLogger.LogWarning($"Caught error of type: {e.GetType()} in KeepOnComp.");
                }
            }
        }

        /// <summary>
        /// Facilitates saving and loading of the KeepOn status
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.m_keepOn, "keepItOn", false, false);
        }

        /// <summary>
        /// The gizmo that allows users to interact with this comp
        /// </summary>
        public KeepOnGizmo Gizmo { get; set; }

        /// <summary>
        /// Retrieves a list of gizmos associated with this comp
        /// (only the one KeepOnGizmo)
        /// </summary>
        /// <returns>A list of Gizmos to display on the light</returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent is Building building && Lights.CanBeLight(building))
            {
                if (ModSettings.FlickLights)
                    yield return Gizmo;
            }
            yield break;
        }
    }
}