#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes Simple Slide Show for all supported platforms and creates release artifacts.

.DESCRIPTION
    This script builds the application for multiple platforms (Windows, Linux, macOS),
    creates ZIP archives, and uses Versionize for version management and changelog generation.

.PARAMETER SkipTag
    Skip creating git tag after version bump (useful for testing)

.PARAMETER PreRelease
    Create a pre-release version (e.g., alpha, beta, rc)

.PARAMETER OutputDir
    Directory where published artifacts will be created. Default: ./publish

.EXAMPLE
    .\publish.ps1
    # Uses Versionize to determine version bump based on conventional commits and publishes

.EXAMPLE
    .\publish.ps1 -SkipTag
    # Bumps version and publishes without creating git tag

.EXAMPLE
    .\publish.ps1 -PreRelease alpha
    # Creates a pre-release version with alpha label
#>

param(
    [switch]$SkipTag,
    [string]$PreRelease,
    [string]$OutputDir = "publish"
)

$ErrorActionPreference = "Stop"

$projectFile = "src/simple-slide-show.csproj"
$projectDir = "src"

# Check if Versionize is installed
$versionizeInstalled = $null -ne (Get-Command versionize -ErrorAction SilentlyContinue)
if (-not $versionizeInstalled) {
    Write-Host "⚠ Versionize not found. Installing globally..." -ForegroundColor Yellow
    dotnet tool install --global Versionize
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install Versionize"
        exit 1
    }
    Write-Host "✓ Versionize installed" -ForegroundColor Green
}

# Run Versionize to bump version and update changelog
Write-Host "`n=============================================" -ForegroundColor Cyan
Write-Host "Running Versionize for Version & Changelog" -ForegroundColor Cyan
Write-Host "=============================================`n" -ForegroundColor Cyan

$versionizeArgs = @()
if ($SkipTag) {
    $versionizeArgs += "--skip-tag"
}
if ($PreRelease) {
    $versionizeArgs += "--pre-release"
    $versionizeArgs += $PreRelease
}

# Dry run first to see what would happen
Write-Host "Analyzing commits..." -ForegroundColor Yellow
$dryRunArgs = $versionizeArgs + @("--dry-run", "--skip-commit")
& versionize @dryRunArgs

if ($LASTEXITCODE -ne 0) {
    Write-Warning "No changes detected or Versionize dry-run failed. Proceeding with current version."
    $skipVersionize = $true
} else {
    $skipVersionize = $false
}

# Actually run versionize if there are changes
if (-not $skipVersionize) {
    Write-Host "`nApplying version bump and updating CHANGELOG.md..." -ForegroundColor Yellow
    & versionize @versionizeArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Versionize failed"
        exit 1
    }
    Write-Host "✓ Version bumped and CHANGELOG.md updated" -ForegroundColor Green
}

# Read current version from .csproj
function Get-CurrentVersion {
    [xml]$csproj = Get-Content $projectFile
    $currentVersion = $csproj.Project.PropertyGroup.Version
    if ([string]::IsNullOrEmpty($currentVersion)) {
        return "1.0.0"
    }
    return $currentVersion
}

# Determine version to use
$currentVersion = Get-CurrentVersion
Write-Host "`nCurrent version: $currentVersion" -ForegroundColor Cyan
$Version = $currentVersion

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

Write-Host "`nPublish process completed successfully!" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Cyan
if (-not $SkipTag) {
    Write-Host "Git tag: v$Version" -ForegroundColor Cyan
}
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  - Review CHANGELOG.md for accuracy" -ForegroundColor Gray
if ($SkipTag) {
    Write-Host "  - Tag was skipped. Run 'git push origin v$Version' manually if needed" -ForegroundColor Gray
} else {
    Write-Host "  - Version tag has been pushed to origin" -ForegroundColor Gray
}

Write-Host "  - Run: git push --follow-tags origin main" -ForegroundColor Green
Write-Host "  - Artifacts are ready in $OutputDir/" -ForegroundColor Gray