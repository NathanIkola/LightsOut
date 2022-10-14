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
        /// A list of message filters to add.
        /// Allows messages based on their debug message key.
        /// </summary>
        public static string[] MessageFilters { get; set; } = { };

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

            string messageFilters = Settings.GetHandle<string>(
                "DebugFilters",
                "LightsOut_Settings_DebugMessageFilterLabel".Translate(),
                "LightsOut_Settings_DebugMessageFilterTooltip".Translate(),
                DebugMessageKeys.Error + DebugMessageKeys.Mods
                );

            StandbyResourceDrawRate = standbyPower / 100f;
            ActiveResourceDrawRate = activePower / 100f;

            NightLights = nightLights;

            MessageFilters = messageFilters.ToLower().Split(' ');

            UpdateLightGlowersOnSettingChange(lightsOut);
            FlickLights = lightsOut;
        }

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