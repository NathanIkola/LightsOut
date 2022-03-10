//************************************************
// Take care of the mod settings
//************************************************

using System.Collections.Generic;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using LightsOut.Common;
using LightsOut.Patches.ModCompatibility;
using RimWorld;
using Verse;

namespace LightsOut.Boilerplate
{
    public class ModSettings : ModBase
    {
        public ModSettings()
        {
            Instance = this;
        }

        //****************************************
        // The Harmony instance of this mod
        //****************************************
        public static Harmony Harmony => Instance.HarmonyInst;

        //****************************************
        // The one instantiated instance of
        // this mod (used to get Harmony)
        //****************************************
        private static ModSettings Instance { get; set; }

        //****************************************
        // Control whether or not pawns
        // flick the lights on and off
        //****************************************
        public static bool FlickLights { get; set; } = true;

        //****************************************
        // Latent power draw of things when
        // they are flicked off
        //****************************************
        public static float StandbyPowerDrawRate { get; set; } = 0f;

        //****************************************
        // Power draw of things when in use
        //****************************************
        public static float ActivePowerDrawRate { get; set; } = 0f;

        //****************************************
        // Control whether or not pawns shut off
        // the lights when they go to bed
        //****************************************
        public static bool NightLights { get; set; } = false;

        public override string ModIdentifier
        {
            get { return "LightsOut"; }
        }

        public override void DefsLoaded()
        {
            SettingsChanged();
            ModCompatibilityManager.LoadCompatibilityPatches();
        }

        public override void SettingsChanged()
        {
            bool lightsOut = Settings.GetHandle<bool>(
                "EverythingButLightsOut",
                "Turn off lights in empty rooms?",
                "If you turn this off, this mod becomes \"LightsOn\". On by default.",
                true);

            bool nightLights = Settings.GetHandle<bool>(
                "NightLights",
                "Keep the lights on when your pawns go to bed?",
                "Enable this if your pawns are scared of the dark. Off by default.",
                false);

            uint standbyPower = Settings.GetHandle<uint>(
                "LatentPowerDrawRate",
                "Standby power draw (%)",
                "Percentage of normal power to draw when in standby. 0% by default.",
                0,
                Validators.IntRangeValidator(0, 100));

            uint activePower = Settings.GetHandle<uint>(
                "ActivePowerDrawRate",
                "Power draw when in use (%)",
                "Percentage of normal power to draw when in use. 100% by default.",
                100,
                Validators.IntRangeValidator(100, int.MaxValue));

            StandbyPowerDrawRate = standbyPower / 100f;
            ActivePowerDrawRate = activePower / 100f;

            NightLights = nightLights;

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
            foreach (var kv in Resources.BuildingStatus)
            {
                Thing thing = kv.Key;
                var light = Lights.GetLightResources(thing as Building);
                if (light != null)
                    affectedLights.Add(light);
            }

            if (!FlickLights)
            {
                foreach (var light in affectedLights)
                {
                    Lights.EnableLight(light);
                }
            }
            else
            {
                foreach (var light in affectedLights)
                {
                    ThingComp glower = light?.Value;
                    Room room = Rooms.GetRoom(glower?.parent as Building);

                    if (room == null || room.OutdoorsForWork) return;

                    if (Lights.ShouldTurnOffAllLights(room, null))
                        Lights.DisableLight(light);
                    else
                        Lights.EnableLight(light);
                }
            }
        }
    }
}