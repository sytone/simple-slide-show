namespace SimpleSlideShow.Models;

/// <summary>
/// Tracks metadata and play-state for a single image used by the rotation logic.
/// </summary>
public class ImageInfo
{
    /// <summary>The file name stored on disk.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Absolute path used for streaming the image bytes.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>How many times this image has been displayed.</summary>
    public int DisplayCount { get; set; }

    /// <summary>Timestamp when the image was first detected.</summary>
    public DateTime DateAdded { get; set; }
}
