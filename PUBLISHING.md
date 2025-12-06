# Publishing and Releases

This document describes how to build and publish Simple Slide Show for multiple platforms.

## Quick Start

### Using PowerShell Script (Recommended)

The `publish.ps1` script uses **Versionize** to automatically:
- Determine version bump based on conventional commits
- Update `CHANGELOG.md` automatically
- Create git tags
- Build for all platforms

```powershell
# Automatic version bump based on commits + publish
.\publish.ps1

# Skip creating git tag (for testing)
.\publish.ps1 -SkipTag

# Create a pre-release version
.\publish.ps1 -PreRelease alpha

# Custom output directory
.\publish.ps1 -OutputDir "releases"
```

The script will:
1. Analyze your commits since last version
2. Determine version bump (major/minor/patch) automatically
3. Update version numbers in `.csproj`
4. Update `CHANGELOG.md` with commit messages
5. Create a git commit and tag (unless `-SkipTag` is used)
6. Build for all platforms (Windows, Linux, macOS x64/ARM64)
7. Create ZIP archives in the `publish/` directory
8. Display file sizes and locations

## Conventional Commits

Versionize uses **Conventional Commits** to determine version bumps:

```bash
# Patch version bump (1.0.0 -> 1.0.1)
fix: correct image loading bug
chore: update dependencies

# Minor version bump (1.0.0 -> 1.1.0)
feat: add new pixelate transition
feat: add port configuration

# Major version bump (1.0.0 -> 2.0.0)
feat!: redesign transition architecture
feat: major API redesign

BREAKING CHANGE: removed old transition API
```

### Commit Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat`: New feature (minor version bump)
- `fix`: Bug fix (patch version bump)
- `chore`: Maintenance, dependencies, etc. (patch version bump)
- `docs`: Documentation only (no version bump)
- `style`: Code style changes (no version bump)
- `refactor`: Code refactoring (no version bump)
- `perf`: Performance improvements (patch version bump)
- `test`: Adding tests (no version bump)

**Breaking Changes:**
Add `!` after type or include `BREAKING CHANGE:` in footer for major version bump

## GitHub Actions Automated Releases

### Automatic Release on Tag Push

The GitHub Action automatically builds and creates releases when you push a version tag.

When you run `publish.ps1`, it automatically creates and pushes the tag for you.

If you need to manually create a release:

```bash
# Versionize handles this automatically, but if needed:
git tag v1.2.0
git push origin v1.2.0
```

The action will:
- Build for all platforms (Windows, Linux, macOS x64, macOS ARM64)
- Create ZIP archives
- Generate SHA256 checksums
- Create a GitHub Release with all artifacts
- Add release notes with download links

### Manual Trigger

You can also trigger the release workflow manually from GitHub:
1. Go to **Actions** tab
2. Select **Build and Release** workflow
3. Click **Run workflow**
4. Choose the branch and run

## Manual Build Commands

### Build for specific platform

```bash
# Windows x64
dotnet publish src -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Linux x64
dotnet publish src -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

# macOS Intel
dotnet publish src -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true

# macOS Apple Silicon
dotnet publish src -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true
```

### Build output locations

Published binaries are located at:
```
src/bin/Release/net8.0/{runtime}/publish/
```

## Version Management

### Version in .csproj

The version is stored in `src/simple-slide-show.csproj`:

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
</PropertyGroup>
```

### Version Bumping

Use the PowerShell script to bump versions automatically:

- **Patch** (1.0.0 → 1.0.1): Bug fixes, minor changes
  ```powershell
  .\publish.ps1 -BumpVersion patch
  ```

- **Minor** (1.0.0 → 1.1.0): New features, backward compatible
  ```powershell
  .\publish.ps1 -BumpVersion minor
  ```

- **Major** (1.0.0 → 2.0.0): Breaking changes
  ```powershell
  .\publish.ps1 -BumpVersion major
  ```

## Release Workflow

### For a new release:

1. **Update code and test**
   ```bash
   dotnet run --project src
   ```

2. **Bump version and build**
   ```powershell
   .\publish.ps1 -BumpVersion patch
   ```

3. **Commit version changes**
   ```bash
   git add src/simple-slide-show.csproj
   git commit -m "Bump version to 1.0.1"
   git push
   ```

4. **Create and push tag**
   ```bash
   git tag v1.0.1
   git push origin v1.0.1
   ```

5. **GitHub Actions automatically creates the release**
   - Builds all platforms
   - Creates ZIP files
   - Publishes to GitHub Releases

## Supported Platforms

- **Windows x64**: Self-contained executable for Windows 10/11
- **Linux x64**: Self-contained binary for most Linux distributions
- **macOS x64**: Self-contained binary for Intel Macs
- **macOS ARM64**: Self-contained binary for Apple Silicon Macs (M1/M2/M3)

## Release Artifacts

Each release includes:
- ZIP archives for each platform
- SHA256 checksum file (`checksums.txt`)
- Automatic release notes with download instructions

## Troubleshooting

### Build fails on GitHub Actions

- Check that secrets are properly configured (GITHUB_TOKEN is automatic)
- Verify `.NET 8.0` SDK is available
- Check workflow logs for specific errors

### PowerShell script execution policy

If you can't run the script:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Version not updating

Ensure you're editing `src/simple-slide-show.csproj` and the XML is well-formed.
