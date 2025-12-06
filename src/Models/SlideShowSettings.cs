using System.Text.Json;

namespace SimpleSlideShow.Models;

/// <summary>
/// Configuration for slideshow behavior provided via appsettings.
/// </summary>
public class SlideShowSettings
{
    /// <summary>When true, images are selected using weighted randomness; otherwise sequential fairness.</summary>
    public bool RandomMode { get; set; } = true;

    /// <summary>Interval between slide changes in seconds.</summary>
    public int IntervalSeconds { get; set; } = 3;

    /// <summary>When true, shows the control panel at the top of the page.</summary>
    public bool ShowControls { get; set; } = true;

    /// <summary>Interval in seconds for refreshing the image list and configuration from the server.</summary>
    public int RefreshIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// List of transition configurations. Each can be a string (transition name only)
    /// or an object with Name and transition-specific settings.
    /// </summary>
    public List<Dictionary<string, string>> Transitions { get; set; } = new();
}
