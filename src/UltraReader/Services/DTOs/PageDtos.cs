namespace UltraReader.Services.DTOs;

/// <summary>
/// Page data for list display.
/// </summary>
public record PageDto
{
    public int Id { get; init; }
    public int PageNumber { get; init; }
    public string ImagePath { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}

/// <summary>
/// Command for adding a new page.
/// </summary>
public record AddPageCommand
{
    public int PageNumber { get; init; }
    public string ImagePath { get; init; } = string.Empty;
}

/// <summary>
/// Command for reordering pages.
/// </summary>
public record ReorderPagesCommand
{
    public List<int> PageIds { get; init; } = [];
}

/// <summary>
/// Result of folder import operation.
/// </summary>
public record FolderImportResult
{
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public List<string> Errors { get; init; } = [];
}
