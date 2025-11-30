using Microsoft.Extensions.Options;
using UltraReader.Configuration;

namespace UltraReader.Services;

/// <summary>
/// Service for handling image paths and URLs.
/// </summary>
public class ImageService : IImageService
{
    private readonly WebtoonSettings _settings;
    private readonly ILogger<ImageService> _logger;

    public ImageService(IOptions<WebtoonSettings> settings, ILogger<ImageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool ValidateImagePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        // Check for path traversal attempts
        if (relativePath.Contains("..") || Path.IsPathRooted(relativePath))
        {
            _logger.LogWarning("Invalid path detected: {Path}", relativePath);
            return false;
        }

        // Check extension
        var extension = Path.GetExtension(relativePath).ToLowerInvariant();
        return GetSupportedExtensions().Contains(extension);
    }

    public string GetFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(_settings.ContentRootPath))
            throw new InvalidOperationException("Content root path is not configured");

        // Normalize path separators
        var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar)
                                         .Replace('\\', Path.DirectorySeparatorChar);

        return Path.Combine(_settings.ContentRootPath, normalizedPath);
    }

    public string GetImageUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return "/images/placeholder.png";

        // Normalize to forward slashes for URL
        var urlPath = relativePath.Replace('\\', '/');
        
        // Ensure no double slashes
        while (urlPath.Contains("//"))
            urlPath = urlPath.Replace("//", "/");

        // Remove leading slash if present
        urlPath = urlPath.TrimStart('/');

        return $"/content/{urlPath}";
    }

    public IEnumerable<string> GetSupportedExtensions()
    {
        return _settings.SupportedImageExtensions ?? [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    }

    public bool FileExists(string relativePath)
    {
        if (!ValidateImagePath(relativePath))
            return false;

        try
        {
            var fullPath = GetFullPath(relativePath);
            return File.Exists(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {Path}", relativePath);
            return false;
        }
    }
}
