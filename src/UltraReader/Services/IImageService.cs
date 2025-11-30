namespace UltraReader.Services;

/// <summary>
/// Service for handling image paths and URLs.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Validates if a relative image path is valid.
    /// </summary>
    bool ValidateImagePath(string relativePath);

    /// <summary>
    /// Gets the full file system path for a relative image path.
    /// </summary>
    string GetFullPath(string relativePath);

    /// <summary>
    /// Gets the URL for serving an image from the content folder.
    /// </summary>
    string GetImageUrl(string relativePath);

    /// <summary>
    /// Gets supported image file extensions.
    /// </summary>
    IEnumerable<string> GetSupportedExtensions();

    /// <summary>
    /// Checks if a file exists at the given relative path.
    /// </summary>
    bool FileExists(string relativePath);

    /// <summary>
    /// Saves a cover image and returns the relative path.
    /// </summary>
    Task<string> SaveCoverImageAsync(Stream imageStream, string fileName);

    /// <summary>
    /// Saves a page image and returns the relative path.
    /// </summary>
    Task<string> SavePageImageAsync(Stream imageStream, string fileName, int chapterId);

    /// <summary>
    /// Deletes a cover image.
    /// </summary>
    void DeleteCoverImage(string relativePath);
}
