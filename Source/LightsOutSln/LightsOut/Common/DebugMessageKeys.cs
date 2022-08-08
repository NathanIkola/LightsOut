namespace LightsOut.Common
{
    /// <summary>
    /// A class that holds all of the debug message keys
    /// so that I can verify they're all spelled right
    /// </summary>
    public static class DebugMessageKeys
    {
        /// <summary>
        /// Logged with all informational messages
        /// </summary>
        public static string Info => "info ";

        /// <summary>
        /// Logged with all warning messages
        /// </summary>
        public static string Warning => "warning ";

        /// <summary>
        /// Logged with all error messages
        /// </summary>
        public static string Error => "error ";

        /// <summary>
        /// Logged with all failed asserts (along with error)
        /// </summary>
        public static string Assert => "assert ";

        /// <summary>
        /// Logged with all mod-related messages
        /// </summary>
        public static string Mods => "mods ";

        /// <summary>
        /// Logged with all light-related messages
        /// </summary>
        public static string Lights => "lights ";

        /// <summary>
        /// Logged with all comp-related messages
        /// </summary>
        public static string Comps => "comps ";

        /// <summary>
        /// Logged with all settings-related messages
        /// </summary>
        public static string Settings => "settings ";

        /// <summary>
        /// Logged with all glower-related messages
        /// </summary>
        public static string Glowers => "glowers ";

        /// <summary>
        /// Logged with all patch-related messages
        /// </summary>
        public static string Patches => "patches ";
    }
}
