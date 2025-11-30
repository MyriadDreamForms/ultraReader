namespace UltraReader.Services.DTOs;

/// <summary>
/// Data for "Continue Reading" section on dashboard.
/// </summary>
public record ContinueReadingDto
{
    public int SeriesId { get; init; }
    public required string SeriesTitle { get; init; }
    public required string SeriesSlug { get; init; }
    public string? CoverImageUrl { get; init; }
    public decimal NextChapterNumber { get; init; }
    public int? NextChapterId { get; init; }
    public DateTime LastReadAt { get; init; }
}

/// <summary>
/// Data for "Recently Updated" section on dashboard.
/// </summary>
public record RecentlyUpdatedDto
{
    public int SeriesId { get; init; }
    public required string SeriesTitle { get; init; }
    public required string SeriesSlug { get; init; }
    public string? CoverImageUrl { get; init; }
    public decimal LatestChapterNumber { get; init; }
    public DateTime UpdatedAt { get; init; }
}
