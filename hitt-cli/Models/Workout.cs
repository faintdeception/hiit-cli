using System.Text.Json.Serialization;

namespace hitt_cli.Models
{
    /// <summary>
    /// Represents a single workout exercise with timing and repetition information
    /// </summary>
    public class Workout
    {
        /// <summary>
        /// The name of the workout exercise (e.g., "pushups", "squats", "burpees")
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Number of sets to perform for this workout
        /// </summary>
        [JsonPropertyName("sets")]
        public int Sets { get; set; }

        /// <summary>
        /// Duration of each set in seconds
        /// </summary>
        [JsonPropertyName("length")]
        public int Length { get; set; }

        /// <summary>
        /// Rest time between sets in seconds
        /// </summary>
        [JsonPropertyName("rest")]
        public int Rest { get; set; }

        /// <summary>
        /// Optional description or instructions for the workout
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Calculates the total time required for this workout including rest periods
        /// </summary>
        /// <returns>Total time in seconds</returns>
        public int GetTotalTimeInSeconds()
        {
            // Total time = (sets * length) + ((sets - 1) * rest)
            // We subtract 1 from sets for rest because there's no rest after the last set
            return (Sets * Length) + ((Sets - 1) * Rest);
        }

        /// <summary>
        /// Gets a formatted string representation of the total workout time
        /// </summary>
        /// <returns>Formatted time string (e.g., "2m 30s")</returns>
        public string GetFormattedTotalTime()
        {
            var totalSeconds = GetTotalTimeInSeconds();
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;

            if (minutes > 0)
            {
                return seconds > 0 ? $"{minutes}m {seconds}s" : $"{minutes}m";
            }
            return $"{seconds}s";
        }

        /// <summary>
        /// Validates that the workout has valid parameters
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && 
                   Sets > 0 && 
                   Length > 0 && 
                   Rest >= 0;
        }

        public override string ToString()
        {
            return $"{Name}: {Sets} sets x {Length}s (rest: {Rest}s) - Total: {GetFormattedTotalTime()}";
        }
    }
}
