using System.Text;

namespace hiit_cli.Services
{
    /// <summary>
    /// Service for handling display symbols and emoji compatibility
    /// </summary>
    public static class DisplayService
    {
        private static bool? _supportsEmojis;
        private static bool? _audioEnabled;
        private static bool? _audioDebugEnabled;
        
        /// <summary>
        /// Checks if the current terminal/environment supports emoji display
        /// </summary>
        public static bool SupportsEmojis
        {
            get
            {
                if (_supportsEmojis.HasValue)
                    return _supportsEmojis.Value;

                // Check environment variable first
                var envOverride = Environment.GetEnvironmentVariable("HIIT_CLI_EMOJIS");
                if (!string.IsNullOrEmpty(envOverride))
                {
                    _supportsEmojis = envOverride.ToLowerInvariant() == "true";
                    return _supportsEmojis.Value;
                }

                // Enhanced detection
                var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
                
                if (!isWindows)
                {
                    // Most Unix/Linux terminals support emojis
                    _supportsEmojis = true;
                }
                else
                {
                    // Windows: check for UTF-8 or Windows Terminal
                    var isWindowsTerminal = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));
                    var encoding = Console.OutputEncoding;
                    var isUTF8 = encoding.CodePage == 65001; // UTF-8 code page
                    
                    _supportsEmojis = isWindowsTerminal || isUTF8;
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
        /// Checks if audio cues are enabled
        /// </summary>
        public static bool AudioEnabled
        {
            get
            {
                if (_audioEnabled.HasValue)
                    return _audioEnabled.Value;

                // Check environment variable for audio preference
                var envAudio = Environment.GetEnvironmentVariable("HIIT_CLI_AUDIO");
                if (!string.IsNullOrEmpty(envAudio))
                {
                    _audioEnabled = envAudio.ToLowerInvariant() == "true";
                    return _audioEnabled.Value;
                }

                // Default to enabled
                _audioEnabled = true;
                return _audioEnabled.Value;
            }
        }

        /// <summary>
        /// Forces audio on or off
        /// </summary>
        public static void SetAudioEnabled(bool enabled)
        {
            _audioEnabled = enabled;
        }

        /// <summary>
        /// Checks if audio debug mode is enabled
        /// </summary>
        public static bool AudioDebugEnabled
        {
            get
            {
                if (_audioDebugEnabled.HasValue)
                    return _audioDebugEnabled.Value;

                // Check environment variable for audio debug preference
                var envAudioDebug = Environment.GetEnvironmentVariable("HIIT_CLI_AUDIO_DEBUG");
                if (!string.IsNullOrEmpty(envAudioDebug))
                {
                    _audioDebugEnabled = envAudioDebug.ToLowerInvariant() == "true";
                    return _audioDebugEnabled.Value;
                }

                // Default to disabled
                _audioDebugEnabled = false;
                return _audioDebugEnabled.Value;
            }
        }

        /// <summary>
        /// Forces audio debug mode on or off
        /// </summary>
        public static void SetAudioDebugEnabled(bool enabled)
        {
            _audioDebugEnabled = enabled;
        }

        /// <summary>
        /// Helper method for checking if audio is enabled
        /// </summary>
        public static bool IsAudioEnabled() => AudioEnabled;

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
        public static string Calendar => SupportsEmojis ? "\U0001F4C5" : "@";

        /// <summary>
        /// Gets an appropriate symbol for clock/time
        /// </summary>
        public static string Clock => SupportsEmojis ? "\u23F0" : "T";

        /// <summary>
        /// Gets an appropriate symbol for document/list
        /// </summary>
        public static string Document => SupportsEmojis ? "\U0001F4DD" : "=";

        /// <summary>
        /// Gets an appropriate symbol for eyes/preview
        /// </summary>
        public static string Eyes => SupportsEmojis ? "\U0001F440" : "?";

        /// <summary>
        /// Gets an appropriate symbol for question/help
        /// </summary>
        public static string Question => SupportsEmojis ? "\u2753" : "?";

        /// <summary>
        /// Gets an appropriate symbol for door/exit
        /// </summary>
        public static string Door => SupportsEmojis ? "\U0001F6AA" : "X";
    }
}
