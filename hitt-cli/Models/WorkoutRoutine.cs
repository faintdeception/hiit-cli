using System.Text.Json.Serialization;

namespace hitt_cli.Models
{
    /// <summary>
    /// Represents a collection of workouts that form a complete routine
    /// </summary>
    public class WorkoutRoutine
    {
        /// <summary>
        /// Name of the routine (e.g., "Morning HIIT", "Full Body Blast")
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the routine
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// List of workouts in this routine
        /// </summary>
        [JsonPropertyName("workouts")]
        public List<Workout> Workouts { get; set; } = new();

        /// <summary>
        /// Difficulty level (1-5, where 1 is beginner and 5 is expert)
        /// </summary>
        [JsonPropertyName("difficulty")]
        public int Difficulty { get; set; } = 1;

        /// <summary>
        /// Number of times to repeat the entire routine (defaults to 1)
        /// </summary>
        [JsonPropertyName("reps")]
        public int Reps { get; set; } = 1;

        /// <summary>
        /// Tags for categorizing routines (e.g., "cardio", "strength", "full-body")
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Calculates the total time for the entire routine including all reps
        /// </summary>
        /// <returns>Total time in seconds</returns>
        public int GetTotalTimeInSeconds()
        {
            var singleRoutineTime = Workouts.Sum(w => w.GetTotalTimeInSeconds());
            return singleRoutineTime * Reps;
        }

        /// <summary>
        /// Gets a formatted string representation of the total routine time
        /// </summary>
        /// <returns>Formatted time string</returns>
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
        /// Gets the total number of exercises in the routine across all reps
        /// </summary>
        /// <returns>Number of workouts</returns>
        public int GetTotalExercises()
        {
            return Workouts.Count * Reps;
        }

        /// <summary>
        /// Gets the number of exercises in a single round of the routine
        /// </summary>
        /// <returns>Number of workouts per rep</returns>
        public int GetExercisesPerRep()
        {
            return Workouts.Count;
        }

        /// <summary>
        /// Validates that the routine has valid parameters
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && 
                   Workouts.Count > 0 && 
                   Workouts.All(w => w.IsValid()) &&
                   Difficulty >= 1 && Difficulty <= 5 &&
                   Reps >= 1;
        }

        public override string ToString()
        {
            var repsText = Reps > 1 ? $" x{Reps} reps" : "";
            return $"{Name} - {GetExercisesPerRep()} exercises{repsText}, {GetFormattedTotalTime()}, Difficulty: {Difficulty}/5";
        }
    }
}
