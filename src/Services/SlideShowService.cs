using Microsoft.Extensions.Logging;
using SimpleSlideShow.Models;

namespace SimpleSlideShow.Services;

/// <summary>
/// Chooses the next image to show, applying fairness rules and optional weighted randomness.
/// </summary>
public class SlideShowService
{
    private readonly ImageService _imageService;
    private readonly ILogger<SlideShowService> _logger;
    private Random _random = new();

    /// <summary>
    /// Creates the selection service with access to the current image catalog.
    /// </summary>
    /// <param name="imageService">Provides image metadata and state.</param>
    /// <param name="logger">Logger for selection diagnostics.</param>
    public SlideShowService(ImageService imageService, ILogger<SlideShowService> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    /// <summary>
    /// Determines the next image to display.
    /// </summary>
    /// <param name="randomMode">When true, uses weighted randomness; otherwise uses a fair sequential strategy.</param>
    /// <returns>The selected image or <c>null</c> when no images are available.</returns>
    public ImageInfo? GetNextImage(bool randomMode)
    {
        var images = _imageService.GetAllImages();

        if (images.Count == 0)
            return null;

        ImageInfo selectedImage;

        if (randomMode)
        {
            selectedImage = SelectWithWeighting(images);
        }
        else
        {
            selectedImage = images.OrderBy(i => i.DisplayCount)
                                  .ThenBy(i => i.DateAdded)
                                  .First();
            _logger.LogInformation(
                "Selected image (sequential): {FileName} with displayCount={DisplayCount} added={Added:O}",
                selectedImage.FileName,
                selectedImage.DisplayCount,
                selectedImage.DateAdded);
        }

        _imageService.IncrementDisplayCount(selectedImage.FileName);
        return selectedImage;
    }

    /// <summary>
    /// Applies weighted randomness so under-served and new images rise to the top without strict sequencing.
    /// </summary>
    /// <param name="images">The current set of candidate images.</param>
    /// <returns>An image chosen using roulette-wheel selection.</returns>
    private ImageInfo SelectWithWeighting(List<ImageInfo> images)
    {
        if (images.Count == 1)
            return images[0];

        double averageDisplayCount = images.Average(i => i.DisplayCount);

        var weights = images.Select((image, index) => new
        {
            Image = image,
            Weight = CalculateWeight(image, averageDisplayCount)
        }).ToList();

        double totalWeight = weights.Sum(w => w.Weight);
        double randomValue = _random.NextDouble() * totalWeight;
        double cumulativeWeight = 0;

        foreach (var item in weights)
        {
            cumulativeWeight += item.Weight;
            if (randomValue <= cumulativeWeight)
            {
                _logger.LogInformation(
                    "Selected image (random): {FileName} weight={Weight:F3} displayCount={DisplayCount} randomValue={RandomValue:F4} totalWeight={TotalWeight:F3}",
                    item.Image.FileName,
                    item.Weight,
                    item.Image.DisplayCount,
                    randomValue,
                    totalWeight);
                return item.Image;
            }
        }

        var fallback = weights.Last().Image;
        _logger.LogInformation(
            "Selected image (random-fallback): {FileName} weight={Weight:F3} displayCount={DisplayCount} totalWeight={TotalWeight:F3}",
            fallback.FileName,
            weights.Last().Weight,
            fallback.DisplayCount,
            totalWeight);
        return fallback;
    }

    /// <summary>
    /// Calculates the weighting factor for a single image.
    /// </summary>
    /// <param name="image">The image being evaluated.</param>
    /// <param name="averageDisplayCount">The current average display count across all images.</param>
    /// <returns>A weight where values above 1 favor under-served or new images.</returns>
    private double CalculateWeight(ImageInfo image, double averageDisplayCount)
    {
        // Base weight: 1
        double weight = 1.0;

        // If below average, increase weight (more likely to be shown)
        if (image.DisplayCount < averageDisplayCount)
        {
            double deficit = averageDisplayCount - image.DisplayCount;
            weight += deficit * 0.5; // Multiplier to prioritize under-shown images
        }

        var ageDays = (DateTime.Now - image.DateAdded).TotalMinutes;
        if (ageDays < 1)
        {
            weight *= 1.5; // New images 50% more likely
        }

        return weight;
    }
}
