//************************************************
// Take care of the mod settings
//************************************************

using System.Collections.Generic;
using HugsLib;
using LightsOut.Utility;
using RimWorld;
using Verse;

namespace LightsOut.Boilerplate
{
    public class ModSettings : ModBase
    {
        //****************************************
        // Control whether or not pawns
        // flick the lights on and off
        //****************************************
        public static bool FlickLights { get; set; } = true;

        public override string ModIdentifier
        {
            get { return "LightsOut"; }
        }

        public override void DefsLoaded()
        {
            SettingsChanged();
        }

        public override void SettingsChanged()
        {
            bool lightsOut = Settings.GetHandle<bool>(
                "EverythingButLightsOut",
                "Turn off lights in empty rooms?",
                "If you turn this off, this mod becomes \"LightsOn\".",
                true);

            DoFlickLightsChange(lightsOut);
            FlickLights = lightsOut;
        }

        //****************************************
        // Do the relevant stuff if the status
        // of FlickLights is changing
        //****************************************
        private void DoFlickLightsChange(bool newVal)
        {
            if (FlickLights == newVal) return;

            FlickLights = newVal;

            var affectedLights = new List<KeyValuePair<CompPowerTrader, ThingComp>?>();
            foreach (var kv in ModResources.BuildingStatus)
            {
                Thing thing = kv.Key;
                var light = ModResources.GetLightResources(thing as Building);
                if (light != null)
                    affectedLights.Add(light);
            }

            if (!FlickLights)
            {
                foreach (var light in affectedLights)
                {
                    CompPowerTrader powerTrader = light?.Key;
                    ThingComp glower = light?.Value;
                    ModResources.SetConsumesPower(powerTrader, true);
                    ModResources.SetCanGlow(glower, true);
                }
            }
            else
            {
                foreach (var light in affectedLights)
                {
                    CompPowerTrader powerTrader = light?.Key;
                    ThingComp glower = light?.Value;

                    if (ModResources.RoomIsEmpty(ModResources.GetRoom(glower.parent as Building), null))
                    {
                        ModResources.SetConsumesPower(powerTrader, false);
                        ModResources.SetCanGlow(glower, false);
                    }
                    else
                    {
                        ModResources.SetConsumesPower(powerTrader, true);
                        ModResources.SetCanGlow(glower, true);
                    }
                }
            }
        }
    }
}
