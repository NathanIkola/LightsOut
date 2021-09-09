//************************************************
// Keeps a light on all the time if it's enabled
// no matter what the pawns want to do with it
//************************************************

using System;
using System.Collections.Generic;
using LightsOut.Gizmos;
using LightsOut.Utility;
using RimWorld;
using Verse;
using ModSettings = LightsOut.Boilerplate.ModSettings;

namespace LightsOut.ThingComps
{
    using LightObject = KeyValuePair<CompPowerTrader, ThingComp>;

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
                    CompPowerTrader powerTrader = thing.PowerComp as CompPowerTrader;
                    ThingComp glower = ModResources.GetGlower(thing);
                    LightObject obj = new LightObject(powerTrader, glower);
                    Room room = ModResources.GetRoom(thing);

                    ModResources.DisableLight(obj);
                    if (value || !ModResources.RoomIsEmpty(room, null))
                        ModResources.EnableLight(obj);
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
            if (parent is Building building && ModResources.CanBeLight(building))
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