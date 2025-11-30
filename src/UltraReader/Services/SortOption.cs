namespace UltraReader.Services;

/// <summary>
/// Sort options for library and list views.
/// </summary>
public enum SortOption
{
    /// <summary>Sort by last read date (most recent first)</summary>
    LastRead,
    
    /// <summary>Sort by last updated date (most recent first)</summary>
    LastUpdated,
    
    /// <summary>Sort alphabetically by title (A-Z)</summary>
    Alphabetical,
    
    /// <summary>Sort alphabetically by title (Z-A)</summary>
    AlphabeticalDesc
}
