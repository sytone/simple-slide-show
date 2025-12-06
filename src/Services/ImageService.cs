using Microsoft.Extensions.Logging;
using SimpleSlideShow.Models;

namespace SimpleSlideShow.Services;

/// <summary>
/// Maintains the in-memory view of slideshow images and keeps it synchronized with a watched folder on disk.
/// </summary>
public class ImageService
{
    private readonly string _watchFolder;
    private readonly ILogger<ImageService> _logger;
    private FileSystemWatcher? _watcher;
    private readonly List<ImageInfo> _images = new();
    private readonly object _lock = new();
    private const string ImageExtension = ".jpg";

    /// <summary>
    /// Creates the image tracker and seeds it from the configured folder.
    /// </summary>
    /// <param name="configuration">Application configuration used to resolve the image root folder.</param>
    /// <param name="logger">Logger for filesystem change and catalog events.</param>
    public ImageService(IConfiguration configuration, ILogger<ImageService> logger)
    {
        _logger = logger;
        _watchFolder = configuration["ImageFolder"] ?? 
                       Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "SlideShow");
        
        Directory.CreateDirectory(_watchFolder);
        LoadExistingImages();
    }

    /// <summary>
    /// Starts a long-lived watcher so additions and deletions immediately update the in-memory list.
    /// </summary>
    public void StartMonitoring()
    {
        _watcher = new FileSystemWatcher(_watchFolder)
        {
            Filter = "*" + ImageExtension,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Created += OnImageAdded;
        _watcher.Deleted += OnImageDeleted;
        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Performs the initial scan of the folder to seed the tracked images.
    /// </summary>
    private void LoadExistingImages()
    {
        lock (_lock)
        {
            _images.Clear();
            var jpgFiles = Directory.GetFiles(_watchFolder, $"*{ImageExtension}");
            
            foreach (var file in jpgFiles)
            {
                var fileName = Path.GetFileName(file);
                if (!_images.Any(i => i.FileName == fileName))
                {
                    _images.Add(new ImageInfo
                    {
                        FileName = fileName,
                        FilePath = file,
                        DisplayCount = 0,
                        DateAdded = DateTime.Now
                    });
                    _logger.LogInformation("Image discovered on startup: {FileName}", fileName);
                }
            }
        }
    }

    /// <summary>
    /// Adds a new entry when the watcher sees a file create.
    /// </summary>
    /// <remarks>A short delay avoids reading incomplete writes from some sources.</remarks>
    private void OnImageAdded(object sender, FileSystemEventArgs e)
    {
        System.Threading.Thread.Sleep(500);
        
        lock (_lock)
        {
            var fileName = Path.GetFileName(e.FullPath);
            if (!_images.Any(i => i.FileName == fileName))
            {
                _images.Add(new ImageInfo
                {
                    FileName = fileName,
                    FilePath = e.FullPath,
                    DisplayCount = 0,
                    DateAdded = DateTime.Now
                });

                _logger.LogInformation("Image added: {FileName} at {Path}", fileName, e.FullPath);
            }
        }
    }

    /// <summary>
    /// Removes stale entries when files are deleted from disk.
    /// </summary>
    private void OnImageDeleted(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            var fileName = Path.GetFileName(e.FullPath);
            _images.RemoveAll(i => i.FileName == fileName);
            _logger.LogInformation("Image removed: {FileName}", fileName);
        }
    }

    /// <summary>
    /// Returns a snapshot of all tracked images.
    /// </summary>
    /// <returns>A new list to preserve thread safety and prevent external mutation.</returns>
    public List<ImageInfo> GetAllImages()
    {
        lock (_lock)
        {
            return new List<ImageInfo>(_images);
        }
    }

    /// <summary>
    /// Increments the display count for the specified image.
    /// </summary>
    /// <param name="fileName">The image whose display count should be increased.</param>
    public void IncrementDisplayCount(string fileName)
    {
        lock (_lock)
        {
            var image = _images.FirstOrDefault(i => i.FileName == fileName);
            if (image != null)
            {
                image.DisplayCount++;
            }
        }
    }

    /// <summary>
    /// Retrieves the absolute file path for an image.
    /// </summary>
    /// <param name="fileName">The image file name to resolve.</param>
    /// <returns>The absolute path if tracked; otherwise an empty string.</returns>
    public string GetImagePath(string fileName)
    {
        lock (_lock)
        {
            var image = _images.FirstOrDefault(i => i.FileName == fileName);
            return image?.FilePath ?? string.Empty;
        }
    }
}
