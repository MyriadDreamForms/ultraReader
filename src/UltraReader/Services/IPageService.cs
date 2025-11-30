using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service interface for page operations.
/// </summary>
public interface IPageService
{
    /// <summary>
    /// Gets all pages for a chapter.
    /// </summary>
    Task<List<PageDto>> GetPagesAsync(int chapterId);

    /// <summary>
    /// Adds a new page to a chapter.
    /// </summary>
    Task<int> AddPageAsync(int chapterId, AddPageCommand command);

    /// <summary>
    /// Updates page order based on the provided list.
    /// </summary>
    Task UpdatePageOrderAsync(int chapterId, List<int> pageIds);

    /// <summary>
    /// Deletes a page.
    /// </summary>
    Task DeletePageAsync(int pageId);

    /// <summary>
    /// Imports pages from a folder.
    /// </summary>
    Task<FolderImportResult> ImportFromFolderAsync(int chapterId, string folderPath);
}
