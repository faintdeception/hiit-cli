using System.Text.Json;
using hitt_cli.Models;

namespace hitt_cli.Services
{
    /// <summary>
    /// Service for loading and managing workout data from JSON files
    /// </summary>
    public class WorkoutDataService
    {
        private readonly string _dataDirectory;
        private readonly string _routinesDirectory;
        private readonly string _schedulesDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public WorkoutDataService(string? dataDirectory = null)
        {
            _dataDirectory = dataDirectory ?? Path.Combine(AppContext.BaseDirectory, "Data");
            _routinesDirectory = Path.Combine(_dataDirectory, "Routines");
            _schedulesDirectory = Path.Combine(_dataDirectory, "Schedules");
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            EnsureDirectoriesExist();
        }

        /// <summary>
        /// Ensures the required directories exist
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_routinesDirectory);
            Directory.CreateDirectory(_schedulesDirectory);
        }

        /// <summary>
        /// Loads a workout routine from a JSON file
        /// </summary>
        /// <param name="routineName">Name of the routine file (with or without .json extension)</param>
        /// <returns>The loaded workout routine or null if not found</returns>
        public async Task<WorkoutRoutine?> LoadRoutineAsync(string routineName)
        {
            try
            {
                var fileName = routineName.EndsWith(".json") ? routineName : $"{routineName}.json";
                var filePath = Path.Combine(_routinesDirectory, fileName);

                if (!File.Exists(filePath))
                {
                    return null;
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                var routine = JsonSerializer.Deserialize<WorkoutRoutine>(jsonContent, _jsonOptions);

                return routine?.IsValid() == true ? routine : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load routine '{routineName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads a workout schedule from a JSON file
        /// </summary>
        /// <param name="scheduleName">Name of the schedule file (with or without .json extension)</param>
        /// <returns>The loaded workout schedule or null if not found</returns>
        public async Task<WorkoutSchedule?> LoadScheduleAsync(string scheduleName)
        {
            try
            {
                var fileName = scheduleName.EndsWith(".json") ? scheduleName : $"{scheduleName}.json";
                var filePath = Path.Combine(_schedulesDirectory, fileName);

                if (!File.Exists(filePath))
                {
                    return null;
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                var schedule = JsonSerializer.Deserialize<WorkoutSchedule>(jsonContent, _jsonOptions);

                return schedule?.IsValid() == true ? schedule : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load schedule '{scheduleName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all available routine files
        /// </summary>
        /// <returns>List of routine file names (without .json extension)</returns>
        public List<string> GetAvailableRoutines()
        {
            if (!Directory.Exists(_routinesDirectory))
                return new List<string>();

            return Directory.GetFiles(_routinesDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList()!;
        }

        /// <summary>
        /// Gets all available schedule files
        /// </summary>
        /// <returns>List of schedule file names (without .json extension)</returns>
        public List<string> GetAvailableSchedules()
        {
            if (!Directory.Exists(_schedulesDirectory))
                return new List<string>();

            return Directory.GetFiles(_schedulesDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList()!;
        }

        /// <summary>
        /// Saves a workout routine to a JSON file
        /// </summary>
        /// <param name="routine">The routine to save</param>
        /// <param name="fileName">Optional file name (defaults to routine name)</param>
        public async Task SaveRoutineAsync(WorkoutRoutine routine, string? fileName = null)
        {
            if (!routine.IsValid())
                throw new ArgumentException("Invalid routine data", nameof(routine));

            fileName ??= routine.Name.ToLowerInvariant().Replace(" ", "-");
            if (!fileName.EndsWith(".json"))
                fileName += ".json";

            var filePath = Path.Combine(_routinesDirectory, fileName);
            var jsonContent = JsonSerializer.Serialize(routine, _jsonOptions);
            
            await File.WriteAllTextAsync(filePath, jsonContent);
        }

        /// <summary>
        /// Saves a workout schedule to a JSON file
        /// </summary>
        /// <param name="schedule">The schedule to save</param>
        /// <param name="fileName">Optional file name (defaults to schedule name)</param>
        public async Task SaveScheduleAsync(WorkoutSchedule schedule, string? fileName = null)
        {
            if (!schedule.IsValid())
                throw new ArgumentException("Invalid schedule data", nameof(schedule));

            fileName ??= schedule.Name.ToLowerInvariant().Replace(" ", "-");
            if (!fileName.EndsWith(".json"))
                fileName += ".json";

            var filePath = Path.Combine(_schedulesDirectory, fileName);
            var jsonContent = JsonSerializer.Serialize(schedule, _jsonOptions);
            
            await File.WriteAllTextAsync(filePath, jsonContent);
        }
    }
}
