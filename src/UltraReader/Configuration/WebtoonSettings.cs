namespace UltraReader.Configuration;

/// <summary>
/// Strongly-typed settings for webtoon content configuration.
/// Bound from appsettings.json "WebtoonSettings" section.
/// </summary>
public class WebtoonSettings
{
    public const string SectionName = "WebtoonSettings";

    /// <summary>
    /// Root folder path where webtoon/manga images are stored.
    /// All image paths in the database are relative to this folder.
    /// </summary>
    public string ContentRootPath { get; set; } = string.Empty;

    /// <summary>
    /// Supported image file extensions for import and validation.
    /// </summary>
    public string[] SupportedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    /// <summary>
    /// Default page size for paginated lists.
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Maximum items to show in "Continue Reading" section.
    /// </summary>
    public int MaxContinueReadingItems { get; set; } = 10;

    /// <summary>
    /// Maximum items to show in "Recently Updated" section.
    /// </summary>
    public int MaxRecentlyUpdatedItems { get; set; } = 10;

    /// <summary>
    /// Validates that the content root path exists.
    /// </summary>
    public bool IsContentRootValid => !string.IsNullOrWhiteSpace(ContentRootPath) && Directory.Exists(ContentRootPath);
}
