# HITT CLI Installation Script for Windows
# Run this script in the hitt-cli directory

Write-Host "🏃‍♂️ Installing HITT CLI..." -ForegroundColor Cyan

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8 SDK first." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Build the application
Write-Host "🔨 Building HITT CLI..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restore packages" -ForegroundColor Red
    exit 1
}

dotnet pack --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to build package" -ForegroundColor Red
    exit 1
}

# Uninstall existing version (if any)
Write-Host "🧹 Removing existing installation..." -ForegroundColor Cyan
dotnet tool uninstall --global hitt-cli 2>$null

# Install the new version
Write-Host "📦 Installing HITT CLI..." -ForegroundColor Cyan
dotnet tool install --global --add-source .\hitt-cli\bin\Release hitt-cli
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to install HITT CLI" -ForegroundColor Red
    exit 1
}

# Setup data directory
Write-Host "📁 Setting up workout data..." -ForegroundColor Cyan
$dataDir = "$env:USERPROFILE\Data"
if (!(Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    New-Item -ItemType Directory -Path "$dataDir\Routines" -Force | Out-Null
    New-Item -ItemType Directory -Path "$dataDir\Schedules" -Force | Out-Null
}

# Copy sample data
Copy-Item -Path ".\hitt-cli\Data\*" -Destination $dataDir -Recurse -Force

Write-Host ""
Write-Host "🎉 HITT CLI installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Quick Start:" -ForegroundColor Yellow
Write-Host "  hitt                    - Interactive mode" -ForegroundColor White
Write-Host "  hitt help               - Show all commands" -ForegroundColor White
Write-Host "  hitt list               - List available workouts" -ForegroundColor White
Write-Host "  hitt preview <routine>  - Preview a workout" -ForegroundColor White
Write-Host "  hitt run <routine>      - Run a workout" -ForegroundColor White
Write-Host ""
Write-Host "📁 Workout data location: $dataDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "💪 Ready to get fit! Try: hitt preview day-one-no-rest-blast" -ForegroundColor Green
