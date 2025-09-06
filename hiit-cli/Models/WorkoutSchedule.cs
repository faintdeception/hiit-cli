using System.Text.Json.Serialization;

namespace hiit_cli.Models
{
    /// <summary>
    /// Represents a schedule that defines when to run specific routines
    /// </summary>
    public class WorkoutSchedule
    {
        /// <summary>
        /// Name of the schedule (e.g., "Weekly HIIT Plan")
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the schedule
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Schedule entries that define what routine to run when
        /// </summary>
        [JsonPropertyName("entries")]
        public List<ScheduleEntry> Entries { get; set; } = new();

        /// <summary>
        /// Gets the appropriate routine for the current day and time
        /// </summary>
        /// <param name="dayOfWeek">Day of the week</param>
        /// <param name="timeOfDay">Time of day</param>
        /// <returns>Matching schedule entry or null if none found</returns>
        public ScheduleEntry? GetRoutineForTime(DayOfWeek dayOfWeek, TimeOnly timeOfDay)
        {
            return Entries
                .Where(entry => entry.IsActiveOn(dayOfWeek, timeOfDay))
                .OrderBy(entry => entry.StartTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets all routines scheduled for a specific day
        /// </summary>
        /// <param name="dayOfWeek">Day of the week</param>
        /// <returns>List of schedule entries for the day</returns>
        public List<ScheduleEntry> GetRoutinesForDay(DayOfWeek dayOfWeek)
        {
            return Entries
                .Where(entry => entry.DaysOfWeek.Contains(dayOfWeek))
                .OrderBy(entry => entry.StartTime)
                .ToList();
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && 
                   Entries.Count > 0 && 
                   Entries.All(e => e.IsValid());
        }
    }

    /// <summary>
    /// Represents a single entry in a workout schedule
    /// </summary>
    public class ScheduleEntry
    {
        /// <summary>
        /// Name or path of the routine file to execute
        /// </summary>
        [JsonPropertyName("routineName")]
        public string RoutineName { get; set; } = string.Empty;

        /// <summary>
        /// Days of the week when this routine should run
        /// </summary>
        [JsonPropertyName("daysOfWeek")]
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();

        /// <summary>
        /// Start time for this routine (24-hour format)
        /// </summary>
        [JsonPropertyName("startTime")]
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// End time for this routine (24-hour format)
        /// </summary>
        [JsonPropertyName("endTime")]
        public TimeOnly EndTime { get; set; }

        /// <summary>
        /// Optional note about this schedule entry
        /// </summary>
        [JsonPropertyName("note")]
        public string? Note { get; set; }

        /// <summary>
        /// Checks if this schedule entry is active for the given day and time
        /// </summary>
        /// <param name="dayOfWeek">Day of the week</param>
        /// <param name="timeOfDay">Time of day</param>
        /// <returns>True if active, false otherwise</returns>
        public bool IsActiveOn(DayOfWeek dayOfWeek, TimeOnly timeOfDay)
        {
            return DaysOfWeek.Contains(dayOfWeek) && 
                   timeOfDay >= StartTime && 
                   timeOfDay <= EndTime;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(RoutineName) && 
                   DaysOfWeek.Count > 0 && 
                   StartTime < EndTime;
        }

        public override string ToString()
        {
            var days = string.Join(", ", DaysOfWeek.Select(d => d.ToString().Substring(0, 3)));
            return $"{RoutineName} - {days} {StartTime:HH:mm}-{EndTime:HH:mm}";
        }
    }
}
