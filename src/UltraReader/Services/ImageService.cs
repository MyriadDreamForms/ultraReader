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
    private readonly IWebHostEnvironment _environment;
    private const string CoversFolder = "covers";

    public ImageService(IOptions<WebtoonSettings> settings, ILogger<ImageService> logger, IWebHostEnvironment environment)
    {
        _settings = settings.Value;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Gets the absolute path for the content root, handling relative paths.
    /// </summary>
    private string GetAbsoluteContentRootPath()
    {
        var contentRootPath = _settings.ContentRootPath ?? "Content";
        
        if (Path.IsPathRooted(contentRootPath))
            return contentRootPath;
        
        return Path.Combine(_environment.ContentRootPath, contentRootPath);
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
        var absoluteContentRoot = GetAbsoluteContentRootPath();

        // Normalize path separators
        var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar)
                                         .Replace('\\', Path.DirectorySeparatorChar);

        return Path.Combine(absoluteContentRoot, normalizedPath);
    }

    public string GetImageUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return "/images/placeholder.png";

        // Eğer base64 data URL ise direkt döndür (eski veriler için)
        if (relativePath.StartsWith("data:"))
            return relativePath;

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

    public async Task<string> SaveCoverImageAsync(Stream imageStream, string fileName)
    {
        // covers klasörünün mutlak yolunu al
        var absoluteContentRoot = GetAbsoluteContentRootPath();
        var coversPath = Path.Combine(absoluteContentRoot, CoversFolder);
        Directory.CreateDirectory(coversPath);

        // Benzersiz dosya adı oluştur
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(coversPath, uniqueFileName);

        // Dosyayı kaydet
        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await imageStream.CopyToAsync(fileStream);

        _logger.LogInformation("Cover image saved: {Path}", fullPath);

        // Göreceli yolu döndür
        return $"{CoversFolder}/{uniqueFileName}";
    }

    public void DeleteCoverImage(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath.StartsWith("data:"))
            return;

        try
        {
            var fullPath = GetFullPath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Cover image deleted: {Path}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover image: {Path}", relativePath);
        }
    }
}
