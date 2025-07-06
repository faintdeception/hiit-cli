# HITT CLI - Distribution Guide

## 🚀 Distribution Options

### **Option 1: Source Code Distribution (Simplest)**
**Best for: Developers or tech-savvy users**

1. **Share the repository:**
   ```bash
   git clone <your-repo-url>
   cd hitt-cli/hitt-cli
   dotnet pack
   dotnet tool install --global --add-source ./bin/Release hitt-cli
   ```

2. **Include setup script (Windows):**
   ```powershell
   # install-hitt.ps1
   dotnet restore
   dotnet pack --configuration Release
   dotnet tool install --global --add-source ./bin/Release hitt-cli
   Write-Host "HITT CLI installed! Run 'hitt' to get started."
   ```

### **Option 2: Pre-built Package Distribution**
**Best for: Easy sharing without requiring build tools**

1. **Create release package:**
   ```bash
   dotnet pack --configuration Release
   ```

2. **Share the .nupkg file:**
   - Copy `bin/Release/hitt-cli.1.0.0.nupkg`
   - User installs with: `dotnet tool install --global --add-source <folder> hitt-cli`

### **Option 3: NuGet.org Publishing (Most Professional)**
**Best for: Public distribution**

1. **Get NuGet API key from nuget.org**
2. **Publish package:**
   ```bash
   dotnet nuget push bin/Release/hitt-cli.1.0.0.nupkg --api-key <your-key> --source https://api.nuget.org/v3/index.json
   ```
3. **Users install with:**
   ```bash
   dotnet tool install --global hitt-cli
   ```

### **Option 4: GitHub Releases (Recommended)**
**Best for: Professional open-source distribution**

1. **Tag a release in GitHub**
2. **Attach the .nupkg file to the release**
3. **Include installation instructions**

### **Option 5: Self-Contained Executables**
**Best for: Users without .NET runtime**

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS 
dotnet publish -c Release -r osx-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

## 📋 **Recommended Distribution Strategy**

### **For Personal/Team Use:**
1. **Create installer script**
2. **Share via Git repository**
3. **Include sample workout files**

### **For Public Distribution:**
1. **Publish to NuGet.org**
2. **Create GitHub releases**
3. **Provide comprehensive README**

## 📁 **What to Include in Distribution**

### **Essential Files:**
- `hitt-cli.exe` or source code
- `Data/` folder with sample workouts
- Installation instructions
- README with usage examples

### **Sample Distribution Structure:**
```
hitt-cli-release/
├── hitt-cli.1.0.0.nupkg
├── install.ps1 (Windows)
├── install.sh (Linux/Mac)
├── README.md
├── USAGE.md
└── sample-data/
    ├── Routines/
    │   ├── morning-hiit-blast.json
    │   ├── evening-power-routine.json
    │   └── day-one-no-rest-blast.json
    └── Schedules/
        └── weekly-schedule.json
```

## 🎯 **Installation Instructions for Users**

### **With .NET SDK installed:**
```bash
# From NuGet (if published)
dotnet tool install --global hitt-cli

# From local package
dotnet tool install --global --add-source <folder> hitt-cli
```

### **From source:**
```bash
git clone <repo-url>
cd hitt-cli/hitt-cli
dotnet pack
dotnet tool install --global --add-source ./bin/Release hitt-cli
```

### **Setup data directory:**
```bash
mkdir ~/Data
cp -r sample-data/* ~/Data/
hitt list  # Verify installation
```

## 💡 **Pro Tips**

1. **Version your releases** - Update version in .csproj
2. **Include changelog** - Document new features
3. **Test installation** - Verify on clean machine
4. **Provide examples** - Include sample workouts
5. **Cross-platform testing** - Test on Windows/Mac/Linux

## 🔧 **Advanced: Automated Release Pipeline**

Consider setting up GitHub Actions for automatic package building and publishing:

```yaml
# .github/workflows/release.yml
name: Release
on:
  push:
    tags: ['v*']
jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    - name: Pack
      run: dotnet pack --configuration Release
    - name: Publish to NuGet
      run: dotnet nuget push **/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```
