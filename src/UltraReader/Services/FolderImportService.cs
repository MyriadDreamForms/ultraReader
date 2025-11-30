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
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

    public FolderImportService(IOptions<WebtoonSettings> settings, ILogger<FolderImportService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
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
        if (string.IsNullOrWhiteSpace(_settings.ContentRootPath))
            return false;

        var contentRoot = Path.GetFullPath(_settings.ContentRootPath);
        var targetPath = Path.GetFullPath(path);

        return targetPath.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase);
    }

    public string GetRelativePath(string absolutePath)
    {
        if (string.IsNullOrWhiteSpace(_settings.ContentRootPath))
            return absolutePath;

        var contentRoot = Path.GetFullPath(_settings.ContentRootPath);
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
