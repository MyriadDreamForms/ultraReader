namespace UltraReader.Services.DTOs;

/// <summary>
/// Data for rendering the reader page.
/// </summary>
public record ReaderDataDto
{
    public required string SeriesTitle { get; init; }
    public required string SeriesSlug { get; init; }
    public int SeriesId { get; init; }
    public decimal ChapterNumber { get; init; }
    public string? ChapterTitle { get; init; }
    public int ChapterId { get; init; }
    public List<ReaderPageDto> Pages { get; init; } = [];
    public decimal? PreviousChapterNumber { get; init; }
    public decimal? NextChapterNumber { get; init; }
}

/// <summary>
/// Single page data for the reader.
/// </summary>
public record ReaderPageDto
{
    public int PageNumber { get; init; }
    public required string ImageUrl { get; init; }
}
