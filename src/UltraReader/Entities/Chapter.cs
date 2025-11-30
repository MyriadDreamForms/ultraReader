using System.ComponentModel.DataAnnotations;

namespace UltraReader.Entities;

/// <summary>
/// Represents a chapter/episode within a series.
/// </summary>
public class Chapter
{
    public int Id { get; set; }

    /// <summary>Parent series ID</summary>
    public int SeriesId { get; set; }

    /// <summary>Chapter number (supports decimals like 10.5)</summary>
    public decimal Number { get; set; }

    /// <summary>Optional chapter title</summary>
    [MaxLength(500)]
    public string? Title { get; set; }

    /// <summary>Original release date (optional)</summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last modification timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Series Series { get; set; } = null!;
    public ICollection<Page> Pages { get; set; } = [];
}
