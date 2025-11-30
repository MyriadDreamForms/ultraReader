using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service for reader functionality.
/// </summary>
public interface IReaderService
{
    /// <summary>
    /// Gets all data needed to render the reader for a specific chapter.
    /// </summary>
    Task<ReaderDataDto?> GetReaderDataAsync(string seriesSlug, decimal chapterNumber);

    /// <summary>
    /// Updates reading progress when a chapter is opened.
    /// </summary>
    Task UpdateReadingProgressAsync(string seriesSlug, decimal chapterNumber);
}
