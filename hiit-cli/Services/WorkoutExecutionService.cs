using hiit_cli.Models;
using Spectre.Console;

namespace hiit_cli.Services
{
    /// <summary>
    /// Service for executing workout routines with timing and progress tracking
    /// </summary>
    public class WorkoutExecutionService
    {
        /// <summary>
        /// Executes a workout routine with interactive timer and progress display
        /// </summary>
        /// <param name="routine">The workout routine to execute</param>
        /// <param name="cancellationToken">Cancellation token to stop the workout</param>
        public async Task ExecuteRoutineAsync(WorkoutRoutine routine, CancellationToken cancellationToken = default)
        {
            if (!routine.IsValid())
                throw new ArgumentException("Invalid routine", nameof(routine));

            AnsiConsole.Clear();
            
            // Display routine header
            DisplayRoutineHeader(routine);

            var totalWorkouts = routine.Workouts.Count;
            
            // Execute each rep of the routine
            for (int currentRep = 1; currentRep <= routine.Reps; currentRep++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Display rep header for multi-rep routines
                if (routine.Reps > 1)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(new Rule($"[bold cyan]{DisplayService.Fire} REP {currentRep} of {routine.Reps} {DisplayService.Fire}[/]").RuleStyle("cyan"));
                    AnsiConsole.WriteLine();
                    
                    if (currentRep > 1)
                    {
                        AnsiConsole.MarkupLine("[yellow]Take a moment to prepare for the next round...[/]");
                        await Task.Delay(3000, cancellationToken);
                    }
                }

                // Execute each workout in the current rep
                for (int workoutIndex = 1; workoutIndex <= totalWorkouts; workoutIndex++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var workout = routine.Workouts[workoutIndex - 1];
                    await ExecuteWorkoutAsync(workout, workoutIndex, totalWorkouts, currentRep, routine.Reps, routine, cancellationToken);
                    
                    // Handle rest between exercises (not after the last exercise in the last rep)
                    var isLastExerciseInLastRep = (workoutIndex == totalWorkouts && currentRep == routine.Reps);
                    
                    if (!isLastExerciseInLastRep && !cancellationToken.IsCancellationRequested)
                    {
                        AnsiConsole.WriteLine();
                        
                        if (routine.Workouts.All(w => w.Rest == 0)) // No-rest routine
                        {
                            AnsiConsole.MarkupLine($"[dim]Moving to next exercise...[/]");
                            await Task.Delay(1000, cancellationToken);
                        }
                        else if (workout.Rest > 0)
                        {
                            // Use the actual rest time specified for this exercise
                            AnsiConsole.MarkupLine($"[yellow]Rest time - catch your breath![/]");
                            await ExecuteTimerAsync("Rest", workout.Rest, Color.Yellow, cancellationToken);
                        }
                        else
                        {
                            // Brief preparation for exercises with no rest specified
                            AnsiConsole.MarkupLine($"[dim]Preparing for next exercise...[/]");
                            await Task.Delay(2000, cancellationToken);
                        }
                    }
                }

                // Rep completion message for multi-rep routines
                if (routine.Reps > 1 && currentRep < routine.Reps && !cancellationToken.IsCancellationRequested)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[bold green]✅ REP {currentRep} COMPLETE![/] [dim]({routine.Reps - currentRep} reps remaining)[/]");
                    
                    // Longer rest between reps
                    if (currentRep < routine.Reps)
                    {
                        AnsiConsole.MarkupLine("[yellow]Rest between reps - catch your breath![/]");
                        await Task.Delay(5000, cancellationToken); // 5 second rest between reps
                    }
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                DisplayWorkoutComplete(routine);
            }
        }

        /// <summary>
        /// Executes a single workout with sets and rest periods
        /// </summary>
        private async Task ExecuteWorkoutAsync(Workout workout, int workoutNumber, int totalWorkouts, int currentRep, int totalReps, WorkoutRoutine routine, CancellationToken cancellationToken)
        {
            var isNoRestWorkout = workout.Rest == 0;
            
            AnsiConsole.WriteLine();
            
            // Enhanced header for multi-rep routines
            var repInfo = totalReps > 1 ? $" (Rep {currentRep}/{totalReps})" : "";
            AnsiConsole.Write(new Rule($"[bold blue]Exercise {workoutNumber}/{totalWorkouts}: {workout.Name}{repInfo}[/]").RuleStyle("blue"));
            
            if (!string.IsNullOrEmpty(workout.Description))
            {
                AnsiConsole.MarkupLine($"[dim]{workout.Description}[/]");
                AnsiConsole.WriteLine();
            }

            // Enhanced display for no-rest vs regular workouts
            if (isNoRestWorkout)
            {
                AnsiConsole.MarkupLine($"[yellow]Sets: {workout.Sets} | Duration: {workout.Length}s | [bold red]NO REST[/][/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]Sets: {workout.Sets} | Duration: {workout.Length}s | Rest: {workout.Rest}s[/]");
            }
            
            // Show overall progress across all reps
            if (totalWorkouts > 1 || totalReps > 1)
            {
                var totalExercisesAcrossAllReps = totalWorkouts * totalReps;
                var currentExerciseNumber = ((currentRep - 1) * totalWorkouts) + workoutNumber;
                var progressPercent = (double)currentExerciseNumber / totalExercisesAcrossAllReps * 100;
                AnsiConsole.MarkupLine($"[dim]Overall Progress: {currentExerciseNumber}/{totalExercisesAcrossAllReps} exercises ({progressPercent:F0}% complete)[/]");
            }
            
            AnsiConsole.WriteLine();

            for (int currentSet = 1; currentSet <= workout.Sets; currentSet++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Enhanced set display
                var setPrefix = workout.Sets > 1 ? $"Set {currentSet}/{workout.Sets} - " : "";
                AnsiConsole.MarkupLine($"[bold green]{setPrefix}GET READY![/]");
                
                // Brief prepare time for intense workouts
                if (isNoRestWorkout && currentSet == 1)
                {
                    AnsiConsole.MarkupLine("[dim]Prepare for intense action...[/]");
                    await Task.Delay(1500, cancellationToken);
                }

                // Execute the set with enhanced messaging
                var activityName = $"GO! {workout.Name}";
                await ExecuteTimerAsync(activityName, workout.Length, Color.Green, cancellationToken);
                
                AnsiConsole.MarkupLine("[bold green]✓ Set Complete![/]");

                // Enhanced rest handling
                if (currentSet < workout.Sets)
                {
                    if (workout.Rest > 0)
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine($"[yellow]Rest time - catch your breath![/]");
                        await ExecuteTimerAsync("Rest", workout.Rest, Color.Yellow, cancellationToken);
                        AnsiConsole.WriteLine();
                    }
                    else if (isNoRestWorkout)
                    {
                        // Brief transition for no-rest workouts
                        AnsiConsole.MarkupLine("[bold yellow]No rest! Keep going...[/]");
                        await Task.Delay(1000, cancellationToken); // 1-second transition
                    }
                }
            }
            
            // Enhanced completion messaging
            if (!cancellationToken.IsCancellationRequested)
            {
                var nextExerciseName = GetNextExerciseName(routine, workoutNumber, totalWorkouts, currentRep, totalReps);
                
                if (isNoRestWorkout && (workoutNumber < totalWorkouts || currentRep < totalReps))
                {
                    if (!string.IsNullOrEmpty(nextExerciseName))
                    {
                        AnsiConsole.MarkupLine($"[bold cyan]{DisplayService.Fire} Exercise {workoutNumber} crushed! Moving to [yellow]{nextExerciseName}[/]...[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[bold cyan]{DisplayService.Fire} Exercise {workoutNumber} crushed! Moving to next...[/]");
                    }
                    await Task.Delay(1500, cancellationToken);
                }
                else if (workoutNumber < totalWorkouts || currentRep < totalReps)
                {
                    if (!string.IsNullOrEmpty(nextExerciseName))
                    {
                        AnsiConsole.MarkupLine($"[bold cyan]✅ Exercise {workoutNumber} complete! Next exercise: [yellow]{nextExerciseName}[/][/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[bold cyan]✅ Exercise {workoutNumber} complete! Next exercise coming up...[/]");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the next exercise in the routine
        /// </summary>
        private string? GetNextExerciseName(WorkoutRoutine routine, int currentWorkoutNumber, int totalWorkouts, int currentRep, int totalReps)
        {
            // If we're in the middle of the current rep, return the next exercise in this rep
            if (currentWorkoutNumber < totalWorkouts)
            {
                return routine.Workouts[currentWorkoutNumber].Name; // currentWorkoutNumber is 1-based, array is 0-based, so this gets the next exercise
            }
            
            // If we're at the end of the current rep but there are more reps, return the first exercise of the next rep
            if (currentRep < totalReps)
            {
                return routine.Workouts[0].Name; // First exercise of the next rep
            }
            
            // No more exercises
            return null;
        }

        /// <summary>
        /// Executes a countdown timer with progress bar and enhanced visual feedback
        /// </summary>
        private async Task ExecuteTimerAsync(string activity, int seconds, Color color, CancellationToken cancellationToken)
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask($"[{color}]{activity}[/]", maxValue: seconds);
                    
                    for (int i = seconds; i > 0; i--)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        // Enhanced countdown with final seconds emphasis
                        if (i <= 3 && activity.StartsWith("GO!"))
                        {
                            task.Description = $"[{color}]{activity}[/] - [bold red]{i}![/]";
                            // Add visual countdown for final seconds
                            AnsiConsole.MarkupLine($"                    [bold red]{i}...[/]");
                        }
                        else if (i <= 5 && activity.StartsWith("GO!"))
                        {
                            task.Description = $"[{color}]{activity}[/] - [yellow]{i}s remaining[/]";
                        }
                        else
                        {
                            task.Description = $"[{color}]{activity}[/] - {i}s remaining";
                        }
                        
                        task.Value = seconds - i;
                        await Task.Delay(1000, cancellationToken);
                    }
                    
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        task.Value = seconds;
                        if (activity.StartsWith("GO!"))
                        {
                            task.Description = $"[{color}]{activity}[/] - [bold green]DONE! {DisplayService.Fire}[/]";
                            
                            // Play completion sound after the exercise is done
                            PlayCompletionSound();
                        }
                        else
                        {
                            task.Description = $"[{color}]{activity}[/] - [green]Complete![/]";
                            
                            // Play softer completion sound for rest periods
                            PlayRestCompletionSound();
                        }
                        
                        // Brief pause to show completion
                        await Task.Delay(500, cancellationToken);
                    }
                });
        }

        /// <summary>
        /// Plays exercise completion sound using the bundled audio file
        /// </summary>
        private void PlayCompletionSound()
        {
            if (!DisplayService.AudioEnabled) return;
            
            _ = Task.Run(() =>
            {
                try
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var audioPath = Path.Combine(baseDirectory, "assets", "audio", "ok-2.wav");
                    
                    if (!File.Exists(audioPath))
                    {
                        if (DisplayService.AudioDebugEnabled)
                            AnsiConsole.MarkupLine("[dim]🔊 Audio file not found, using system beep[/]");
                        
                        PlaySystemBeep();
                        return;
                    }

                    if (OperatingSystem.IsWindows())
                    {
                        if (DisplayService.AudioDebugEnabled)
                            AnsiConsole.MarkupLine("[dim]🔊 Playing Windows completion sound[/]");
                        
                        using var player = new System.Media.SoundPlayer(audioPath);
                        player.Load(); // Explicitly load the file first
                        player.PlaySync(); // Use synchronous playback for reliability
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        if (DisplayService.AudioDebugEnabled)
                            AnsiConsole.MarkupLine("[dim]🔊 Playing Linux completion sound[/]");
                        
                        PlayLinuxSound(audioPath);
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        if (DisplayService.AudioDebugEnabled)
                            AnsiConsole.MarkupLine("[dim]🔊 Playing macOS completion sound[/]");
                        
                        PlayMacSound(audioPath);
                    }
                    else
                    {
                        if (DisplayService.AudioDebugEnabled)
                            AnsiConsole.MarkupLine("[dim]🔊 Playing system beep (unsupported platform)[/]");
                        
                        PlaySystemBeep();
                    }
                }
                catch
                {
                    // If audio file fails, fall back to system beep
                    if (DisplayService.AudioDebugEnabled)
                        AnsiConsole.MarkupLine("[dim]🔊 Audio file failed, falling back to system beep[/]");
                    
                    PlaySystemBeep();
                }
            });
        }

        /// <summary>
        /// Plays rest completion sound (softer system beep)
        /// </summary>
        private void PlayRestCompletionSound()
        {
            if (!DisplayService.AudioEnabled) return;
            
            _ = Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        if (DisplayService.AudioDebugEnabled)
                            AnsiConsole.MarkupLine("[dim]🔊 Playing Windows rest beep[/]");
                        
                        // Gentle single beep for rest completion
                        Console.Beep(700, 200);
                    }
                    else
                    {
                        if (DisplayService.AudioDebugEnabled)
                            AnsiConsole.MarkupLine("[dim]🔊 Playing system alert for rest[/]");
                        
                        Console.Write("\a");
                    }
                }
                catch
                {
                    // Ignore audio errors
                }
            });
        }

        /// <summary>
        /// Plays sound on Windows using System.Media.SoundPlayer
        /// </summary>
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private void PlayWindowsSound(string audioPath)
        {
            try
            {
                // Use the built-in .NET SoundPlayer for Windows
                using var player = new System.Media.SoundPlayer(audioPath);
                player.Play(); // Non-blocking play
            }
            catch
            {
                PlaySystemBeep();
            }
        }

        /// <summary>
        /// Plays sound on Linux using common audio tools
        /// </summary>
        private void PlayLinuxSound(string audioPath)
        {
            try
            {
                // Try different common Linux audio players
                var players = new[] { "aplay", "paplay", "play" };
                
                foreach (var player in players)
                {
                    try
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = player,
                                Arguments = $"\"{audioPath}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            }
                        };
                        
                        process.Start();
                        // Don't wait for completion, let it play in background
                        return;
                    }
                    catch
                    {
                        // Try next player
                        continue;
                    }
                }
                
                // If all players fail, use system beep
                PlaySystemBeep();
            }
            catch
            {
                PlaySystemBeep();
            }
        }

        /// <summary>
        /// Plays sound on macOS using afplay
        /// </summary>
        private void PlayMacSound(string audioPath)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"\"{audioPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                
                process.Start();
                // Don't wait for completion, let it play in background
            }
            catch
            {
                PlaySystemBeep();
            }
        }

        /// <summary>
        /// Fallback system beep for when audio file playback fails
        /// </summary>
        private void PlaySystemBeep()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    if (DisplayService.AudioDebugEnabled)
                        AnsiConsole.MarkupLine("[dim]🔊 Playing Windows system beep fallback[/]");
                    
                    Console.Beep(800, 200);
                }
                else
                {
                    if (DisplayService.AudioDebugEnabled)
                        AnsiConsole.MarkupLine("[dim]🔊 Playing system alert fallback[/]");
                    
                    Console.Write("\a");
                }
            }
            catch
            {
                // Ignore beep errors
            }
        }

        /// <summary>
        /// Displays the routine header with summary information
        /// </summary>
        private void DisplayRoutineHeader(WorkoutRoutine routine)
        {
            var repsInfo = routine.Reps > 1 ? $"\n[blue]Reps:[/] {routine.Reps} rounds" : "";
            
            var panel = new Panel(new Markup($"[bold yellow]{routine.Name}[/]\n" +
                                           $"[dim]{routine.Description ?? ""}[/]\n\n" +
                                           $"[blue]Exercises:[/] {routine.GetExercisesPerRep()} per round\n" +
                                           $"[blue]Total Time:[/] {routine.GetFormattedTotalTime()}\n" +
                                           $"[blue]Difficulty:[/] {routine.Difficulty}/5{repsInfo}\n" +
                                           $"[blue]Tags:[/] {string.Join(", ", routine.Tags)}"))
            {
                Header = new PanelHeader($"[bold]{DisplayService.Runner} Workout Starting[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Blue)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            // Enhanced countdown with reps information
            if (routine.Reps > 1)
            {
                AnsiConsole.MarkupLine($"[dim]Get ready for {routine.Reps} rounds of intensity! Starting in...[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[dim]Get ready! Starting in...[/]");
            }
            
            for (int i = 3; i > 0; i--)
            {
                AnsiConsole.MarkupLine($"[bold red]{i}[/]");
                Thread.Sleep(1000);
            }
            AnsiConsole.MarkupLine("[bold green]GO![/]");
        }

        /// <summary>
        /// Displays the workout completion message with enhanced messaging for different workout types
        /// </summary>
        private void DisplayWorkoutComplete(WorkoutRoutine routine)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold green]🎉 WORKOUT CRUSHED! 🎉[/]").RuleStyle("green"));
            AnsiConsole.WriteLine();

            var isNoRestRoutine = routine.Workouts.All(w => w.Rest == 0);
            
            var completionMessages = isNoRestRoutine ? new[]
            {
                $"INCREDIBLE! You just crushed a no-rest workout! {DisplayService.Fire}{DisplayService.Muscle}",
                $"BEAST MODE ACTIVATED! That was pure intensity! {DisplayService.Lightning}{DisplayService.Hundred}", 
                $"UNSTOPPABLE! You powered through without stopping! {DisplayService.Rocket}{DisplayService.Explosion}",
                $"WARRIOR! That no-rest challenge was conquered! {DisplayService.Sword}{DisplayService.Trophy}",
                $"LEGEND! You just proved your endurance is next level! {DisplayService.Crown}{DisplayService.Fire}"
            } : new[]
            {
                $"Great job! You've completed your workout! {DisplayService.Muscle}",
                $"Excellent work! Your body will thank you later! {DisplayService.Fire}",
                $"Fantastic effort! You're getting stronger every day! {DisplayService.Star}",
                $"Well done! Another step closer to your fitness goals! {DisplayService.Target}",
                $"Amazing work! You showed up and crushed it! {DisplayService.Rocket}"
            };

            var random = new Random();
            var message = completionMessages[random.Next(completionMessages.Length)];
            
            AnsiConsole.MarkupLine($"[bold green]{message}[/]");
            AnsiConsole.WriteLine();
            
            // Enhanced stats for no-rest workouts
            if (isNoRestRoutine)
            {
                var totalExerciseTime = routine.Workouts.Sum(w => w.Sets * w.Length) * routine.Reps;
                var totalSets = routine.Workouts.Sum(w => w.Sets) * routine.Reps;
                AnsiConsole.MarkupLine($"[bold yellow]{DisplayService.Fire} INTENSITY STATS:[/]");
                AnsiConsole.MarkupLine($"   • [green]Pure exercise time:[/] {totalExerciseTime / 60}m {totalExerciseTime % 60}s");
                AnsiConsole.MarkupLine($"   • [red]Total exercises:[/] {totalSets} sets");
                AnsiConsole.MarkupLine($"   • [blue]Zero rest breaks![/] Maximum intensity achieved!");
                AnsiConsole.WriteLine();
            }
            
            AnsiConsole.MarkupLine($"[dim]Remember to stay hydrated and stretch! {DisplayService.Water}{DisplayService.Meditation}[/]");
        }

        /// <summary>
        /// Previews a workout routine without executing it
        /// </summary>
        /// <param name="routine">The routine to preview</param>
        public void PreviewRoutine(WorkoutRoutine routine)
        {
            if (!routine.IsValid())
                throw new ArgumentException("Invalid routine", nameof(routine));

            var table = new Table();
            table.AddColumn("[bold]Exercise[/]");
            table.AddColumn("[bold]Sets[/]");
            table.AddColumn("[bold]Duration[/]");
            table.AddColumn("[bold]Rest[/]");
            table.AddColumn("[bold]Total Time[/]");

            foreach (var workout in routine.Workouts)
            {
                table.AddRow(
                    workout.Name,
                    workout.Sets.ToString(),
                    $"{workout.Length}s",
                    $"{workout.Rest}s",
                    workout.GetFormattedTotalTime()
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Enhanced summary with reps information
            var repsText = routine.Reps > 1 ? $" x {routine.Reps} reps" : "";
            AnsiConsole.MarkupLine($"[bold]Single Round Time:[/] {new WorkoutRoutine { Workouts = routine.Workouts }.GetFormattedTotalTime()}");
            
            if (routine.Reps > 1)
            {
                AnsiConsole.MarkupLine($"[bold]Total Routine Time:[/] {routine.GetFormattedTotalTime()} [dim]({routine.Reps} reps)[/]");
                AnsiConsole.MarkupLine($"[bold]Total Exercises:[/] {routine.GetTotalExercises()} [dim]({routine.GetExercisesPerRep()} per rep)[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold]Total Routine Time:[/] {routine.GetFormattedTotalTime()}");
            }
        }
        
        /// <summary>
        /// Debug method to test audio file path resolution and playback
        /// </summary>
        public void TestAudioPath()
        {
            AnsiConsole.MarkupLine("[yellow]Testing audio file paths...[/]");
            
            var baseDirectory = AppContext.BaseDirectory;
            AnsiConsole.MarkupLine($"[dim]Base Directory: {baseDirectory}[/]");
            
            var paths = new[]
            {
                Path.Combine(baseDirectory, "assets", "audio", "ok-2.wav"),
                Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "assets", "audio", "ok-2.wav"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", ".store", "hiit-cli", "1.0.0", "hiit-cli", "1.0.0", "tools", "net8.0", "any", "assets", "audio", "ok-2.wav")
            };
            
            string? validPath = null;
            for (int i = 0; i < paths.Length; i++)
            {
                var exists = File.Exists(paths[i]);
                var status = exists ? "[green]EXISTS[/]" : "[red]NOT FOUND[/]";
                AnsiConsole.MarkupLine($"[dim]Path {i + 1}: {status} - {paths[i]}[/]");
                if (exists && validPath == null)
                    validPath = paths[i];
            }
            
            // Additional debugging info
            AnsiConsole.MarkupLine($"[dim]Current OS: {Environment.OSVersion}[/]");
            AnsiConsole.MarkupLine($"[dim]Audio Enabled: {DisplayService.AudioEnabled}[/]");
            AnsiConsole.MarkupLine($"[dim]Is Windows: {OperatingSystem.IsWindows()}[/]");
            
            if (validPath != null)
            {
                AnsiConsole.MarkupLine($"[green]Found valid audio file at: {validPath}[/]");
                
                // Check file properties
                var fileInfo = new FileInfo(validPath);
                AnsiConsole.MarkupLine($"[dim]File size: {fileInfo.Length} bytes[/]");
                AnsiConsole.MarkupLine($"[dim]File last modified: {fileInfo.LastWriteTime}[/]");
                
                AnsiConsole.MarkupLine("[yellow]Testing audio playback...[/]");
                
                // Test the actual playback
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        AnsiConsole.MarkupLine("[blue]Attempting Windows audio playback...[/]");
                        using var player = new System.Media.SoundPlayer(validPath);
                        
                        // First try to load the file
                        player.Load();
                        AnsiConsole.MarkupLine("[green]Audio file loaded successfully![/]");
                        
                        // Now try to play it
                        player.PlaySync(); // Use sync for testing
                        AnsiConsole.MarkupLine("[green]Windows audio playback successful![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[blue]Non-Windows platform - would use system commands[/]");
                        AnsiConsole.MarkupLine("[yellow]Testing system beep fallback...[/]");
                        PlaySystemBeep();
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Audio playback failed: {ex.Message}[/]");
                    AnsiConsole.MarkupLine($"[red]Exception type: {ex.GetType().Name}[/]");
                    AnsiConsole.MarkupLine("[yellow]Falling back to system beep...[/]");
                    PlaySystemBeep();
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No valid audio file found![/]");
                AnsiConsole.MarkupLine("[yellow]Testing system beep fallback...[/]");
                PlaySystemBeep();
            }
        }
    }
}
