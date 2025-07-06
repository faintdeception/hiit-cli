using System.Text;

namespace hitt_cli.Services
{
    /// <summary>
    /// Service for handling display symbols and emoji compatibility
    /// </summary>
    public static class DisplayService
    {
        private static bool? _supportsEmojis;
        
        /// <summary>
        /// Checks if the current terminal/environment supports emoji display
        /// </summary>
        public static bool SupportsEmojis
        {
            get
            {
                if (_supportsEmojis.HasValue)
                    return _supportsEmojis.Value;

                // Check if we're in a Windows environment that might have font issues
                var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
                var encoding = Console.OutputEncoding;
                
                // Basic heuristic: if we're on Windows and not using UTF-8, likely emoji issues
                _supportsEmojis = !isWindows || encoding.Equals(Encoding.UTF8);
                
                // Allow override via environment variable
                var envOverride = Environment.GetEnvironmentVariable("HITT_CLI_EMOJIS");
                if (!string.IsNullOrEmpty(envOverride))
                {
                    _supportsEmojis = envOverride.ToLowerInvariant() == "true";
                }
                
                return _supportsEmojis.Value;
            }
        }

        /// <summary>
        /// Forces emoji support on or off
        /// </summary>
        public static void SetEmojiSupport(bool supported)
        {
            _supportsEmojis = supported;
        }

        /// <summary>
        /// Gets an appropriate symbol for fire/intensity
        /// </summary>
        public static string Fire => SupportsEmojis ? "\U0001F525" : "*";

        /// <summary>
        /// Gets an appropriate symbol for lightning/energy
        /// </summary>
        public static string Lightning => SupportsEmojis ? "\u26A1" : "!";

        /// <summary>
        /// Gets an appropriate symbol for muscle/strength
        /// </summary>
        public static string Muscle => SupportsEmojis ? "\U0001F4AA" : "+";

        /// <summary>
        /// Gets an appropriate symbol for target/goal
        /// </summary>
        public static string Target => SupportsEmojis ? "\U0001F3AF" : ">";

        /// <summary>
        /// Gets an appropriate symbol for rocket/speed
        /// </summary>
        public static string Rocket => SupportsEmojis ? "\U0001F680" : "^";

        /// <summary>
        /// Gets an appropriate symbol for star/excellence
        /// </summary>
        public static string Star => SupportsEmojis ? "\u2B50" : "*";

        /// <summary>
        /// Gets an appropriate symbol for crown/achievement
        /// </summary>
        public static string Crown => SupportsEmojis ? "\U0001F451" : "#";

        /// <summary>
        /// Gets an appropriate symbol for sword/warrior
        /// </summary>
        public static string Sword => SupportsEmojis ? "\u2694\uFE0F" : "X";

        /// <summary>
        /// Gets an appropriate symbol for trophy/victory
        /// </summary>
        public static string Trophy => SupportsEmojis ? "\U0001F3C6" : "!";

        /// <summary>
        /// Gets an appropriate symbol for runner/exercise
        /// </summary>
        public static string Runner => SupportsEmojis ? "\U0001F3C3\u200D\u2642\uFE0F" : ">";

        /// <summary>
        /// Gets an appropriate symbol for explosion/power
        /// </summary>
        public static string Explosion => SupportsEmojis ? "\U0001F4A5" : "*";

        /// <summary>
        /// Gets an appropriate symbol for water/hydration
        /// </summary>
        public static string Water => SupportsEmojis ? "\U0001F4A7" : "~";

        /// <summary>
        /// Gets an appropriate symbol for meditation/stretching
        /// </summary>
        public static string Meditation => SupportsEmojis ? "\U0001F9D8\u200D\u2640\uFE0F" : "-";

        /// <summary>
        /// Gets an appropriate symbol for 100/perfection
        /// </summary>
        public static string Hundred => SupportsEmojis ? "\U0001F4AF" : "!";

        /// <summary>
        /// Gets an appropriate symbol for calendar/schedule
        /// </summary>
        public static string Calendar => SupportsEmojis ? "📅" : "@";

        /// <summary>
        /// Gets an appropriate symbol for clock/time
        /// </summary>
        public static string Clock => SupportsEmojis ? "⏰" : "T";

        /// <summary>
        /// Gets an appropriate symbol for document/list
        /// </summary>
        public static string Document => SupportsEmojis ? "📝" : "=";

        /// <summary>
        /// Gets an appropriate symbol for eyes/preview
        /// </summary>
        public static string Eyes => SupportsEmojis ? "👀" : "?";

        /// <summary>
        /// Gets an appropriate symbol for question/help
        /// </summary>
        public static string Question => SupportsEmojis ? "❓" : "?";

        /// <summary>
        /// Gets an appropriate symbol for door/exit
        /// </summary>
        public static string Door => SupportsEmojis ? "🚪" : "X";
    }
}
