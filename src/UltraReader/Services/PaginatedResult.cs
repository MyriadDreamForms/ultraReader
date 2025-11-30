namespace UltraReader.Services;

/// <summary>
/// Generic paginated result wrapper for list views.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }

    public PaginatedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items.ToList().AsReadOnly();
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>Total number of pages</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>Whether there is a previous page</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Whether there is a next page</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Create an empty result</summary>
    public static PaginatedResult<T> Empty(int page = 1, int pageSize = 20) 
        => new([], 0, page, pageSize);
}
