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
        public static string Fire => SupportsEmojis ? "🔥" : "*";

        /// <summary>
        /// Gets an appropriate symbol for lightning/energy
        /// </summary>
        public static string Lightning => SupportsEmojis ? "⚡" : "!";

        /// <summary>
        /// Gets an appropriate symbol for muscle/strength
        /// </summary>
        public static string Muscle => SupportsEmojis ? "💪" : "+";

        /// <summary>
        /// Gets an appropriate symbol for target/goal
        /// </summary>
        public static string Target => SupportsEmojis ? "🎯" : ">";

        /// <summary>
        /// Gets an appropriate symbol for rocket/speed
        /// </summary>
        public static string Rocket => SupportsEmojis ? "🚀" : "^";

        /// <summary>
        /// Gets an appropriate symbol for star/excellence
        /// </summary>
        public static string Star => SupportsEmojis ? "⭐" : "*";

        /// <summary>
        /// Gets an appropriate symbol for crown/achievement
        /// </summary>
        public static string Crown => SupportsEmojis ? "👑" : "#";

        /// <summary>
        /// Gets an appropriate symbol for sword/warrior
        /// </summary>
        public static string Sword => SupportsEmojis ? "⚔️" : "X";

        /// <summary>
        /// Gets an appropriate symbol for trophy/victory
        /// </summary>
        public static string Trophy => SupportsEmojis ? "🏆" : "!";

        /// <summary>
        /// Gets an appropriate symbol for runner/exercise
        /// </summary>
        public static string Runner => SupportsEmojis ? "🏃‍♂️" : ">";

        /// <summary>
        /// Gets an appropriate symbol for explosion/power
        /// </summary>
        public static string Explosion => SupportsEmojis ? "💥" : "*";

        /// <summary>
        /// Gets an appropriate symbol for water/hydration
        /// </summary>
        public static string Water => SupportsEmojis ? "💧" : "~";

        /// <summary>
        /// Gets an appropriate symbol for meditation/stretching
        /// </summary>
        public static string Meditation => SupportsEmojis ? "🧘‍♀️" : "-";

        /// <summary>
        /// Gets an appropriate symbol for 100/perfection
        /// </summary>
        public static string Hundred => SupportsEmojis ? "💯" : "!";
    }
}
