using System.ComponentModel.DataAnnotations;

namespace UltraReader.Entities;

/// <summary>
/// Represents a single image page within a chapter.
/// </summary>
public class Page
{
    public int Id { get; set; }

    /// <summary>Parent chapter ID</summary>
    public int ChapterId { get; set; }

    /// <summary>Display order (1-based). Duplicate orders are sorted by Id as tiebreaker.</summary>
    public int PageNumber { get; set; }

    /// <summary>Relative path from content root to the image file</summary>
    [Required]
    [MaxLength(1000)]
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last modification timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Chapter Chapter { get; set; } = null!;
}
