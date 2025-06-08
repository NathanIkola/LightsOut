using LightsOut.Common;

namespace LightsOut.Boilerplate
{
    /// <summary>
    /// A class that holds the static fields for the settings
    /// </summary>
    /// <remarks>
    /// I didn't want to do this, but other mods started to patch
    /// fields in the ModSettings class, so I wasn't able to rename it
    /// without causing compatibility issues. I left these here and moved
    /// the rest of the functionality into LightsOutSettings
    /// </remarks>
    public static class ModSettings
    {
        /// <summary>
        /// Control whether or not Pawns flick lights on and off
        /// </summary>
        public static bool FlickLights = true;

        /// <summary>
        /// The percentage form of the draw rate
        /// </summary>
        public static int StandbyResourceDrawPercent = 0;

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
        public static float ActiveResourceDrawRate => ActiveResourceDrawPercent / 100f;

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
    }
}