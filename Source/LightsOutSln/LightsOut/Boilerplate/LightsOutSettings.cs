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
    public class LightsOutSettings : Verse.ModSettings
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
        /// Draws the settings menu for users to interact with
        /// </summary>
        /// <param name="settingListing">The setting lister to use</param>
        public void DrawSettingsMenu(Listing_Standard settingListing)
        {
            bool flickLights = ModSettings.FlickLights;

            // initialize the input buffers if they haven't been already
            _standbyBuf = _standbyBuf ?? ModSettings.StandbyResourceDrawPercent.ToString();
            _activeBuf = _activeBuf ?? ModSettings.ActiveResourceDrawPercent.ToString();
            _delayBuf = _delayBuf ?? ModSettings.DelaySeconds.ToString();
            _messageFilterBuf = _messageFilterBuf ?? ModSettings.MessageFilters.Join(null, " ");

            settingListing.CheckboxLabeled(
                "LightsOut_Settings_EverythingButLightsOutLabel".Translate(),
                ref flickLights,
                "LightsOut_Settings_EverythingButLightsOutTooltip".Translate()
                );

            settingListing.CheckboxLabeled(
                "LightsOut_Settings_NightLightsLabel".Translate(),
                ref ModSettings.NightLights,
                "LightsOut_Settings_NightLightsTooltip".Translate()
                );

            settingListing.CheckboxLabeled(
                "LightsOut_Settings_AnimalPartyLabel".Translate(),
                ref ModSettings.AnimalParty,
                "LightsOut_Settings_AnimalPartyTooltip".Translate()
                );

            settingListing.TextFieldNumericLabeled(
                "LightsOut_Settings_DelaySecondsLabel".Translate(),
                ref ModSettings.DelaySeconds, ref _delayBuf,
                0, float.MaxValue
                );

            settingListing.TextFieldNumericLabeled(
                "LightsOut_Settings_LatentPowerDrawRateLabel".Translate(),
                ref ModSettings.StandbyResourceDrawPercent, ref _standbyBuf,
                0, 100
                );

            settingListing.TextFieldNumericLabeled(
                "LightsOut_Settings_ActivePowerDrawRateLabel".Translate(),
                ref ModSettings.ActiveResourceDrawPercent, ref _activeBuf,
                0, int.MaxValue
                );

            _messageFilterBuf = settingListing.TextEntryLabeled(
                "LightsOut_Settings_DebugMessageFilterLabel".Translate(),
                _messageFilterBuf,
                1
                );

            ModSettings.MessageFilters = _messageFilterBuf.ToLower().Split(' ');
            UpdateLightGlowersOnSettingChange(flickLights);
            ModSettings.FlickLights = flickLights;
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
            _messageFilterBuf = _messageFilterBuf ?? ModSettings.MessageFilters.Join(null, " ");

            // clamp the active power draw to at least 100
            if (ModSettings.ActiveResourceDrawPercent < 100)
                ModSettings.ActiveResourceDrawPercent = 100;

            Scribe_Values.Look(ref ModSettings.FlickLights, "EverythingButLightsOut", ModSettings.FlickLights, true);
            Scribe_Values.Look(ref ModSettings.NightLights, "NightLights", ModSettings.NightLights, true);
            Scribe_Values.Look(ref ModSettings.AnimalParty, "AnimalParty", ModSettings.AnimalParty, true);
            Scribe_Values.Look(ref ModSettings.DelaySeconds, "DelaySeconds", ModSettings.DelaySeconds, true);
            Scribe_Values.Look(ref ModSettings.StandbyResourceDrawPercent, "LatentPowerDrawRate", ModSettings.StandbyResourceDrawPercent, true);
            Scribe_Values.Look(ref ModSettings.ActiveResourceDrawPercent, "ActivePowerDrawRate", ModSettings.ActiveResourceDrawPercent, true);
            Scribe_Values.Look(ref _messageFilterBuf, "DebugFilters", _messageFilterBuf, true);

            ModSettings.MessageFilters = _messageFilterBuf.ToLower().Split(' ');

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
            if (ModSettings.FlickLights == newVal) return;
            DebugLogger.LogInfo($"Changing FlickLights from {ModSettings.FlickLights} to {newVal}", DebugMessageKeys.Settings);

            ModSettings.FlickLights = newVal;

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

            if (!ModSettings.FlickLights)
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
                                TryParse(value, ref ModSettings.FlickLights);
                            else if (settingNode.Name == "NightLights")
                                TryParse(value, ref ModSettings.NightLights);
                            else if (settingNode.Name == "AnimalParty")
                                TryParse(value, ref ModSettings.AnimalParty);
                            else if (settingNode.Name == "DelaySeconds")
                                TryParse(value, ref ModSettings.DelaySeconds);
                            else if (settingNode.Name == "LatentPowerDrawRate")
                                TryParse(value, ref ModSettings.StandbyResourceDrawPercent);
                            else if (settingNode.Name == "ActivePowerDrawRate")
                                TryParse(value, ref ModSettings.ActiveResourceDrawPercent);
                            else if (settingNode.Name == "DebugFilters")
                                ModSettings.MessageFilters = value.ToLower().Split(' ');
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