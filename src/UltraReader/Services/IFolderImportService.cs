namespace UltraReader.Services;

/// <summary>
/// Represents a file or folder item in the content browser.
/// </summary>
public class ContentBrowserItem
{
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public bool IsImage { get; set; }
}

/// <summary>
/// Service interface for folder import operations.
/// </summary>
public interface IFolderImportService
{
    /// <summary>
    /// Gets image files from a folder sorted naturally.
    /// </summary>
    List<string> GetImageFilesFromFolder(string folderPath);

    /// <summary>
    /// Validates if a path is within the allowed content root.
    /// </summary>
    bool IsPathWithinContentRoot(string path);

    /// <summary>
    /// Gets the relative path from content root.
    /// </summary>
    string GetRelativePath(string absolutePath);

    /// <summary>
    /// Gets folders and images in a directory for content browsing.
    /// </summary>
    List<ContentBrowserItem> GetContentItems(string relativePath = "");

    /// <summary>
    /// Gets the content root path.
    /// </summary>
    string GetContentRootPath();
}
