using Verse;

namespace LightsOut.Common
{
    /// <summary>
    /// Handles all of the debug logging needs for my mod.
    /// Respects the mod settings option for printing extra debug messages.
    /// Adds a header to the beginning of all messages passed in so they
    /// are easy to identify in the log.
    /// </summary>
    public static class DebugLogger
    {
        /// <summary>
        /// Alias for the message filters mod setting property
        /// </summary>
        private static string[] MessageFilters => Boilerplate.LightsOutSettings.MessageFilters;

        /// <summary>
        /// Checks whether the specific debug message should be logged
        /// </summary>
        /// <param name="keyString">A list of debug message keys to log</param>
        /// <returns><see langword="true"/> if the messages should be logged, 
        /// <see langword="false"/> otherwise</returns>
        private static bool ShouldLog(string keyString)
        {
            if (string.IsNullOrWhiteSpace(keyString))
                return false;

            string[] keys = keyString.ToLower().Split(' ');

            bool allowAll = false;
            foreach (string allowedKey in MessageFilters)
            {
                // split introduces the empty string :(
                if (allowedKey.Equals(string.Empty))
                    continue;

                // there could still be disallowed keys specified later
                if (allowedKey.Equals("all")) 
                    allowAll = true;

                foreach (string key in keys)
                {
                    // explicitly allowed key
                    if (key.Equals(allowedKey)) return true;

                    // explicitly disallowed key
                    if (allowedKey.Equals('!' + key)) return false;
                }
            }

            return allowAll;
        }

        /// <summary>
        /// Adds the debug header to a message
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <returns>The message with the debug header prepended</returns>
        private static string AddDebugHeader(string message)
        {
            return "<color=orange>[LightsOut - Debug]</color> " + message;
        }

        /// <summary>
        /// Logs an informational message to the console.
        /// Should be used to log things that are good to
        /// know, but are not problematic
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="keys">A list of keys that apply to this message</param>
        public static void LogInfo(string message, string keys)
        {
            keys = "info " + keys;
            if (ShouldLog(keys)) Log.Message(AddDebugHeader(message));
        }

        /// <summary>
        /// Logs a warning message to the console.
        /// Should be used to log things that could be
        /// issues, but aren't causing any fatal problems
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="keys">A list of keys that apply to this message</param>
        public static void LogWarning(string message, string keys)
        {
            keys = "warning " + keys;
            if (ShouldLog(keys)) Log.Warning(AddDebugHeader(message));
        }

        /// <summary>
        /// Logs an error message to the console.
        /// Should be used to log when something has
        /// gone terribly, terribly wrong.
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="keys">A list of keys that apply to this message</param>
        public static void LogError(string message, string keys)
        {
            keys = "error " + keys;
            if (ShouldLog(keys)) Log.Error(AddDebugHeader(message));
        }

        /// <summary>
        /// Logs an error a single time, no matter how
        /// many times it was encountered.
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="errorKey">The error's key (unique for each error)</param>
        /// <param name="debugKeys">A list of keys that apply to this message</param>
        public static void LogErrorOnce(string message, int errorKey, string debugKeys)
        {
            debugKeys = "error " + debugKeys;
            if (ShouldLog(debugKeys)) Log.ErrorOnce(AddDebugHeader(message), errorKey);
        }

        /// <summary>
        /// Logs an error a single time, no matter how
        /// many times it was encountered.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="keys">A list of keys that apply to this message</param>
        /// <remarks>
        /// Uses the hash of <paramref name="message"/>
        /// to only log the message once. If you make <paramref name="message"/> have dynamic
        /// content, it will appear once for every unique version of <paramref name="message"/>
        /// that is passed in.
        /// </remarks>
        public static void LogErrorOnce(string message, string keys)
        {
            LogErrorOnce(message, message.GetHashCode(), keys);
        }

        /// <summary>
        /// Used to catch places where an expression is expected to be true,
        /// but is actually false. Useful for finding places where code is
        /// not behaving correctly. Does not log unless an error is found.
        /// </summary>
        /// <param name="expression">The expression that should be <see langword="true"/></param>
        /// <param name="failMessage">The error message to print out if <paramref name="expression"/> is <see langword="false"/></param>
        /// <param name="onlyOnce">Log this assertion failure only one time</param>
        /// <remarks>
        /// If <paramref name="onlyOnce"/> is set, this uses the hash of <paramref name="failMessage"/>
        /// to only log the message once. If you make <paramref name="failMessage"/> have dynamic
        /// content, it will appear once for every unique version of <paramref name="failMessage"/>
        /// that is passed in.
        /// </remarks>
        public static void Assert(bool expression, string failMessage, bool onlyOnce = false)
        {
            if (expression) return;

            if (onlyOnce)
                LogErrorOnce(failMessage, failMessage.GetHashCode(), DebugMessageKeys.Assert);
            else
                LogError(failMessage, DebugMessageKeys.Assert);
        }

        /// <summary>
        /// Same as <see cref="DebugLogger.Assert(bool, string, bool)"/>, except it expects the
        /// expression passed in to be false instead of true.
        /// </summary>
        /// <param name="expression">The expression that should be <see langword="false"/></param>
        /// <param name="failMessage">The error message to print out if <paramref name="expression"/> is <see langword="true"/></param>
        /// <param name="onlyOnce">Log this assertion failure only one time</param>
        public static void AssertFalse(bool expression, string failMessage, bool onlyOnce = false)
        {
            Assert(!expression, failMessage, onlyOnce);
        }
    }
}