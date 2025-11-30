using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service interface for dashboard operations.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets series with reading progress for "Continue Reading" section.
    /// </summary>
    Task<List<ContinueReadingDto>> GetContinueReadingAsync(int limit = 10);

    /// <summary>
    /// Gets recently updated series for "Recently Updated" section.
    /// </summary>
    Task<List<RecentlyUpdatedDto>> GetRecentlyUpdatedAsync(int limit = 10);
}
