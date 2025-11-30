namespace UltraReader.Entities;

/// <summary>
/// Tracks reading state for a series.
/// One ReadingProgress per Series (1:1 relationship).
/// </summary>
public class ReadingProgress
{
    public int Id { get; set; }

    /// <summary>Related series ID (unique - one progress per series)</summary>
    public int SeriesId { get; set; }

    /// <summary>Last read chapter ID</summary>
    public int LastReadChapterId { get; set; }

    /// <summary>When the chapter was last read</summary>
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last viewed page number (optional, for resume to exact position)</summary>
    public int? LastPageNumber { get; set; }

    // Navigation properties
    public Series Series { get; set; } = null!;
    public Chapter LastReadChapter { get; set; } = null!;
}
