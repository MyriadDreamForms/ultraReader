using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service interface for chapter operations.
/// </summary>
public interface IChapterService
{
    /// <summary>
    /// Gets all chapters for a series.
    /// </summary>
    Task<List<ChapterListItemDto>> GetChaptersAsync(int seriesId);

    /// <summary>
    /// Gets a chapter for editing.
    /// </summary>
    Task<ChapterEditDto?> GetChapterForEditAsync(int chapterId);

    /// <summary>
    /// Creates a new chapter.
    /// </summary>
    Task<int> CreateChapterAsync(int seriesId, CreateChapterCommand command);

    /// <summary>
    /// Updates an existing chapter.
    /// </summary>
    Task UpdateChapterAsync(int chapterId, UpdateChapterCommand command);

    /// <summary>
    /// Deletes a chapter.
    /// </summary>
    Task DeleteChapterAsync(int chapterId);

    /// <summary>
    /// Marks a chapter as read.
    /// </summary>
    Task MarkAsReadAsync(int chapterId);
    
    /// <summary>
    /// Marks a chapter as unread.
    /// </summary>
    Task MarkAsUnreadAsync(int chapterId);
    
    /// <summary>
    /// Toggles the read status of a chapter.
    /// </summary>
    Task<bool> ToggleReadStatusAsync(int chapterId);
}
