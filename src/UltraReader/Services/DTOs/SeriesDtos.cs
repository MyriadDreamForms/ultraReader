using UltraReader.Entities;

namespace UltraReader.Services.DTOs;

/// <summary>
/// Series data for library card display.
/// </summary>
public record SeriesCardDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? CoverImagePath { get; init; }
    public string? CoverImageUrl { get; init; }
    public SeriesStatus Status { get; init; }
    public bool IsFavorite { get; init; }
    public int TotalChapters { get; init; }
    public int UnreadChapterCount { get; init; }
    public DateTime? LastReadAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Filter options for the library view.
/// </summary>
public record LibraryFilter
{
    public string? Search { get; init; }
    public string? Tag { get; init; }
    public SeriesStatus? Status { get; init; }
    public bool? FavoritesOnly { get; init; }
    public SortOption Sort { get; init; } = SortOption.LastRead;
}

/// <summary>
/// Series detail data with chapters.
/// </summary>
public record SeriesDetailDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? OriginalTitle { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public SeriesStatus Status { get; init; }
    public bool IsFavorite { get; init; }
    public string? CoverImagePath { get; init; }
    public string? CoverImageUrl { get; init; }
    public List<string> Tags { get; init; } = [];
    public List<ChapterListItemDto> Chapters { get; init; } = [];
    public int? LastReadChapterId { get; init; }
    public decimal? LastReadChapterNumber { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Command for creating a new series.
/// </summary>
public record CreateSeriesCommand
{
    public required string Title { get; init; }
    public string? OriginalTitle { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public SeriesStatus Status { get; init; } = SeriesStatus.NotStarted;
    public string? CoverImagePath { get; init; }
    public List<string> Tags { get; init; } = [];
}

/// <summary>
/// Command for updating an existing series.
/// </summary>
public record UpdateSeriesCommand
{
    public required string Title { get; init; }
    public string? OriginalTitle { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public SeriesStatus Status { get; init; }
    public string? CoverImagePath { get; init; }
    public List<string> Tags { get; init; } = [];
}
