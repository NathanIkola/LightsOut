using System.Collections.Generic;
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
        /// Control whether or not Pawns flick lights on and off
        /// </summary>
        public static bool FlickLights = true;

        /// <summary>
        /// Latent power draw of things when they're flicked off
        /// </summary>
        public static float StandbyResourceDrawRate = 0f;

        /// <summary>
        /// The resource draw rate of things when they're in-use
        /// </summary>
        public static float ActiveResourceDrawRate = 1f;

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
        public static string[] MessageFilters = { };

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
            int standbyRate = (int)(StandbyResourceDrawRate * 100);
            int activeRate = (int)(ActiveResourceDrawRate * 100);

            // initialize the input buffers if they haven't been already
            _standbyBuf = _standbyBuf ?? standbyRate.ToString();
            _activeBuf = _activeBuf ?? activeRate.ToString();
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
                ref standbyRate, ref _standbyBuf,
                0, 100
                );

            settingListing.TextFieldNumericLabeled(
                "LightsOut_Settings_ActivePowerDrawRateLabel".Translate(),
                ref activeRate, ref _activeBuf,
                100, int.MaxValue
                );

            _messageFilterBuf = settingListing.TextEntryLabeled(
                "LightsOut_Settings_DebugMessageFilterLabel".Translate(),
                _messageFilterBuf,
                1
                );

            StandbyResourceDrawRate = standbyRate / 100f;
            ActiveResourceDrawRate = activeRate / 100f;
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
            uint standbyRate = (uint)(StandbyResourceDrawRate * 100);
            uint activeRate = (uint)(ActiveResourceDrawRate * 100);

            Scribe_Values.Look(ref FlickLights, "EverythingButLightsOut");
            Scribe_Values.Look(ref NightLights, "NightLights");
            Scribe_Values.Look(ref AnimalParty, "AnimalParty");
            Scribe_Values.Look(ref DelaySeconds, "DelaySeconds");
            Scribe_Values.Look(ref standbyRate, "LatentPowerDrawRate");
            Scribe_Values.Look(ref activeRate, "ActivePowerDrawRate");
            Scribe_Values.Look(ref _messageFilterBuf, "DebugFilters");

            StandbyResourceDrawRate = standbyRate / 100f;
            ActiveResourceDrawRate = activeRate / 100f;
            MessageFilters = _messageFilterBuf.ToLower().Split(' ');
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
            DebugLogger.LogInfo($"Changing FlickLights from {FlickLights} to {newVal}", DebugMessageKeys.Settings);
            if (FlickLights == newVal) return;

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
    }
}