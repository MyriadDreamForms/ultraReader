using UltraReader.Entities;
using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service for series operations.
/// </summary>
public interface ISeriesService
{
    // Queries
    Task<PaginatedResult<SeriesCardDto>> GetLibraryAsync(LibraryFilter filter, int page, int pageSize);
    Task<SeriesDetailDto?> GetSeriesDetailAsync(string slug);
    Task<SeriesDetailDto?> GetSeriesDetailByIdAsync(int id);
    Task<List<string>> GetAllTagsAsync();

    // Commands
    Task<int> CreateSeriesAsync(CreateSeriesCommand command);
    Task UpdateSeriesAsync(int id, UpdateSeriesCommand command);
    Task DeleteSeriesAsync(int id);
    Task ToggleFavoriteAsync(int id);
}
