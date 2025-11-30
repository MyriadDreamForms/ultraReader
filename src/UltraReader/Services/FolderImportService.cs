using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using UltraReader.Configuration;

namespace UltraReader.Services;

/// <summary>
/// Service for folder import operations with natural sorting.
/// </summary>
public class FolderImportService : IFolderImportService
{
    private readonly WebtoonSettings _settings;
    private readonly ILogger<FolderImportService> _logger;
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

    public FolderImportService(IOptions<WebtoonSettings> settings, ILogger<FolderImportService> logger, IWebHostEnvironment environment)
    {
        _settings = settings.Value;
        _logger = logger;
        _environment = environment;
    }

    private string GetAbsoluteContentRootPath()
    {
        if (string.IsNullOrWhiteSpace(_settings.ContentRootPath))
            return string.Empty;

        // Eğer mutlak yol ise direkt kullan, değilse uygulama dizinine göre çöz
        if (Path.IsPathRooted(_settings.ContentRootPath))
            return _settings.ContentRootPath;

        return Path.Combine(_environment.ContentRootPath, _settings.ContentRootPath);
    }

    public string GetContentRootPath()
    {
        return GetAbsoluteContentRootPath();
    }

    public List<ContentBrowserItem> GetContentItems(string relativePath = "")
    {
        var items = new List<ContentBrowserItem>();
        
        var absoluteContentRoot = GetAbsoluteContentRootPath();
        if (string.IsNullOrWhiteSpace(absoluteContentRoot))
        {
            _logger.LogWarning("ContentRootPath is not configured");
            return items;
        }

        var basePath = Path.GetFullPath(absoluteContentRoot);
        var targetPath = string.IsNullOrEmpty(relativePath) 
            ? basePath 
            : Path.GetFullPath(Path.Combine(basePath, relativePath));

        // Security check
        if (!targetPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Attempted to access path outside content root: {Path}", targetPath);
            return items;
        }

        if (!Directory.Exists(targetPath))
        {
            _logger.LogWarning("Directory does not exist: {Path}", targetPath);
            return items;
        }

        try
        {
            // Get folders
            var directories = Directory.GetDirectories(targetPath);
            foreach (var dir in directories.OrderBy(d => Path.GetFileName(d)))
            {
                var folderName = Path.GetFileName(dir);
                var folderRelativePath = string.IsNullOrEmpty(relativePath) 
                    ? folderName 
                    : Path.Combine(relativePath, folderName).Replace('\\', '/');

                items.Add(new ContentBrowserItem
                {
                    Name = folderName,
                    RelativePath = folderRelativePath,
                    IsFolder = true,
                    IsImage = false
                });
            }

            // Get image files
            var files = Directory.GetFiles(targetPath)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            files.Sort(NaturalCompare);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var fileRelativePath = string.IsNullOrEmpty(relativePath) 
                    ? fileName 
                    : Path.Combine(relativePath, fileName).Replace('\\', '/');

                items.Add(new ContentBrowserItem
                {
                    Name = fileName,
                    RelativePath = fileRelativePath,
                    IsFolder = false,
                    IsImage = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading directory: {Path}", targetPath);
        }

        return items;
    }

    public List<string> GetImageFilesFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            _logger.LogWarning("Folder does not exist: {FolderPath}", folderPath);
            return [];
        }

        var files = Directory.GetFiles(folderPath)
            .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLower()))
            .ToList();

        // Natural sort (handles numeric parts correctly: 1, 2, 10, 11 instead of 1, 10, 11, 2)
        files.Sort(NaturalCompare);

        _logger.LogInformation("Found {Count} image files in {FolderPath}", files.Count, folderPath);
        return files;
    }

    public bool IsPathWithinContentRoot(string path)
    {
        var absoluteContentRoot = GetAbsoluteContentRootPath();
        if (string.IsNullOrWhiteSpace(absoluteContentRoot))
            return false;

        var contentRoot = Path.GetFullPath(absoluteContentRoot);
        var targetPath = Path.GetFullPath(path);

        return targetPath.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase);
    }

    public string GetRelativePath(string absolutePath)
    {
        var absoluteContentRoot = GetAbsoluteContentRootPath();
        if (string.IsNullOrWhiteSpace(absoluteContentRoot))
            return absolutePath;

        var contentRoot = Path.GetFullPath(absoluteContentRoot);
        var targetPath = Path.GetFullPath(absolutePath);

        if (targetPath.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = targetPath.Substring(contentRoot.Length);
            return relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        return absolutePath;
    }

    private static int NaturalCompare(string x, string y)
    {
        // Extract numeric parts for comparison
        var numericPattern = new Regex(@"(\d+)");
        
        var xParts = numericPattern.Split(Path.GetFileName(x));
        var yParts = numericPattern.Split(Path.GetFileName(y));

        for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
        {
            int result;
            
            // Try to parse as numbers
            if (int.TryParse(xParts[i], out int xNum) && int.TryParse(yParts[i], out int yNum))
            {
                result = xNum.CompareTo(yNum);
            }
            else
            {
                result = string.Compare(xParts[i], yParts[i], StringComparison.OrdinalIgnoreCase);
            }

            if (result != 0)
                return result;
        }

        return xParts.Length.CompareTo(yParts.Length);
    }
}
