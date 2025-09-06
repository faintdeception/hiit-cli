# HIIT CLI Installation Script for Windows
# Run this script in the hiit-cli directory

Write-Host "🏃‍♂️ Installing HIIT CLI..." -ForegroundColor Cyan

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
Write-Host "🔨 Building HIIT CLI..." -ForegroundColor Cyan
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
dotnet tool uninstall --global hiit-cli 2>$null

# Install the new version
Write-Host "📦 Installing HIIT CLI..." -ForegroundColor Cyan
dotnet tool install --global --add-source .\hiit-cli\bin\Release hiit-cli
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to install HIIT CLI" -ForegroundColor Red
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
Copy-Item -Path ".\hiit-cli\Data\*" -Destination $dataDir -Recurse -Force

Write-Host ""
Write-Host "🎉 HIIT CLI installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Quick Start:" -ForegroundColor Yellow
Write-Host "  hiit                    - Interactive mode" -ForegroundColor White
Write-Host "  hiit help               - Show all commands" -ForegroundColor White
Write-Host "  hiit list               - List available workouts" -ForegroundColor White
Write-Host "  hiit preview <routine>  - Preview a workout" -ForegroundColor White
Write-Host "  hiit run <routine>      - Run a workout" -ForegroundColor White
Write-Host ""
Write-Host "📁 Workout data location: $dataDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "💪 Ready to get fit! Try: hiit preview day-one-no-rest-blast" -ForegroundColor Green
