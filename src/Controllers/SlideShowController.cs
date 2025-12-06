using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleSlideShow.Models;
using SimpleSlideShow.Services;
using Serilog;

namespace SimpleSlideShow.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Exposes a thin API for the browser to request slideshow data and image streams without direct filesystem access.
/// </summary>
public class SlideShowController : ControllerBase
{
    private readonly ImageService _imageService;
    private readonly SlideShowService _slideShowService;
    private readonly IOptionsMonitor<SlideShowSettings> _settings;
    private readonly Serilog.ILogger _logger;

    /// <summary>
    /// Initializes the controller with the backing services responsible for image tracking and selection.
    /// </summary>
    /// <param name="imageService">Tracks images on disk and their display counts.</param>
    /// <param name="slideShowService">Selects the next image using sequential or weighted-random logic.</param>
    /// <param name="settings">Provides slideshow configuration from appsettings with live reload.</param>
    public SlideShowController(ImageService imageService, SlideShowService slideShowService, IOptionsMonitor<SlideShowSettings> settings)
    {
        _imageService = imageService;
        _slideShowService = slideShowService;
        _settings = settings;
        _logger = Log.ForContext<SlideShowController>();
    }

    [HttpGet("images")]
    /// <summary>
    /// Returns a snapshot of the known images so the UI can render counts and per-image metadata.
    /// </summary>
    /// <returns>All tracked images with their current display counts and added dates.</returns>
    public ActionResult<List<ImageInfo>> GetAllImages()
    {
        _logger.Information("GetAllImages called");
        var images = _imageService.GetAllImages();
        _logger.Information("Returning {Count} images", images.Count);
        return Ok(images);
    }

    [HttpGet("next")]
    /// <summary>
    /// Fetches the next image according to configured sequential or weighted-random rules.
    /// </summary>
    /// <returns>The next image to display, or 404 if none exist.</returns>
    public ActionResult<ImageInfo> GetNextImage()
    {
        _logger.Information("GetNextImage called with RandomMode={RandomMode}", _settings.CurrentValue.RandomMode);
        var image = _slideShowService.GetNextImage(_settings.CurrentValue.RandomMode);
        if (image == null)
        {
            _logger.Warning("No images available");
            return NotFound();
        }

        _logger.Information("Selected image: {FileName}", image.FileName);
        return Ok(image);
    }

    /// <summary>
    /// Returns slideshow configuration so the client can honor server-defined behavior.
    /// </summary>
    /// <returns>The current slideshow settings.</returns>
    [HttpGet("config")]
    public ActionResult<SlideShowSettings> GetConfig()
    {
        _logger.Information("GetConfig called");
        var config = _settings.CurrentValue;
        _logger.Information("Config: RandomMode={RandomMode}, IntervalSeconds={IntervalSeconds}, Transitions={TransitionCount}",
            config.RandomMode, config.IntervalSeconds, config.Transitions?.Count ?? 0);
        _logger.Information("Full config: {@Config}", config);
        return Ok(config);
    }

    [HttpGet("image/{fileName}")]
    /// <summary>
    /// Streams a specific image from disk so the browser can render it without file path exposure.
    /// </summary>
    /// <param name="fileName">The image file name to stream.</param>
    /// <returns>The image bytes as JPEG, or 404 if the file is missing.</returns>
    public ActionResult GetImage(string fileName)
    {
        _logger.Information("GetImage called for {FileName}", fileName);
        var imagePath = _imageService.GetImagePath(fileName);
        if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
        {
            _logger.Warning("Image not found: {FileName}", fileName);
            return NotFound();
        }

        _logger.Debug("Streaming image from {Path}", imagePath);
        var stream = System.IO.File.OpenRead(imagePath);
        return File(stream, "image/jpeg");
    }

    [HttpPost("stats")]
    /// <summary>
    /// Records that an image was displayed so weighting remains accurate.
    /// </summary>
    /// <param name="fileName">The image that was shown.</param>
    /// <returns>200 on success.</returns>
    public ActionResult LogImageDisplay([FromBody] string fileName)
    {
        _logger.Information("LogImageDisplay called for {FileName}", fileName);
        _imageService.IncrementDisplayCount(fileName);
        _logger.Debug("Display count incremented for {FileName}", fileName);
        return Ok();
    }
}
