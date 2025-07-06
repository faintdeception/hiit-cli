#!/bin/bash
# HITT CLI Installation Script for Linux/macOS
# Run this script in the hitt-cli directory

echo "🏃‍♂️ Installing HITT CLI..."

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8 SDK first."
    echo "Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

dotnet_version=$(dotnet --version)
echo "✅ .NET SDK found: $dotnet_version"

# Build the application
echo "🔨 Building HITT CLI..."
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
dotnet tool uninstall --global hitt-cli 2>/dev/null

# Install the new version
echo "📦 Installing HITT CLI..."
dotnet tool install --global --add-source ./hitt-cli/bin/Release hitt-cli
if [ $? -ne 0 ]; then
    echo "❌ Failed to install HITT CLI"
    exit 1
fi

# Setup data directory
echo "📁 Setting up workout data..."
data_dir="$HOME/Data"
mkdir -p "$data_dir/Routines"
mkdir -p "$data_dir/Schedules"

# Copy sample data
cp -r ./hitt-cli/Data/* "$data_dir/"

echo ""
echo "🎉 HITT CLI installed successfully!"
echo ""
echo "📋 Quick Start:"
echo "  hitt                    - Interactive mode"
echo "  hitt help               - Show all commands"
echo "  hitt list               - List available workouts"
echo "  hitt preview <routine>  - Preview a workout"
echo "  hitt run <routine>      - Run a workout"
echo ""
echo "📁 Workout data location: $data_dir"
echo ""
echo "💪 Ready to get fit! Try: hitt preview day-one-no-rest-blast"
