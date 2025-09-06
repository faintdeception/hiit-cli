#!/bin/bash
# HIIT CLI Installation Script for Linux/macOS
# Run this script in the hiit-cli directory

echo "🏃‍♂️ Installing HIIT CLI..."

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8 SDK first."
    echo "Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

dotnet_version=$(dotnet --version)
echo "✅ .NET SDK found: $dotnet_version"

# Build the application
echo "🔨 Building HIIT CLI..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Failed to restore packages"
    exit 1
fi

dotnet pack --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ Failed to build package"
    exit 1
fi

# Uninstall existing version (if any)
echo "🧹 Removing existing installation..."
dotnet tool uninstall --global hiit-cli 2>/dev/null

# Install the new version
echo "📦 Installing HIIT CLI..."
dotnet tool install --global --add-source ./hiit-cli/bin/Release hiit-cli
if [ $? -ne 0 ]; then
    echo "❌ Failed to install HIIT CLI"
    exit 1
fi

# Setup data directory
echo "📁 Setting up workout data..."
data_dir="$HOME/Data"
mkdir -p "$data_dir/Routines"
mkdir -p "$data_dir/Schedules"

# Copy sample data
cp -r ./hiit-cli/Data/* "$data_dir/"

echo ""
echo "🎉 HIIT CLI installed successfully!"
echo ""
echo "📋 Quick Start:"
echo "  hiit                    - Interactive mode"
echo "  hiit help               - Show all commands"
echo "  hiit list               - List available workouts"
echo "  hiit preview <routine>  - Preview a workout"
echo "  hiit run <routine>      - Run a workout"
echo ""
echo "📁 Workout data location: $data_dir"
echo ""
echo "💪 Ready to get fit! Try: hiit preview day-one-no-rest-blast"
