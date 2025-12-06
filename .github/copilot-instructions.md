# Simple Slide Show – Copilot Instructions

## Purpose
- ASP.NET Core 8 app that serves a slideshow of JPG images from a monitored folder. New images are auto-detected and prioritized in weighted random playback.

## Architecture
- `Program.cs` wires DI, static files, controllers, and starts `ImageService.StartMonitoring()` after the container is built.
- `Services/ImageService` uses `FileSystemWatcher` to keep an in-memory catalog (`List<ImageInfo>`) synced with the configured folder (default `%USERPROFILE%\Pictures\SlideShow`). Thread-safe via a lock; exposes image list, path lookup, and display count incrementing.
- `Services/SlideShowService` selects the next image: sequential (least shown, then oldest) or weighted random (under-average and fresh images favored; roulette-wheel selection).
- `Controllers/SlideShowController` exposes API endpoints the frontend calls: list images, next image, stream image by file name, record display.
- `wwwroot/index.html` is the SPA: fetches API data, toggles random mode, interval input, and periodically refreshes image list.

## Key behaviors and data
- Only `.jpg` files are considered; display counts are kept in memory (no persistence on restart).
- New images get a 1.5x weight for the first day; under-served images boost weight proportionally to the deficit vs average display count.
- Image folder path is configurable via `appsettings.json` (`ImageFolder`); folder is created if missing.

## Workflows
- Build: `dotnet build src` (task `build` in `.vscode/tasks.json`).
- Run: `dotnet run --project src` (task `run`) or `dotnet watch run --project src` (task `watch`).
- Debug: `.vscode/launch.json` uses preLaunchTask `build` and launches `bin/Debug/net8.0/simple-slide-show.dll`.
- Frontend served from `wwwroot`; fallback to `index.html` handles routing.

## Conventions
- Use C# XML documentation comments (`///`) for classes and methods to describe purpose/behavior; avoid inline `//` for this purpose.
- Keep services registered as singletons so folder monitoring state and display counts stay consistent across requests.
- Keep random-selection logic in `SlideShowService`; filesystem access and counts in `ImageService`; controllers stay thin.

## Files to know
- `src/Program.cs` – hosting, DI, start watcher.
- `src/Services/ImageService.cs` – folder sync, counts, path lookup.
- `src/Services/SlideShowService.cs` – selection algorithms.
- `src/Controllers/SlideShowController.cs` – API surface.
- `src/Models/ImageInfo.cs` / `SlideShowState.cs` – metadata and UI state.
- `wwwroot/index.html` – client UI and API calls.

## When changing behaviors
- Update weighting rules only in `SlideShowService`; keep count mutations through `ImageService.IncrementDisplayCount`.
- If adding formats or persistence, centralize file filtering and load logic inside `ImageService`.
- Keep API contract stable for the SPA: `/api/slideshow/images`, `/api/slideshow/next?randomMode=`, `/api/slideshow/image/{fileName}`, `/api/slideshow/stats`.
