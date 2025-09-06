using hiit_cli.Services;
using Spectre.Console;

namespace hiit_cli
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
                // Force UTF-8 output encoding for emoji support
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                
                // Check for emoji support flags
                if (args.Contains("--no-emoji") || args.Contains("--text-only"))
                {
                    DisplayService.SetEmojiSupport(false);
                }
                else if (args.Contains("--emoji"))
                {
                    DisplayService.SetEmojiSupport(true);
                }

                // Check for audio support flags
                if (args.Contains("--no-audio") || args.Contains("--silent"))
                {
                    DisplayService.SetAudioEnabled(false);
                }
                else if (args.Contains("--audio"))
                {
                    DisplayService.SetAudioEnabled(true);
                }

                // Check for audio debug flag
                if (args.Contains("--audio-debug"))
                {
                    DisplayService.SetAudioDebugEnabled(true);
                }

                // Initialize services
                InitializeServices();

                // Display welcome header
                DisplayWelcomeHeader();

                // Handle command line arguments or show interactive menu
                if (args.Length > 0 && !args.All(a => a.StartsWith("--")))
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
            // Initialize services - WorkoutDataService will handle proper data directory selection
            _dataService = new WorkoutDataService();
            _scheduleService = new WorkoutScheduleService(_dataService);
            _executionService = new WorkoutExecutionService();
        }

        static void DisplayWelcomeHeader()
        {
            AnsiConsole.Write(
                new FigletText("HIIT CLI")
                    .LeftJustified()
                    .Color(Color.Red));

            AnsiConsole.MarkupLine("[dim]High Intensity Interval Training - Command Line Interface[/]");
            
            // Show current settings status
            var emojiStatus = DisplayService.SupportsEmojis ? "[green]ON[/]" : "[red]OFF[/]";
            var audioStatus = DisplayService.AudioEnabled ? "[green]ON[/]" : "[red]OFF[/]";
            var debugInfo = DisplayService.AudioDebugEnabled ? " | Audio Debug: [yellow]ON[/]" : "";
            AnsiConsole.MarkupLine($"[dim]Emojis: {emojiStatus} | Audio: {audioStatus}{debugInfo}[/]");
            AnsiConsole.WriteLine();
        }

        static async Task HandleCommandLineArgs(string[] args)
        {
            // Filter out emoji flags
            var filteredArgs = args.Where(a => !a.StartsWith("--")).ToArray();
            if (filteredArgs.Length == 0)
            {
                await ShowInteractiveMenu();
                return;
            }

            var command = filteredArgs[0].ToLowerInvariant();

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
                    if (filteredArgs.Length > 1)
                        await RunSpecificRoutine(filteredArgs[1]);
                    else
                        AnsiConsole.MarkupLine("[red]Please specify a routine name to run[/]");
                    break;
                case "preview":
                    if (filteredArgs.Length > 1)
                        await PreviewRoutine(filteredArgs[1]);
                    else
                        AnsiConsole.MarkupLine("[red]Please specify a routine name to preview[/]");
                    break;
                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;
                case "debug-audio":
                    _executionService.TestAudioPath();
                    break;
                case "data":
                case "data-dir":
                case "datadir":
                    ShowDataDirectory();
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
                        $"{DisplayService.Runner} Start Current Workout",
                        $"{DisplayService.Calendar} View Today's Schedule",
                        $"{DisplayService.Clock} Show Next Workout",
                        $"{DisplayService.Document} List Available Content",
                        $"{DisplayService.Fire} Run Specific Routine",
                        $"{DisplayService.Eyes} Preview Routine",
                        $"{DisplayService.Question} Help",
                        $"{DisplayService.Door} Exit"
                    }));

            switch (choice)
            {
                case var c when c.Contains("Start Current Workout"):
                    await StartCurrentWorkout();
                    break;
                case var c when c.Contains("View Today's Schedule"):
                    await ShowTodaysWorkouts();
                    break;
                case var c when c.Contains("Show Next Workout"):
                    await ShowNextWorkout();
                    break;
                case var c when c.Contains("List Available Content"):
                    ShowAvailableContent();
                    break;
                case var c when c.Contains("Run Specific Routine"):
                    await InteractiveRunRoutine();
                    break;
                case var c when c.Contains("Preview Routine"):
                    await InteractivePreviewRoutine();
                    break;
                case var c when c.Contains("Help"):
                    ShowHelp();
                    break;
                case var c when c.Contains("Exit"):
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
            AnsiConsole.MarkupLine("[bold yellow]HIIT CLI - Usage:[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Commands:[/]");
            AnsiConsole.MarkupLine("  [green]hiit[/]                    - Interactive mode");
            AnsiConsole.MarkupLine("  [green]hiit now[/]                - Start current scheduled workout");
            AnsiConsole.MarkupLine("  [green]hiit today[/]              - View today's workout schedule");
            AnsiConsole.MarkupLine("  [green]hiit next[/]               - Show next scheduled workout");
            AnsiConsole.MarkupLine("  [green]hiit list[/]               - List available schedules and routines");
            AnsiConsole.MarkupLine("  [green]hiit run <routine>[/]      - Run a specific routine");
            AnsiConsole.MarkupLine("  [green]hiit preview <routine>[/]  - Preview a routine without running");
            AnsiConsole.MarkupLine("  [green]hiit debug-audio[/]        - Test audio playback and paths");
            AnsiConsole.MarkupLine("  [green]hiit data[/]               - Show data directory location and status");
            AnsiConsole.MarkupLine("  [green]hiit help[/]               - Show this help");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Options:[/]");
            AnsiConsole.MarkupLine("  [cyan]--emoji[/]                  - Force emoji display (default on Unix)");
            AnsiConsole.MarkupLine("  [cyan]--no-emoji, --text-only[/]  - Use text symbols instead of emojis");
            AnsiConsole.MarkupLine("  [cyan]--audio[/]                  - Enable audio cues for timers (default)");
            AnsiConsole.MarkupLine("  [cyan]--no-audio, --silent[/]     - Disable all audio cues");
            AnsiConsole.MarkupLine("  [cyan]--audio-debug[/]            - Show audio method being used for playback");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Environment variables:[/]");
            AnsiConsole.MarkupLine("[dim]  HIIT_CLI_EMOJIS=true/false      - Control emoji display[/]");
            AnsiConsole.MarkupLine("[dim]  HIIT_CLI_AUDIO=true/false       - Control audio cues[/]");
            AnsiConsole.MarkupLine("[dim]  HIIT_CLI_AUDIO_DEBUG=true/false - Show audio debug info[/]");
            AnsiConsole.MarkupLine("[dim]You can add custom workout routines and schedules to your data directory.[/]");
        }

        static void ShowDataDirectory()
        {
            AnsiConsole.MarkupLine("[bold yellow]HIIT CLI - Data Directory Information:[/]");
            AnsiConsole.WriteLine();
            
            AnsiConsole.MarkupLine($"[bold]Data Directory:[/] [green]{_dataService.DataDirectory}[/]");
            AnsiConsole.MarkupLine($"[bold]Routines:[/] [cyan]{_dataService.RoutinesDirectory}[/]");
            AnsiConsole.MarkupLine($"[bold]Schedules:[/] [cyan]{_dataService.SchedulesDirectory}[/]");
            AnsiConsole.WriteLine();
            
            // Show directory status
            var dataExists = Directory.Exists(_dataService.DataDirectory);
            var routinesCount = dataExists ? Directory.GetFiles(_dataService.RoutinesDirectory, "*.json").Length : 0;
            var schedulesCount = dataExists ? Directory.GetFiles(_dataService.SchedulesDirectory, "*.json").Length : 0;
            
            AnsiConsole.MarkupLine($"[bold]Status:[/]");
            AnsiConsole.MarkupLine($"  • Data directory: {(dataExists ? "[green]EXISTS[/]" : "[red]NOT FOUND[/]")}");
            AnsiConsole.MarkupLine($"  • Routine files: [cyan]{routinesCount}[/] found");
            AnsiConsole.MarkupLine($"  • Schedule files: [cyan]{schedulesCount}[/] found");
            AnsiConsole.WriteLine();
            
            AnsiConsole.MarkupLine("[dim]You can add custom JSON files to these directories to extend your workout library.[/]");
        }
    }
}
