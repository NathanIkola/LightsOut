using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using HarmonyLib;
using LightsOut.Common;
using Verse;

namespace LightsOut.Boilerplate
{
    /// <summary>
    /// The class that initializes the mod and holds all
    /// mod-related settings
    /// </summary>
    public class LightsOutSettings : ModSettings
    {
        /// <summary>
        /// Initializes the settings object
        /// </summary>
        public LightsOutSettings()
        {
            // the mod used to use HugsLib for managing settings, but doesn't anymore
            // but I don't want to make people angry by resetting all of their settings
            // so manually find and parse their old settings as the new defaults so that
            // they don't even notice that I changed anything
            TryRestoreSettingsFromHugsLib();
        }

        /// <summary>
        /// Control whether or not Pawns flick lights on and off
        /// </summary>
        public static bool FlickLights = true;

        /// <summary>
        /// The percentage form of the draw rate
        /// </summary>
        private static int StandbyResourceDrawPercent = 0;

        /// <summary>
        /// Latent power draw of things when they're flicked off
        /// </summary>
        public static float StandbyResourceDrawRate => StandbyResourceDrawPercent / 100f;

        /// <summary>
        /// The percentage form of the draw rate
        /// </summary>
        public static int ActiveResourceDrawPercent = 100;

        /// <summary>
        /// The resource draw rate of things when they're in-use
        /// </summary>
        public static float ActiveResourceDrawRate => ActiveResourceDrawPercent / 100f ;

        /// <summary>
        /// Control whether or not Pawns shut off the lights when
        /// they go to bed
        /// </summary>
        public static bool NightLights = false;

        /// <summary>
        /// If set to true, allows animals to turn on the lights
        /// </summary>
        public static bool AnimalParty = false;

        /// <summary>
        /// A list of message filters to add.
        /// Allows messages based on their debug message key.
        /// </summary>
        public static string[] MessageFilters = { DebugMessageKeys.Error + DebugMessageKeys.Mods };

        /// <summary>
        /// The number of ticks to delay turning a light off by
        /// </summary>
        public static float DelaySeconds = 1.5f;

        /// <summary>
        /// The number of ticks to wait between checking for
        /// buildings with delays to turn off
        /// </summary>
        public static int TicksBetweenShutoffCheck = 30;

        /// <summary>
        /// An easier way to get the number of ticks
        /// </summary>
        public static int DelayTicks => (int)(DelaySeconds * 60f);

        /// <summary>
        /// Draws the settings menu for users to interact with
        /// </summary>
        /// <param name="settingListing">The setting lister to use</param>
        public void DrawSettingsMenu(Listing_Standard settingListing)
        {
            bool flickLights = FlickLights;

            // initialize the input buffers if they haven't been already
            _standbyBuf = _standbyBuf ?? StandbyResourceDrawPercent.ToString();
            _activeBuf = _activeBuf ?? ActiveResourceDrawPercent.ToString();
            _delayBuf = _delayBuf ?? DelaySeconds.ToString();
            _messageFilterBuf = _messageFilterBuf ?? MessageFilters.Join(null, " ");

            settingListing.CheckboxLabeled(
                "LightsOut_Settings_EverythingButLightsOutLabel".Translate(),
                ref flickLights,
                "LightsOut_Settings_EverythingButLightsOutTooltip".Translate()
                );

            settingListing.CheckboxLabeled(
                "LightsOut_Settings_NightLightsLabel".Translate(),
                ref NightLights,
                "LightsOut_Settings_NightLightsTooltip".Translate()
                );

            settingListing.CheckboxLabeled(
                "LightsOut_Settings_AnimalPartyLabel".Translate(),
                ref AnimalParty,
                "LightsOut_Settings_AnimalPartyTooltip".Translate()
                );

            settingListing.TextFieldNumericLabeled(
                "LightsOut_Settings_DelaySecondsLabel".Translate(),
                ref DelaySeconds, ref _delayBuf,
                0, float.MaxValue
                );

            settingListing.TextFieldNumericLabeled(
                "LightsOut_Settings_LatentPowerDrawRateLabel".Translate(),
                ref StandbyResourceDrawPercent, ref _standbyBuf,
                0, 100
                );

            settingListing.TextFieldNumericLabeled(
                "LightsOut_Settings_ActivePowerDrawRateLabel".Translate(),
                ref ActiveResourceDrawPercent, ref _activeBuf,
                0, int.MaxValue
                );

            _messageFilterBuf = settingListing.TextEntryLabeled(
                "LightsOut_Settings_DebugMessageFilterLabel".Translate(),
                _messageFilterBuf,
                1
                );

            MessageFilters = _messageFilterBuf.ToLower().Split(' ');
            UpdateLightGlowersOnSettingChange(flickLights);
            FlickLights = flickLights;
        }

        /// <summary>
        /// Actually handles exposing the settings data on save/load
        /// </summary>
        public override void ExposeData()
        {
            ExposeSettings();
            base.ExposeData();
        }

        /// <summary>
        /// Saves and loads the mod's settings
        /// </summary>
        public void ExposeSettings()
        {
            _messageFilterBuf = _messageFilterBuf ?? MessageFilters.Join(null, " ");

            // clamp the active power draw to at least 100
            if (ActiveResourceDrawPercent < 100)
                ActiveResourceDrawPercent = 100;

            Scribe_Values.Look(ref FlickLights, "EverythingButLightsOut", FlickLights, true);
            Scribe_Values.Look(ref NightLights, "NightLights", NightLights, true);
            Scribe_Values.Look(ref AnimalParty, "AnimalParty", AnimalParty, true);
            Scribe_Values.Look(ref DelaySeconds, "DelaySeconds", DelaySeconds, true);
            Scribe_Values.Look(ref StandbyResourceDrawPercent, "LatentPowerDrawRate", StandbyResourceDrawPercent, true);
            Scribe_Values.Look(ref ActiveResourceDrawPercent, "ActivePowerDrawRate", ActiveResourceDrawPercent, true);
            Scribe_Values.Look(ref _messageFilterBuf, "DebugFilters", _messageFilterBuf, true);

            MessageFilters = _messageFilterBuf.ToLower().Split(' ');

            // reset the buffers so that they get rebuilt the next time the settings window renders
            // do this in case we needed to adjust an input value due to it falling out of range
            // (otherwise the buffer would continue to show the old value that isn't acutally saved)
            _standbyBuf = null;
            _activeBuf = null;
            _delayBuf = null;
        }

        /// <summary>
        /// The string buffer for the standby value
        /// </summary>
        private string _standbyBuf = null;

        /// <summary>
        /// The string buffer for the active value
        /// </summary>
        private string _activeBuf = null;

        /// <summary>
        /// The string buffer for the delay value
        /// </summary>
        private string _delayBuf = null;

        /// <summary>
        /// The string buffer for the message filter
        /// </summary>
        private string _messageFilterBuf = null;

        /// <summary>
        /// Flicks (or unflicks) lights depending on the new value from the settings
        /// </summary>
        /// <param name="newVal">The new value the setting is being set to</param>
        private void UpdateLightGlowersOnSettingChange(bool newVal)
        {
            if (FlickLights == newVal) return;
            DebugLogger.LogInfo($"Changing FlickLights from {FlickLights} to {newVal}", DebugMessageKeys.Settings);

            FlickLights = newVal;

            var affectedLights = new List<ThingComp>();
            foreach (var kv in Resources.BuildingStatus)
            {
                ThingWithComps thing = kv.Key;
                if (!Lights.CanBeLight(thing))
                    continue;

                var light = Glowers.GetGlower(thing);
                if (light != null)
                    affectedLights.Add(light);
            }

            if (!FlickLights)
            {
                foreach (ThingComp glower in affectedLights)
                    Glowers.EnableGlower(glower);
            }
            else
            {
                foreach (ThingComp glower in affectedLights)
                {
                    Room room = Rooms.GetRoom(glower?.parent);

                    if (Lights.ShouldTurnOffAllLights(room, null))
                        Glowers.DisableGlower(glower);
                    else
                        Glowers.EnableGlower(glower);
                }
            }
        }

        #region legacy HugsLib setting support
        /// <summary>
        /// The mod no longer uses HugsLib, but that's where settings used to be stored
        /// 
        /// in order to make the transition easier for users, attempt to load the old
        /// settings from HugsLib into the current settings location as the default values
        /// 
        /// this should effectively restore the settings without overwriting them each time
        /// </summary>
        private void TryRestoreSettingsFromHugsLib()
        {
            // try to locate the HugsLib mod settings
            string hugsLibPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "HugsLib");
            if (!Directory.Exists(hugsLibPath)) { return; }
            string modSettingPath = Path.Combine(hugsLibPath, "ModSettings.xml");
            if (!File.Exists(modSettingPath)) { return; }

            // if it exists, load the old settings
            try
            {
                XDocument doc = XDocument.Load(modSettingPath);
                if (doc.Root != null)
                {
                    foreach (XElement modNode in doc.Root.Elements())
                    {
                        if (modNode.Name != "LightsOut") { continue; }
                        Log.Message("[LightsOut] Found previous settings from HugsLib. Using them as default values.");
                        foreach (XElement settingNode in modNode.Elements())
                        {
                            string value = settingNode.Value;
                            if (settingNode.Name == "EverythingButLightsOut")
                                TryParse(value, ref FlickLights);
                            else if (settingNode.Name == "NightLights")
                                TryParse(value, ref NightLights);
                            else if (settingNode.Name == "AnimalParty")
                                TryParse(value, ref AnimalParty);
                            else if (settingNode.Name == "DelaySeconds")
                                TryParse(value, ref DelaySeconds);
                            else if (settingNode.Name == "LatentPowerDrawRate")
                                TryParse(value, ref StandbyResourceDrawPercent);
                            else if (settingNode.Name == "ActivePowerDrawRate")
                                TryParse(value, ref ActiveResourceDrawPercent);
                            else if (settingNode.Name == "DebugFilters")
                                MessageFilters = value.ToLower().Split(' ');
                        }
                        // no need to loop further, we found our settings
                        break;
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Attempts to parse the value into the specified variable
        /// </summary>
        /// <param name="value">The value to try to parse</param>
        /// <param name="toMod">The variable to put the value into</param>
        private static void TryParse(string value, ref bool toMod)
        {
            if (bool.TryParse(value, out bool result))
            {
                toMod = result;
            }
        }

        /// <summary>
        /// Attempts to parse the value into the specified variable
        /// </summary>
        /// <param name="value">The value to try to parse</param>
        /// <param name="toMod">The variable to put the value into</param>
        private static void TryParse(string value, ref float toMod)
        {
            if (float.TryParse(value, out float result))
            {
                toMod = result;
            }
        }

        /// <summary>
        /// Attempts to parse the value into the specified variable
        /// </summary>
        /// <param name="value">The value to try to parse</param>
        /// <param name="toMod">The variable to put the value into</param>
        private static void TryParse(string value, ref int toMod)
        {
            if (int.TryParse(value, out int result))
            {
                toMod = result;
            }
        }
        #endregion
    }
}