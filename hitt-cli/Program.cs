using hitt_cli.Services;
using Spectre.Console;

namespace hitt_cli
{
    internal class Program
    {
        private static WorkoutDataService _dataService = null!;
        private static WorkoutScheduleService _scheduleService = null!;
        private static WorkoutExecutionService _executionService = null!;

        static async Task Main(string[] args)
        {
            try
            {
                // Initialize services
                InitializeServices();

                // Display welcome header
                DisplayWelcomeHeader();

                // Handle command line arguments or show interactive menu
                if (args.Length > 0)
                {
                    await HandleCommandLineArgs(args);
                }
                else
                {
                    await ShowInteractiveMenu();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                Environment.Exit(1);
            }
        }

        static void InitializeServices()
        {
            // Initialize services with the Data directory in the current working directory
            // This allows the tool to work from any directory if Data folder is present
            var currentDir = Directory.GetCurrentDirectory();
            var dataPath = Path.Combine(currentDir, "Data");
            
            // If Data directory doesn't exist in current directory, use the application directory
            if (!Directory.Exists(dataPath))
            {
                dataPath = Path.Combine(AppContext.BaseDirectory, "Data");
            }

            _dataService = new WorkoutDataService(dataPath);
            _scheduleService = new WorkoutScheduleService(_dataService);
            _executionService = new WorkoutExecutionService();
        }

        static void DisplayWelcomeHeader()
        {
            AnsiConsole.Write(
                new FigletText("HITT CLI")
                    .LeftJustified()
                    .Color(Color.Red));

            AnsiConsole.MarkupLine("[dim]High Intensity Interval Training - Command Line Interface[/]");
            AnsiConsole.WriteLine();
        }

        static async Task HandleCommandLineArgs(string[] args)
        {
            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "now":
                case "start":
                    await StartCurrentWorkout();
                    break;
                case "today":
                    await ShowTodaysWorkouts();
                    break;
                case "next":
                    await ShowNextWorkout();
                    break;
                case "list":
                    ShowAvailableContent();
                    break;
                case "run":
                    if (args.Length > 1)
                        await RunSpecificRoutine(args[1]);
                    else
                        AnsiConsole.MarkupLine("[red]Please specify a routine name to run[/]");
                    break;
                case "preview":
                    if (args.Length > 1)
                        await PreviewRoutine(args[1]);
                    else
                        AnsiConsole.MarkupLine("[red]Please specify a routine name to preview[/]");
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                default:
                    AnsiConsole.MarkupLine($"[red]Unknown command: {command}[/]");
                    ShowHelp();
                    break;
            }
        }

        static async Task ShowInteractiveMenu()
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]What would you like to do?[/]")
                    .AddChoices(new[]
                    {
                        "🏃‍♂️ Start Current Workout",
                        "📅 View Today's Schedule",
                        "⏰ Show Next Workout",
                        "📝 List Available Content",
                        "🎯 Run Specific Routine",
                        "👀 Preview Routine",
                        "❓ Help",
                        "🚪 Exit"
                    }));

            switch (choice)
            {
                case "🏃‍♂️ Start Current Workout":
                    await StartCurrentWorkout();
                    break;
                case "📅 View Today's Schedule":
                    await ShowTodaysWorkouts();
                    break;
                case "⏰ Show Next Workout":
                    await ShowNextWorkout();
                    break;
                case "📝 List Available Content":
                    ShowAvailableContent();
                    break;
                case "🎯 Run Specific Routine":
                    await InteractiveRunRoutine();
                    break;
                case "👀 Preview Routine":
                    await InteractivePreviewRoutine();
                    break;
                case "❓ Help":
                    ShowHelp();
                    break;
                case "🚪 Exit":
                    return;
            }
        }

        static async Task StartCurrentWorkout()
        {
            var currentWorkout = await _scheduleService.GetCurrentWorkoutAsync();
            
            if (currentWorkout == null)
            {
                AnsiConsole.MarkupLine("[yellow]No workout scheduled for this time.[/]");
                
                var nextWorkout = await _scheduleService.GetNextWorkoutAsync();
                if (nextWorkout != null)
                {
                    AnsiConsole.MarkupLine($"[dim]Next workout: {nextWorkout}[/]");
                }
                return;
            }

            AnsiConsole.MarkupLine($"[green]Starting scheduled workout: {currentWorkout.Name}[/]");
            await _executionService.ExecuteRoutineAsync(currentWorkout);
        }

        static async Task ShowTodaysWorkouts()
        {
            var todaysWorkouts = await _scheduleService.GetTodaysWorkoutsAsync();
            
            if (!todaysWorkouts.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No workouts scheduled for today.[/]");
                return;
            }

            var table = new Table();
            table.AddColumn("[bold]Time[/]");
            table.AddColumn("[bold]Workout[/]");
            table.AddColumn("[bold]Duration[/]");
            table.AddColumn("[bold]Status[/]");

            foreach (var workout in todaysWorkouts.OrderBy(w => w.ScheduleEntry.StartTime))
            {
                var status = workout.IsActiveNow() ? "[green]● Active[/]" : 
                            workout.ScheduleEntry.StartTime < TimeOnly.FromDateTime(DateTime.Now) ? "[dim]○ Completed[/]" : 
                            "[blue]○ Upcoming[/]";

                table.AddRow(
                    workout.GetTimeRange(),
                    workout.Routine.Name,
                    workout.Routine.GetFormattedTotalTime(),
                    status
                );
            }

            AnsiConsole.Write(table);
        }

        static async Task ShowNextWorkout()
        {
            var nextWorkout = await _scheduleService.GetNextWorkoutAsync();
            
            if (nextWorkout == null)
            {
                AnsiConsole.MarkupLine("[yellow]No upcoming workouts found in the next 7 days.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[bold]Next Workout:[/] {nextWorkout}");
            AnsiConsole.MarkupLine($"[dim]Duration: {nextWorkout.Routine.GetFormattedTotalTime()}[/]");
            
            if (!string.IsNullOrEmpty(nextWorkout.ScheduleEntry.Note))
            {
                AnsiConsole.MarkupLine($"[dim]Note: {nextWorkout.ScheduleEntry.Note}[/]");
            }
        }

        static void ShowAvailableContent()
        {
            AnsiConsole.MarkupLine("[bold yellow]Available Schedules:[/]");
            var schedules = _scheduleService.GetAvailableSchedules();
            if (schedules.Any())
            {
                foreach (var schedule in schedules)
                {
                    AnsiConsole.MarkupLine($"  • {schedule}");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[dim]  No schedules found[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Available Routines:[/]");
            var routines = _scheduleService.GetAvailableRoutines();
            if (routines.Any())
            {
                foreach (var routine in routines)
                {
                    AnsiConsole.MarkupLine($"  • {routine}");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[dim]  No routines found[/]");
            }
        }

        static async Task RunSpecificRoutine(string routineName)
        {
            var routine = await _dataService.LoadRoutineAsync(routineName);
            
            if (routine == null)
            {
                AnsiConsole.MarkupLine($"[red]Routine '{routineName}' not found.[/]");
                return;
            }

            await _executionService.ExecuteRoutineAsync(routine);
        }

        static async Task InteractiveRunRoutine()
        {
            var routines = _scheduleService.GetAvailableRoutines();
            if (!routines.Any())
            {
                AnsiConsole.MarkupLine("[red]No routines available.[/]");
                return;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select a routine to run:[/]")
                    .AddChoices(routines));

            await RunSpecificRoutine(choice);
        }

        static async Task PreviewRoutine(string routineName)
        {
            var routine = await _dataService.LoadRoutineAsync(routineName);
            
            if (routine == null)
            {
                AnsiConsole.MarkupLine($"[red]Routine '{routineName}' not found.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[bold yellow]Preview: {routine.Name}[/]");
            if (!string.IsNullOrEmpty(routine.Description))
            {
                AnsiConsole.MarkupLine($"[dim]{routine.Description}[/]");
            }
            AnsiConsole.WriteLine();

            _executionService.PreviewRoutine(routine);
        }

        static async Task InteractivePreviewRoutine()
        {
            var routines = _scheduleService.GetAvailableRoutines();
            if (!routines.Any())
            {
                AnsiConsole.MarkupLine("[red]No routines available.[/]");
                return;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Select a routine to preview:[/]")
                    .AddChoices(routines));

            await PreviewRoutine(choice);
        }

        static void ShowHelp()
        {
            AnsiConsole.MarkupLine("[bold yellow]HITT CLI - Usage:[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Commands:[/]");
            AnsiConsole.MarkupLine("  [green]hitt[/]                    - Interactive mode");
            AnsiConsole.MarkupLine("  [green]hitt now[/]                - Start current scheduled workout");
            AnsiConsole.MarkupLine("  [green]hitt today[/]              - View today's workout schedule");
            AnsiConsole.MarkupLine("  [green]hitt next[/]               - Show next scheduled workout");
            AnsiConsole.MarkupLine("  [green]hitt list[/]               - List available schedules and routines");
            AnsiConsole.MarkupLine("  [green]hitt run <routine>[/]      - Run a specific routine");
            AnsiConsole.MarkupLine("  [green]hitt preview <routine>[/]  - Preview a routine without running");
            AnsiConsole.MarkupLine("  [green]hitt help[/]               - Show this help");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Place your workout data files in a 'Data' directory with 'Routines' and 'Schedules' subdirectories.[/]");
        }
    }
}
