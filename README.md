# 🏃🏾 HIIT CLI - High Intensity Interval Training

A powerful command-line tool for guided HIIT workouts with interactive timers, progress tracking, and customizable routines.

![HIIT CLI Demo](https://img.shields.io/badge/Platform-.NET%208-512BD4) ![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen) ![License](https://img.shields.io/badge/License-MIT-blue)

## ✨ Features

- 🕐 **Interactive Timers** - Live countdown with progress bars
- 📅 **Smart Scheduling** - Automatic workout selection based on day/time
- 🔥 **No-Rest Workouts** - Optimized for continuous high-intensity training
- 📊 **Progress Tracking** - Visual progress through exercises and reps
- 🎯 **Multi-Rep Routines** - Complete workout rounds with rest between reps
- 📱 **Rich Terminal UI** - Beautiful console interface with Spectre Console
- 📁 **JSON-Based** - Easy-to-edit workout and schedule files
- 🚀 **Global CLI Tool** - Run from anywhere with `hiit` command

## 🚀 Quick Start

### Installation

**Option 1: Run installer script (Windows)**
```powershell
.\install.ps1
```

**Option 2: Run installer script (Linux/macOS)**
```bash
chmod +x install.sh
./install.sh
```

**Option 3: Manual installation**
```bash
dotnet pack --configuration Release
dotnet tool install --global --add-source ./hiit-cli/bin/Release hiit-cli
```

### First Run

```bash
# Start interactive mode
hiit

# List available workouts
hiit list

# Preview a workout
hiit preview day-one-no-rest-blast

# Run a specific workout
hiit run morning-hiit-blast

# Check today's schedule
hiit today

# Start current scheduled workout
hiit now
```

## 📋 Commands

| Command | Description |
|---------|-------------|
| `hiit` | Interactive mode with menu |
| `hiit now` | Start current scheduled workout |
| `hiit today` | View today's workout schedule |
| `hiit next` | Show next scheduled workout |
| `hiit list` | List available schedules and routines |
| `hiit run <routine>` | Run a specific routine |
| `hiit preview <routine>` | Preview a routine without running |
| `hiit help` | Show help information |

## 📁 Workout Structure

### Routine Example
```json
{
  "name": "Day One No Rest Blast",
  "description": "High-intensity workout with no rest between exercises",
  "difficulty": 4,
  "reps": 3,
  "tags": ["strength", "cardio", "no-rest"],
  "workouts": [
    {
      "name": "High knees",
      "sets": 1,
      "length": 30,
      "rest": 0,
      "description": "Stand in place and run, bringing knees up to waist level"
    }
  ]
}
```

### Schedule Example
```json
{
  "name": "Weekly HIIT Schedule",
  "description": "Balanced weekly workout schedule",
  "entries": [
    {
      "routineName": "day-one-no-rest-blast.json",
      "daysOfWeek": [1],
      "startTime": "06:00:00",
      "endTime": "08:00:00",
      "note": "Start the week strong!"
    }
  ]
}
```

## 🎯 Workout Types

### Classic Workouts
- Multiple sets per exercise
- Rest periods between sets
- Traditional HIIT structure

### No-Rest Workouts  
- Continuous movement
- No breaks between exercises
- Maximum intensity training

### Multi-Rep Routines
- Complete workout rounds
- Rest periods between reps
- Circuit-style training

## 📊 Example Workouts Included

- **Morning HIIT Blast** - 9m 20s energizing workout
- **Evening Power Routine** - 10m 25s strength-focused session  
- **Day One No Rest Blast** - 15m no-rest intensity challenge

## 🔧 Customization

### Adding New Workouts

1. Create JSON file in `Data/Routines/`
2. Follow the routine structure
3. Run `- Use `hiit list` to verify` to verify

### Creating Schedules

1. Create JSON file in `Data/Schedules/`
2. Define time slots and routine mappings
3. Use day numbers (0=Sunday, 1=Monday, etc.)

### Directory Structure
```
Data/
├── Routines/
│   ├── morning-hiit-blast.json
│   ├── evening-power-routine.json
│   └── day-one-no-rest-blast.json
└── Schedules/
    └── weekly-schedule.json
```

## 🎮 Interactive Experience

The app provides a complete guided workout experience:

1. **Workout Header** - Shows routine info and total time
2. **Exercise-by-Exercise** - Individual exercise guidance
3. **Live Timers** - Countdown with progress bars
4. **Set Tracking** - Visual progress through sets
5. **Rep Management** - Multi-round workout support
6. **Completion Celebration** - Motivational finish messages

## 💡 Pro Tips

- Use `hiit preview` before running new workouts
- Customize rest periods for your fitness level
- Create multiple schedules for different goals
- Use tags to categorize workouts
- The `reps` field creates workout rounds
- Set `rest: 0` for no-rest challenges

## 🔄 Updates

The app automatically picks up new JSON files - no reinstallation needed!

## 📄 License

MIT License - Feel free to modify and distribute

## 💪🏾 Get Started

Ready to transform your fitness routine? 

```bash
hiit preview day-one-no-rest-blast
```

Then when you're ready:

```bash
hiit run day-one-no-rest-blast
```

**Let's get fit! 🔥**
