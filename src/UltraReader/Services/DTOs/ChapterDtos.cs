namespace UltraReader.Services.DTOs;

/// <summary>
/// Chapter data for list display.
/// </summary>
public record ChapterListItemDto
{
    public int Id { get; init; }
    public decimal Number { get; init; }
    public string? Title { get; init; }
    public bool IsRead { get; init; }
    public DateTime? LastReadAt { get; init; }
    public int PageCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Chapter data for editing.
/// </summary>
public record ChapterEditDto
{
    public int Id { get; init; }
    public int SeriesId { get; init; }
    public string? SeriesTitle { get; init; }
    public decimal Number { get; init; }
    public string? Title { get; init; }
    public DateTime? ReleaseDate { get; init; }
}

/// <summary>
/// Command for creating a new chapter.
/// </summary>
public record CreateChapterCommand
{
    public decimal Number { get; init; }
    public string? Title { get; init; }
    public DateTime? ReleaseDate { get; init; }
}

/// <summary>
/// Command for updating an existing chapter.
/// </summary>
public record UpdateChapterCommand
{
    public decimal Number { get; init; }
    public string? Title { get; init; }
    public DateTime? ReleaseDate { get; init; }
}
