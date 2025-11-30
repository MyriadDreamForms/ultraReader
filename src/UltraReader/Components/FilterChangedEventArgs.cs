using UltraReader.Entities;
using UltraReader.Services;

namespace UltraReader.Components;

/// <summary>
/// Event args for filter changes in SearchFilter component.
/// </summary>
public record FilterChangedEventArgs
{
    public string? SearchTerm { get; init; }
    public string? Tag { get; init; }
    public SeriesStatus? Status { get; init; }
    public bool? FavoritesOnly { get; init; }
    public SortOption SortBy { get; init; } = SortOption.LastRead;
}
