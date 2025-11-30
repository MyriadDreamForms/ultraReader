using System.ComponentModel.DataAnnotations;

namespace UltraReader.Entities;

/// <summary>
/// Represents a manga/webtoon title in the library.
/// </summary>
public class Series
{
    public int Id { get; set; }

    /// <summary>Display title</summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Original language title (optional)</summary>
    [MaxLength(500)]
    public string? OriginalTitle { get; set; }

    /// <summary>URL-safe identifier (e.g., "tower-of-god")</summary>
    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Synopsis or description</summary>
    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>Reading status</summary>
    public SeriesStatus Status { get; set; } = SeriesStatus.NotStarted;

    /// <summary>Favorite flag for quick filtering</summary>
    public bool IsFavorite { get; set; }

    /// <summary>Relative path to cover image from content root</summary>
    [MaxLength(1000)]
    public string? CoverImagePath { get; set; }

    /// <summary>Absolute path to the series folder on disk</summary>
    [MaxLength(2000)]
    public string? FolderPath { get; set; }

    /// <summary>Genre/category tags stored as JSON array</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last modification timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Chapter> Chapters { get; set; } = [];
    public ReadingProgress? ReadingProgress { get; set; }
}
