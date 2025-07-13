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

        /// <summary>
        /// Gets the path to the data directory being used
        /// </summary>
        public string DataDirectory => _dataDirectory;

        /// <summary>
        /// Gets the path to the routines directory
        /// </summary>
        public string RoutinesDirectory => _routinesDirectory;

        /// <summary>
        /// Gets the path to the schedules directory
        /// </summary>
        public string SchedulesDirectory => _schedulesDirectory;

        public WorkoutDataService(string? dataDirectory = null)
        {
            _dataDirectory = dataDirectory ?? GetDefaultDataDirectory();
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
        /// Gets the platform-appropriate default data directory
        /// </summary>
        private static string GetDefaultDataDirectory()
        {
            if (OperatingSystem.IsWindows())
            {
                // Windows: %LOCALAPPDATA%\HITT-CLI
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, "HITT-CLI");
            }
            else if (OperatingSystem.IsMacOS())
            {
                // macOS: ~/Library/Application Support/HITT-CLI
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homeDir, "Library", "Application Support", "HITT-CLI");
            }
            else
            {
                // Linux/Unix: ~/.local/share/HITT-CLI or $XDG_DATA_HOME/HITT-CLI
                var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (!string.IsNullOrEmpty(xdgDataHome))
                {
                    return Path.Combine(xdgDataHome, "HITT-CLI");
                }
                
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homeDir, ".local", "share", "HITT-CLI");
            }
        }

        /// <summary>
        /// Ensures the required directories exist and copies default data files if needed
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_routinesDirectory);
            Directory.CreateDirectory(_schedulesDirectory);
            
            // Copy default data files if user directories are empty
            CopyDefaultDataFiles();
        }

        /// <summary>
        /// Copies default routine and schedule files from the app bundle to user data directory
        /// </summary>
        private void CopyDefaultDataFiles()
        {
            var appDataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            
            if (Directory.Exists(appDataDirectory))
            {
                // Copy routines if user directory is empty
                if (!Directory.GetFiles(_routinesDirectory, "*.json").Any())
                {
                    CopyFilesFromDirectory(Path.Combine(appDataDirectory, "Routines"), _routinesDirectory);
                }
                
                // Copy schedules if user directory is empty
                if (!Directory.GetFiles(_schedulesDirectory, "*.json").Any())
                {
                    CopyFilesFromDirectory(Path.Combine(appDataDirectory, "Schedules"), _schedulesDirectory);
                }
            }
        }

        /// <summary>
        /// Copies all JSON files from source directory to destination directory
        /// </summary>
        private void CopyFilesFromDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir)) return;
            
            try
            {
                var jsonFiles = Directory.GetFiles(sourceDir, "*.json");
                foreach (var sourceFile in jsonFiles)
                {
                    var fileName = Path.GetFileName(sourceFile);
                    var destFile = Path.Combine(destDir, fileName);
                    
                    if (!File.Exists(destFile))
                    {
                        File.Copy(sourceFile, destFile);
                    }
                }
            }
            catch
            {
                // Silently ignore copy errors - app will still function
            }
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
