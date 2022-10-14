using System.Collections.Generic;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using LightsOut.Common;
using LightsOut.Patches.ModCompatibility;
using Verse;

namespace LightsOut.Boilerplate
{
    /// <summary>
    /// The class that initializes the mod and holds all
    /// mod-related settings
    /// </summary>
    public class ModSettings : ModBase
    {
        /// <summary>
        /// Default constructor that sets the Harmony instance. 
        /// This should only be called by the game when it loads the mod.
        /// </summary>
        public ModSettings()
        {
            Instance = this;
        }

        /// <summary>
        /// The Harmony instance associated with this mod
        /// </summary>
        public static Harmony Harmony => Instance.HarmonyInst;

        /// <summary>
        /// The instance of this mod that was instantiated by the game.
        /// This is only used to get the Harmony instance.
        /// </summary>
        private static ModSettings Instance { get; set; }

        /// <summary>
        /// Control whether or not Pawns flick lights on and off
        /// </summary>
        public static bool FlickLights { get; set; } = true;

        /// <summary>
        /// Latent power draw of things when they're flicked off
        /// </summary>
        public static float StandbyResourceDrawRate { get; set; } = 0f;

        /// <summary>
        /// The resource draw rate of things when they're in-use
        /// </summary>
        public static float ActiveResourceDrawRate { get; set; } = 1f;

        /// <summary>
        /// Control whether or not Pawns shut off the lights when
        /// they go to bed
        /// </summary>
        public static bool NightLights { get; set; } = false;

        /// <summary>
        /// Control whether or not extra debug messages get 
        /// printed to the console during gameplay
        /// </summary>
        public static bool DebugMessages { get; set; } = false;

        /// <summary>
        /// Control whether or not the extra spammy debug
        /// messages get printed to the console during gameplay
        /// </summary>
        public static bool SpamMessages { get; set; } = false;

        /// <summary>
        /// The identifier this mod uses to identify itself in-game
        /// </summary>
        public override string ModIdentifier
        {
            get { return "LightsOut"; }
        }

        /// <summary>
        /// The function called when the game finishes loading defs
        /// </summary>
        public override void DefsLoaded()
        {
            Log.Message("LightsOut_InitializingMod".Translate() + " [" + typeof(ModSettings).Assembly.GetName().Version + "]");
            SettingsChanged();
            ModCompatibilityManager.LoadCompatibilityPatches();
        }

        /// <summary>
        /// The function called when the game settings change for any reason
        /// </summary>
        public override void SettingsChanged()
        {
            bool lightsOut = Settings.GetHandle<bool>(
                "EverythingButLightsOut",
                "LightsOut_Settings_EverythingButLightsOutLabel".Translate(),
                "LightsOut_Settings_EverythingButLightsOutTooltip".Translate(),
                true);

            bool nightLights = Settings.GetHandle<bool>(
                "NightLights",
                "LightsOut_Settings_NightLightsLabel".Translate(),
                "LightsOut_Settings_NightLightsTooltip".Translate(),
                false);

            uint standbyPower = Settings.GetHandle<uint>(
                "LatentPowerDrawRate",
                "LightsOut_Settings_LatentPowerDrawRateLabel".Translate(),
                "LightsOut_Settings_LatentPowerDrawRateTooltip".Translate(),
                0,
                Validators.IntRangeValidator(0, 100));

            uint activePower = Settings.GetHandle<uint>(
                "ActivePowerDrawRate",
                "LightsOut_Settings_ActivePowerDrawRateLabel".Translate(),
                "LightsOut_Settings_ActivePowerDrawRateTooltip".Translate(),
                100,
                Validators.IntRangeValidator(100, int.MaxValue));

            bool debugMessages = Settings.GetHandle<bool>(
                "DebugMessages",
                "LightsOut_Settings_DebugMessagesLabel".Translate(),
                "LightsOut_Settings_DebugMessagesTooltip".Translate(),
                false
                );

            bool spamMessages = Settings.GetHandle<bool>(
                "SpamMessages",
                "LightsOut_Settings_SpamMessagesLabel".Translate(),
                "LightsOut_Settings_SpamMessagesTooltip".Translate(),
                false
                );

            StandbyResourceDrawRate = standbyPower / 100f;
            ActiveResourceDrawRate = activePower / 100f;

            NightLights = nightLights;
            DebugMessages = debugMessages;
            SpamMessages = spamMessages;

            UpdateLightGlowersOnSettingChange(lightsOut);
            FlickLights = lightsOut;
        }

        /// <summary>
        /// Flicks (or unflicks) lights depending on the new value from the settings
        /// </summary>
        /// <param name="newVal">The new value the setting is being set to</param>
        private void UpdateLightGlowersOnSettingChange(bool newVal)
        {
            DebugLogger.LogInfo($"Changing FlickLights from {FlickLights} to {newVal}", true);
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