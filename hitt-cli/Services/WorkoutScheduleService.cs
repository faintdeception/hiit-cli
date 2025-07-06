using hitt_cli.Models;

namespace hitt_cli.Services
{
    /// <summary>
    /// Service for managing workout scheduling logic
    /// </summary>
    public class WorkoutScheduleService
    {
        private readonly WorkoutDataService _dataService;

        public WorkoutScheduleService(WorkoutDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }

        /// <summary>
        /// Gets the appropriate workout routine for the current day and time
        /// </summary>
        /// <param name="scheduleName">Name of the schedule to use (optional, uses first available if not specified)</param>
        /// <returns>The workout routine to execute, or null if none found</returns>
        public async Task<WorkoutRoutine?> GetCurrentWorkoutAsync(string? scheduleName = null)
        {
            var now = DateTime.Now;
            return await GetWorkoutForTimeAsync(now.DayOfWeek, TimeOnly.FromDateTime(now), scheduleName);
        }

        /// <summary>
        /// Gets the appropriate workout routine for a specific day and time
        /// </summary>
        /// <param name="dayOfWeek">Day of the week</param>
        /// <param name="timeOfDay">Time of day</param>
        /// <param name="scheduleName">Name of the schedule to use (optional, uses first available if not specified)</param>
        /// <returns>The workout routine to execute, or null if none found</returns>
        public async Task<WorkoutRoutine?> GetWorkoutForTimeAsync(DayOfWeek dayOfWeek, TimeOnly timeOfDay, string? scheduleName = null)
        {
            // Load the schedule
            var schedule = await LoadScheduleAsync(scheduleName);
            if (schedule == null)
                return null;

            // Find the appropriate routine for the current time
            var scheduleEntry = schedule.GetRoutineForTime(dayOfWeek, timeOfDay);
            if (scheduleEntry == null)
                return null;

            // Load and return the routine
            return await _dataService.LoadRoutineAsync(scheduleEntry.RoutineName);
        }

        /// <summary>
        /// Gets all workouts scheduled for today
        /// </summary>
        /// <param name="scheduleName">Name of the schedule to use (optional, uses first available if not specified)</param>
        /// <returns>List of scheduled workouts for today</returns>
        public async Task<List<ScheduledWorkout>> GetTodaysWorkoutsAsync(string? scheduleName = null)
        {
            var today = DateTime.Now.DayOfWeek;
            return await GetWorkoutsForDayAsync(today, scheduleName);
        }

        /// <summary>
        /// Gets all workouts scheduled for a specific day
        /// </summary>
        /// <param name="dayOfWeek">Day of the week</param>
        /// <param name="scheduleName">Name of the schedule to use (optional, uses first available if not specified)</param>
        /// <returns>List of scheduled workouts for the specified day</returns>
        public async Task<List<ScheduledWorkout>> GetWorkoutsForDayAsync(DayOfWeek dayOfWeek, string? scheduleName = null)
        {
            var result = new List<ScheduledWorkout>();
            
            // Load the schedule
            var schedule = await LoadScheduleAsync(scheduleName);
            if (schedule == null)
                return result;

            // Get all entries for the specified day
            var dayEntries = schedule.GetRoutinesForDay(dayOfWeek);

            // Load each routine and create scheduled workout objects
            foreach (var entry in dayEntries)
            {
                var routine = await _dataService.LoadRoutineAsync(entry.RoutineName);
                if (routine != null)
                {
                    result.Add(new ScheduledWorkout
                    {
                        Routine = routine,
                        ScheduleEntry = entry,
                        DayOfWeek = dayOfWeek
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the next scheduled workout after the current time
        /// </summary>
        /// <param name="scheduleName">Name of the schedule to use (optional, uses first available if not specified)</param>
        /// <returns>The next scheduled workout, or null if none found</returns>
        public async Task<ScheduledWorkout?> GetNextWorkoutAsync(string? scheduleName = null)
        {
            var now = DateTime.Now;
            var currentTime = TimeOnly.FromDateTime(now);
            
            // First, check if there's a workout later today
            var todaysWorkouts = await GetWorkoutsForDayAsync(now.DayOfWeek, scheduleName);
            var laterToday = todaysWorkouts
                .Where(w => w.ScheduleEntry.StartTime > currentTime)
                .OrderBy(w => w.ScheduleEntry.StartTime)
                .FirstOrDefault();

            if (laterToday != null)
                return laterToday;

            // If no workout later today, check the next 7 days
            for (int i = 1; i <= 7; i++)
            {
                var futureDay = now.AddDays(i).DayOfWeek;
                var futureWorkouts = await GetWorkoutsForDayAsync(futureDay, scheduleName);
                
                if (futureWorkouts.Any())
                {
                    var nextWorkout = futureWorkouts.OrderBy(w => w.ScheduleEntry.StartTime).First();
                    nextWorkout.DayOfWeek = futureDay;
                    return nextWorkout;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads a schedule, using the first available if no specific name is provided
        /// </summary>
        /// <param name="scheduleName">Optional schedule name</param>
        /// <returns>The loaded schedule or null if none found</returns>
        private async Task<WorkoutSchedule?> LoadScheduleAsync(string? scheduleName)
        {
            if (!string.IsNullOrEmpty(scheduleName))
            {
                return await _dataService.LoadScheduleAsync(scheduleName);
            }

            // Use the first available schedule
            var availableSchedules = _dataService.GetAvailableSchedules();
            if (availableSchedules.Any())
            {
                return await _dataService.LoadScheduleAsync(availableSchedules.First());
            }

            return null;
        }

        /// <summary>
        /// Gets all available schedules
        /// </summary>
        /// <returns>List of schedule names</returns>
        public List<string> GetAvailableSchedules()
        {
            return _dataService.GetAvailableSchedules();
        }

        /// <summary>
        /// Gets all available routines
        /// </summary>
        /// <returns>List of routine names</returns>
        public List<string> GetAvailableRoutines()
        {
            return _dataService.GetAvailableRoutines();
        }
    }

    /// <summary>
    /// Represents a workout routine scheduled for a specific time
    /// </summary>
    public class ScheduledWorkout
    {
        public WorkoutRoutine Routine { get; set; } = new();
        public ScheduleEntry ScheduleEntry { get; set; } = new();
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Checks if this workout is currently active
        /// </summary>
        /// <returns>True if the workout should be running now</returns>
        public bool IsActiveNow()
        {
            var now = DateTime.Now;
            return now.DayOfWeek == DayOfWeek && 
                   ScheduleEntry.IsActiveOn(now.DayOfWeek, TimeOnly.FromDateTime(now));
        }

        /// <summary>
        /// Gets the formatted time range for this workout
        /// </summary>
        /// <returns>Time range string</returns>
        public string GetTimeRange()
        {
            return $"{ScheduleEntry.StartTime:HH:mm} - {ScheduleEntry.EndTime:HH:mm}";
        }

        public override string ToString()
        {
            return $"{Routine.Name} ({DayOfWeek} {GetTimeRange()})";
        }
    }
}
