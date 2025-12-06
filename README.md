# Simple Slide Show

A lightweight ASP.NET Core web application that displays JPG images from a monitored folder as an interactive slideshow.

## Features

- **Folder Monitoring**: Automatically detects and loads new JPG images added to the designated folder
- **Sequential Mode**: Displays images in rotation, prioritizing images shown less frequently
- **Random Mode**: Displays images in random order with intelligent weighting
- **Smart Weighting**: Images shown less than the average are prioritized for display
- **New Image Priority**: Newly added images get weighted to appear sooner in random mode
- **Display Statistics**: Tracks how many times each image has been shown
- **Configurable Transitions**: Extensible transition system with support for custom animations
- **Live Configuration**: Changes to `appsettings.json` take effect without restarting
- **Fullscreen Mode**: Optional control panel can be hidden for clean slideshow display
- **Minimal Dependencies**: Uses only ASP.NET Core with no external UI frameworks
- **Responsive Interface**: Clean, dark-themed web interface

## Configuration

The application is configured via `appsettings.json`. All settings support live reload—changes take effect without restarting the app.

### Image Folder

```json
"ImageFolder": "C:\\Path\\To\\Your\\Images"
```

Absolute path to the folder containing JPG images. The folder is created automatically if it doesn't exist.

### Urls

```json
"Urls": "http://localhost:5000"
```

The URL(s) the application listens on. Can specify multiple URLs separated by semicolons:

```json
"Urls": "http://localhost:5000;https://localhost:5001"
```

Change the port to avoid conflicts with other applications:

```json
"Urls": "http://localhost:8080"
```

### SlideShowSettings

```json
"SlideShowSettings": {
  "RandomMode": true,
  "IntervalSeconds": 8,
  "ShowControls": false,
  "RefreshIntervalSeconds": 30,
  "Transitions": [
    {
      "Name": "fade",
      "Duration": 0.6
    }
  ]
}
```

#### Settings Reference

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `RandomMode` | boolean | `true` | When `true`, uses weighted random selection. When `false`, uses sequential rotation prioritizing least-shown images. |
| `IntervalSeconds` | number | `8` | How long each image is displayed (in seconds) before transitioning to the next. |
| `ShowControls` | boolean | `true` | When `true`, shows the control panel at the top with stats. When `false`, hides controls for fullscreen slideshow mode. |
| `RefreshIntervalSeconds` | number | `5` | How often (in seconds) the client checks for new images and updated configuration from the server. |
| `Transitions` | array | `[]` | List of transition effects to use. Can be strings (transition name only) or objects with transition-specific configuration. |

### Transitions Configuration

Transitions control how images fade, slide, or animate when changing. You can configure multiple transitions and the slideshow will randomly select from them.

#### Simple Format (Name Only)

```json
"Transitions": ["fade"]
```

Uses default settings for the transition.

#### Object Format (With Configuration)

```json
"Transitions": [
  {
    "Name": "fade",
    "Duration": 1.2
  }
]
```

Allows customizing transition-specific settings.

#### Multiple Transitions

```json
"Transitions": [
  "fade",
  {
    "Name": "fade",
    "Duration": 0.3
  }
]
```

The slideshow randomly picks one transition for each image change.

## Available Transitions

### Fade

Cross-fades between current and next image with adjustable duration.

**Configuration Options:**
- `Duration` (number, default: `0.6`): Fade duration in seconds

**Examples:**

```json
// Quick fade
{ "Name": "fade", "Duration": 0.3 }

// Slow fade
{ "Name": "fade", "Duration": 2.0 }

// Default fade
"fade"
```

### Pixelate

Creates a pixelated/blur effect that transitions between images by blurring out the current image and then blurring in the next image.

**Configuration Options:**
- `Duration` (number, default: `1.0`): Total transition duration in seconds
- `MaxPixelSize` (number, default: `20`): Maximum blur amount at the peak of the transition

**Examples:**

```json
// Subtle pixelate
{ "Name": "pixelate", "Duration": 0.8, "MaxPixelSize": 10 }

// Heavy pixelate
{ "Name": "pixelate", "Duration": 1.5, "MaxPixelSize": 40 }

// Default pixelate
"pixelate"
```

**How it works:**
- First half: Current image blurs out and fades to opacity 0
- Second half: Next image blurs in from maximum blur to sharp and fades to opacity 1

## Creating Custom Transitions

Transitions are defined in `wwwroot/transitions.js`. To add a new transition:

```javascript
Transitions.register('myTransition', function(currentImg, nextImg, config, onComplete) {
    // currentImg: the currently visible image element
    // nextImg: the next image element (already loaded, currently hidden)
    // config: object with your custom settings (e.g., { Duration: 1.0, Direction: "left" })
    // onComplete: callback to invoke when animation completes
    
    const duration = config.Duration || config.duration || 1.0;
    
    // Set up your CSS transitions/animations
    currentImg.style.transition = `opacity ${duration}s`;
    nextImg.style.transition = `opacity ${duration}s`;
    
    // Trigger animation
    requestAnimationFrame(() => {
        currentImg.style.opacity = '0';
        nextImg.style.opacity = '1';
    });
    
    // Call onComplete when done
    if (onComplete) {
        setTimeout(onComplete, duration * 1000);
    }
});
```

Then configure it in `appsettings.json`:

```json
"Transitions": [
  {
    "Name": "myTransition",
    "Duration": 1.5
  }
]
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK (for development)
- Or download pre-built binaries from [Releases](https://github.com/sytone/simple-slide-show/releases)

### Installation from Release

1. Download the ZIP for your platform from the latest release
2. Extract the archive
3. Edit `appsettings.json` to configure your image folder and settings
4. Run the executable:
   - Windows: `simple-slide-show.exe`
   - Linux/macOS: `./simple-slide-show` (may need `chmod +x simple-slide-show`)
5. Open browser to configured URL (default: http://localhost:5000)

### Running from Source

```bash
dotnet run --project src
```

The application will start at the configured URL (default: `http://localhost:5000`).

### Adding Images

Add JPG images to your configured folder. The application will automatically detect and include them in the slideshow rotation.

## Usage

- The app honors settings from `appsettings.json` (random vs sequential, interval seconds)
- The UI displays the active mode/interval along with total image and current count stats

## Display Logic

### Sequential Mode
- Images are displayed in order of least frequently shown
- Ensures even distribution of display time

### Random Mode
- Images are selected randomly with weighted probability
- Images shown less than average get higher priority
- Newly added images get 50% higher probability for first day
- Ensures all images get fair rotation while maintaining randomness

## Development

The project uses:
- ASP.NET Core 8.0
- Built-in dependency injection
- FileSystemWatcher for folder monitoring
- Minimal frontend (vanilla HTML/CSS/JavaScript)

### Project Structure

```
simple-slide-show/
├── Controllers/      # API endpoints
├── Models/          # Data models
├── Services/        # Business logic (ImageService, SlideShowService)
├── Properties/      # Launch settings
├── wwwroot/         # Static files
│   ├── index.html   # Main slideshow UI
│   └── transitions.js # Transition registry and effects
├── Program.cs       # Application startup
└── appsettings.json # Configuration
```

### API Endpoints

- `GET /api/slideshow/images` - Get all image metadata
- `GET /api/slideshow/next` - Get next image for display (uses configured RandomMode)
- `GET /api/slideshow/config` - Get current slideshow configuration
- `GET /api/slideshow/image/{fileName}` - Stream image file as JPEG
- `POST /api/slideshow/stats` - Log image display (increments display count)
