namespace UltraReader.Services;

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
}
