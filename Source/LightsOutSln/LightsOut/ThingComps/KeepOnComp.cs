//************************************************
// Keeps a light on all the time if it's enabled
// no matter what the pawns want to do with it
//************************************************

using System;
using System.Collections.Generic;
using LightsOut.Gizmos;
using LightsOut.Common;
using RimWorld;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.ThingComps
{
    public class KeepOnComp : ThingComp
    {
        public KeepOnComp() : base()
        {
            Gizmo = new KeepOnGizmo(this);
        }

        private bool m_keepOn = false;
        public bool KeepOn
        {
            get
            {
                return m_keepOn;
            }
            set
            {
                m_keepOn = value;

                try
                {
                    Building thing = parent as Building;
                    ThingComp glower = Glowers.GetGlower(thing);
                    Room room = Rooms.GetRoom(thing);

                    Lights.DisableLight(glower);
                    if (value || room.OutdoorsForWork || !Lights.ShouldTurnOffAllLights(room, null))
                        Lights.EnableLight(glower);
                }
                catch(Exception e)
                {
                    Log.Warning($"LightsOut caught error of type: {e.GetType()} in KeepOnComp.");
                }
            }
        }

        //****************************************
        // Make sure to restore our data when we
        // load from a save
        //****************************************
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.m_keepOn, "keepItOn", false, false);
        }


        public KeepOnGizmo Gizmo { get; set; }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent is Building building && Lights.CanBeLight(building))
            {
                if (ModSettings.FlickLights)
                    yield return Gizmo;
            }
            else
            {
                // if it isn't possible, STOP ASKING US
                //parent.AllComps.Remove(this);
            }
            yield break;
        }
    }
}