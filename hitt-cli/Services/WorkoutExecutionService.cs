using hitt_cli.Models;
using Spectre.Console;

namespace hitt_cli.Services
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
                    await ExecuteWorkoutAsync(workout, workoutIndex, totalWorkouts, currentRep, routine.Reps, cancellationToken);
                    
                    // Add a brief pause between workouts (except for the last workout in the last rep)
                    if (workoutIndex < totalWorkouts || currentRep < routine.Reps)
                    {
                        AnsiConsole.WriteLine();
                        if (routine.Workouts.All(w => w.Rest == 0)) // No-rest routine
                        {
                            AnsiConsole.MarkupLine($"[dim]Moving to next exercise...[/]");
                            await Task.Delay(1000, cancellationToken);
                        }
                        else
                        {
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
        private async Task ExecuteWorkoutAsync(Workout workout, int workoutNumber, int totalWorkouts, int currentRep, int totalReps, CancellationToken cancellationToken)
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
                var activityName = isNoRestWorkout ? $"GO! {workout.Name}" : $"{workout.Name}";
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
                if (isNoRestWorkout && (workoutNumber < totalWorkouts || currentRep < totalReps))
                {
                    AnsiConsole.MarkupLine($"[bold cyan]{DisplayService.Fire} Exercise {workoutNumber} crushed! Moving to next...[/]");
                    await Task.Delay(1500, cancellationToken);
                }
                else if (workoutNumber < totalWorkouts || currentRep < totalReps)
                {
                    AnsiConsole.MarkupLine($"[bold cyan]✅ Exercise {workoutNumber} complete! Next exercise coming up...[/]");
                }
            }
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
                            // Add audio-like countdown for final seconds
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
                        }
                        else
                        {
                            task.Description = $"[{color}]{activity}[/] - [green]Complete![/]";
                        }
                        
                        // Brief pause to show completion
                        await Task.Delay(500, cancellationToken);
                    }
                });
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
    }
}
