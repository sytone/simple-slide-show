namespace SimpleSlideShow.Models;

/// <summary>
/// Represents UI-facing state so clients can cache settings and the current library snapshot.
/// </summary>
public class SlideShowState
{
    /// <summary>Indicates whether the client is requesting weighted random playback.</summary>
    public bool IsRandomMode { get; set; }

    /// <summary>Duration each slide is shown, in seconds.</summary>
    public int IntervalSeconds { get; set; } = 3;

    /// <summary>Current catalog of images available to the client.</summary>
    public List<ImageInfo> Images { get; set; } = new();
}
