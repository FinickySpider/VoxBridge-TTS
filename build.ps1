#!/usr/bin/env pwsh
<#
.SYNOPSIS
Build script for TTS Communication Tool with automatic version bumping.

.DESCRIPTION
This script:
1. Kills any running TtsCommunicationTool.App.exe processes
2. Bumps the version in .csproj (optional)
3. Updates CHANGELOG.md with new version header (if bumped)
4. Runs dotnet build

.PARAMETER VersionBump
The type of version bump to apply.
- 'none': No version change; builds with current version
- 'patch': Bump PATCH (0.9.2 to 0.9.3)
- 'minor': Bump MINOR (0.9.2 to 0.10.0, PATCH resets to 0)
- 'major': Bump MAJOR (0.9.2 to 1.0.0, MINOR/PATCH reset to 0)

.PARAMETER Configuration
Build configuration: 'Debug' (default, builds into the Debug output folder) or 'Release'

.EXAMPLE
.\build.ps1 -VersionBump patch -Configuration Release
Bumps patch version and builds Release configuration

.EXAMPLE
.\build.ps1
Builds Debug configuration with no version change and writes the EXE to
src\TtsCommunicationTool.App\bin\Debug\net10.0-windows\

.EXAMPLE
.\build.ps1 -VersionBump minor
Bumps minor version and builds Debug configuration

.EXAMPLE
.\build.ps1 -Configuration Release
Builds Release configuration and writes the EXE to
src\TtsCommunicationTool.App\bin\Release\net10.0-windows\
#>

param(
    [ValidateSet('none', 'patch', 'minor', 'major')]
    [string]$VersionBump = 'none',
    
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

# Kill running process
Write-Host "Killing running TtsCommunicationTool.App.exe instances..." -ForegroundColor Cyan
try {
    $runningProcess = Get-Process -Name "TtsCommunicationTool.App" -ErrorAction SilentlyContinue
    if ($runningProcess) {
        Stop-Process -InputObject $runningProcess -Force -ErrorAction Stop
        Write-Host "[OK] Process terminated" -ForegroundColor Green
        Start-Sleep -Milliseconds 500
    }
    else {
        Write-Host "[INFO] No running process found" -ForegroundColor Gray
    }
}
catch {
    Write-Host "[WARN] Could not kill process (may not be running): $_" -ForegroundColor Yellow
}

# Version bumping (if requested)
$projectFile = "e:\Projects\tts\src\TtsCommunicationTool.App\TtsCommunicationTool.App.csproj"
$changelogFile = "e:\Projects\tts\CHANGELOG.md"
$versionBumped = $false
$oldVersion = $null
$newVersion = $null

if ($VersionBump -ne 'none') {
    Write-Host "`nVersion Bumping: $VersionBump" -ForegroundColor Cyan
    
    # Read current version from .csproj
    $csprojContent = Get-Content -Path $projectFile -Raw
    $versionMatch = [regex]::Match($csprojContent, '<Version>([0-9]+\.[0-9]+\.[0-9]+)</Version>')
    
    if (-not $versionMatch.Success) {
        Write-Host "[ERROR] Could not find Version tag in csproj" -ForegroundColor Red
        exit 1
    }
    
    $oldVersion = $versionMatch.Groups[1].Value
    Write-Host "Current version: $oldVersion" -ForegroundColor Gray
    
    # Parse version components
    $versionParts = $oldVersion -split '\.'
    [int]$major = $versionParts[0]
    [int]$minor = $versionParts[1]
    [int]$patch = $versionParts[2]
    
    # Bump version
    switch ($VersionBump) {
        'patch' {
            $patch++
        }
        'minor' {
            $minor++
            $patch = 0
        }
        'major' {
            $major++
            $minor = 0
            $patch = 0
        }
    }
    
    $newVersion = "$major.$minor.$patch"
    Write-Host "New version: $newVersion" -ForegroundColor Green
    
    # Update .csproj (3 version fields)
    $csprojContent = $csprojContent -replace 
        '<Version>([0-9]+\.[0-9]+\.[0-9]+)</Version>', 
        "<Version>$newVersion</Version>"
    
    $csprojContent = $csprojContent -replace 
        '<AssemblyVersion>([0-9]+\.[0-9]+\.[0-9]+)\.[0-9]+</AssemblyVersion>', 
        "<AssemblyVersion>$newVersion.0</AssemblyVersion>"
    
    $csprojContent = $csprojContent -replace 
        '<FileVersion>([0-9]+\.[0-9]+\.[0-9]+)\.[0-9]+</FileVersion>', 
        "<FileVersion>$newVersion.0</FileVersion>"
    
    Set-Content -Path $projectFile -Value $csprojContent -NoNewline
    Write-Host "[OK] Updated .csproj" -ForegroundColor Green
    
    # Update CHANGELOG.md with new version header
    $today = Get-Date -Format 'yyyy-MM-dd'
    $changelogHeader = @"
## [v$newVersion] -- $today

### Features

- (add features here)

### Bug Fixes

- (add bug fixes here)

### Improvements

- (add improvements here)

---

"@
    
    $changelogContent = Get-Content -Path $changelogFile -Raw
    # Insert new version section after "# Changelog ... ---" block
    $changelogContent = $changelogContent -replace 
        '(# Changelog\s+All notable.*?\s+---\s+)', 
        "`$1`n$changelogHeader"
    
    Set-Content -Path $changelogFile -Value $changelogContent -NoNewline
    Write-Host "[OK] Updated CHANGELOG.md with v$newVersion header" -ForegroundColor Green
    
    $versionBumped = $true
}
else {
    Write-Host "`nSkipping version bump (VersionBump = 'none')" -ForegroundColor Gray
}

# Build project
Write-Host "`nBuilding project ($Configuration)..." -ForegroundColor Cyan
$buildCmd = @(
    "e:\Projects\tts\TtsCommunicationTool.slnx",
    "-c", $Configuration
)

try {
    & "C:\Program Files\dotnet\dotnet.exe" build @buildCmd
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "[ERROR] Build failed: $_" -ForegroundColor Red
    exit 1
}

# Success summary
Write-Host "`n" -ForegroundColor Gray
Write-Host "==================================================================" -ForegroundColor Green
Write-Host "BUILD SUCCESSFUL" -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Green

if ($versionBumped) {
    Write-Host "Version bumped: $oldVersion -> $newVersion" -ForegroundColor Cyan
    Write-Host "CHANGELOG.md updated with new version header" -ForegroundColor Cyan
    Write-Host "NEXT: Edit CHANGELOG.md to describe your changes" -ForegroundColor Yellow
}
else {
    Write-Host "Version unchanged: (VersionBump = 'none')" -ForegroundColor Gray
}

Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
if ($Configuration -eq 'Debug') {
    Write-Host "Executable: src\TtsCommunicationTool.App\bin\Debug\net10.0-windows\TtsCommunicationTool.App.exe" -ForegroundColor Cyan
    Write-Host "Note: running .\build.ps1 with no -Configuration argument builds to the Debug folder." -ForegroundColor Gray
}
else {
    Write-Host "Executable: src\TtsCommunicationTool.App\bin\Release\net10.0-windows\TtsCommunicationTool.App.exe" -ForegroundColor Cyan
}
Write-Host "==================================================================" -ForegroundColor Green
Write-Host ""
