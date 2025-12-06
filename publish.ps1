#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes Simple Slide Show for all supported platforms and creates release artifacts.

.DESCRIPTION
    This script builds the application for multiple platforms (Windows, Linux, macOS),
    creates ZIP archives, and optionally bumps the version number in the project file.

.PARAMETER Version
    Optional version number to set (e.g., "1.2.3"). If not specified, uses current version from .csproj

.PARAMETER BumpVersion
    Automatically increment the version. Valid values: major, minor, patch

.PARAMETER OutputDir
    Directory where published artifacts will be created. Default: ./publish

.EXAMPLE
    .\publish.ps1
    # Publishes all platforms with current version

.EXAMPLE
    .\publish.ps1 -Version "1.2.0"
    # Sets version to 1.2.0 and publishes

.EXAMPLE
    .\publish.ps1 -BumpVersion patch
    # Increments patch version (e.g., 1.0.0 -> 1.0.1) and publishes
#>

param(
    [string]$Version,
    [ValidateSet('major', 'minor', 'patch')]
    [string]$BumpVersion,
    [string]$OutputDir = "publish"
)

$ErrorActionPreference = "Stop"

$projectFile = "src/simple-slide-show.csproj"
$projectDir = "src"

# Read current version from .csproj
function Get-CurrentVersion {
    [xml]$csproj = Get-Content $projectFile
    $currentVersion = $csproj.Project.PropertyGroup.Version
    if ([string]::IsNullOrEmpty($currentVersion)) {
        return "1.0.0"
    }
    return $currentVersion
}

# Bump version based on type
function Get-BumpedVersion {
    param(
        [string]$current,
        [string]$bumpType
    )
    
    $parts = $current -split '\.'
    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patch = [int]$parts[2]
    
    switch ($bumpType) {
        'major' { 
            $major++
            $minor = 0
            $patch = 0
        }
        'minor' { 
            $minor++
            $patch = 0
        }
        'patch' { 
            $patch++
        }
    }
    
    return "$major.$minor.$patch"
}

# Update version in .csproj
function Set-ProjectVersion {
    param([string]$newVersion)
    
    [xml]$csproj = Get-Content $projectFile
    $propertyGroup = $csproj.Project.PropertyGroup | Where-Object { $_.Version }
    
    if (-not $propertyGroup) {
        $propertyGroup = $csproj.Project.PropertyGroup[0]
        $versionNode = $csproj.CreateElement("Version")
        $versionNode.InnerText = $newVersion
        $propertyGroup.AppendChild($versionNode) | Out-Null
    } else {
        $propertyGroup.Version = $newVersion
    }
    
    $propertyGroup.AssemblyVersion = "$newVersion.0"
    $propertyGroup.FileVersion = "$newVersion.0"
    
    $csproj.Save((Resolve-Path $projectFile))
    Write-Host "✓ Updated version to $newVersion" -ForegroundColor Green
}

# Determine version to use
$currentVersion = Get-CurrentVersion
Write-Host "Current version: $currentVersion" -ForegroundColor Cyan

if ($BumpVersion) {
    $Version = Get-BumpedVersion -current $currentVersion -bumpType $BumpVersion
    Write-Host "Bumping $BumpVersion version to: $Version" -ForegroundColor Yellow
    Set-ProjectVersion -newVersion $Version
} elseif ($Version) {
    Write-Host "Setting version to: $Version" -ForegroundColor Yellow
    Set-ProjectVersion -newVersion $Version
} else {
    $Version = $currentVersion
    Write-Host "Using current version: $Version" -ForegroundColor Yellow
}

# Clean and create output directory
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Define platforms
$platforms = @(
    @{ RID = "win-x64"; Name = "Windows-x64" },
    @{ RID = "linux-x64"; Name = "Linux-x64" },
    @{ RID = "osx-x64"; Name = "macOS-x64" },
    @{ RID = "osx-arm64"; Name = "macOS-ARM64" }
)

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Publishing Simple Slide Show v$Version" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

foreach ($platform in $platforms) {
    $rid = $platform.RID
    $name = $platform.Name
    
    Write-Host "Building $name..." -ForegroundColor Yellow
    
    $publishPath = "$projectDir/bin/Release/net8.0/$rid/publish"
    
    # Publish
    dotnet publish $projectDir `
        -c Release `
        -r $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:DebugType=None `
        -p:DebugSymbols=false
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to publish $name"
        exit 1
    }
    
    # Create ZIP archive
    $archiveName = "simple-slide-show-v$Version-$name.zip"
    $archivePath = Join-Path $OutputDir $archiveName
    
    Write-Host "Creating archive: $archiveName" -ForegroundColor Cyan
    Compress-Archive -Path "$publishPath/*" -DestinationPath $archivePath -Force
    
    $size = (Get-Item $archivePath).Length / 1MB
    Write-Host "✓ $name completed ($([math]::Round($size, 2)) MB)`n" -ForegroundColor Green
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "All builds completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`nArtifacts saved to: $OutputDir" -ForegroundColor Cyan
Write-Host "Version: $Version`n" -ForegroundColor Cyan

# List all files
Get-ChildItem $OutputDir | ForEach-Object {
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  - $($_.Name) ($sizeMB MB)" -ForegroundColor Gray
}
