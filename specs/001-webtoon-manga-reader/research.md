# Research: Webtoon/Manga Reader Web Application

**Feature**: 001-webtoon-manga-reader  
**Date**: 2024-11-30  
**Purpose**: Resolve technical unknowns and document best practices for implementation

---

## Technology Decisions

### 1. Blazor Server vs Blazor WebAssembly

**Decision**: Blazor Server

**Rationale**:
- Simpler deployment as single self-hosted application
- No need for separate API project - server-side rendering handles data access directly
- Better for LAN environment where latency is minimal
- Smaller initial download (no WASM runtime to transfer)
- Full access to server resources (file system) without API layer

**Alternatives Considered**:
- Blazor WebAssembly: Rejected because requires separate API, adds complexity for single-user LAN app
- Blazor Hybrid: Rejected because desktop app not required; web browser access is preferred

---

### 2. Entity Framework Core Configuration for SQLite

**Decision**: Code-first with migrations, single DbContext

**Rationale**:
- Code-first allows version-controlled schema evolution
- SQLite is ideal for single-user, self-hosted application
- No separate database server to manage
- EF Core migrations provide easy schema updates

**Best Practices Applied**:
```csharp
// Connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=App_Data/webtoon.db"
}

// DbContext configuration
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Required indexes for performance
modelBuilder.Entity<Series>()
    .HasIndex(s => s.Slug)
    .IsUnique();

modelBuilder.Entity<Chapter>()
    .HasIndex(c => new { c.SeriesId, c.Number })
    .IsUnique();

modelBuilder.Entity<Page>()
    .HasIndex(p => new { p.ChapterId, p.PageNumber });

modelBuilder.Entity<ReadingProgress>()
    .HasIndex(rp => rp.SeriesId)
    .IsUnique();  // Single user, one progress per series
```

---

### 3. Static File Serving for Images

**Decision**: ASP.NET Core Static Files Middleware with custom PhysicalFileProvider

**Rationale**:
- Images remain on disk, not copied or stored in database
- Standard HTTP caching works out of the box
- No upload/download overhead - direct file serving

**Best Practices Applied**:
```csharp
// Configure static file serving from configurable root folder
var contentRoot = configuration["WebtoonSettings:ContentRootPath"];
if (!string.IsNullOrEmpty(contentRoot) && Directory.Exists(contentRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(contentRoot),
        RequestPath = "/content"
    });
}
```

**URL Pattern**: `/content/{series-slug}/{chapter-folder}/{image-file}`

---

### 4. Tags Implementation

**Decision**: Tags stored as JSON array column in Series table

**Rationale**:
- Simpler than many-to-many relationship for single-user app
- SQLite supports JSON functions for querying
- No need for tag normalization/deduplication in single-user context
- Fewer tables, simpler queries

**Implementation**:
```csharp
public class Series
{
    // Other properties...
    public List<string> Tags { get; set; } = new();
}

// EF Core configuration
modelBuilder.Entity<Series>()
    .Property(s => s.Tags)
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new()
    );
```

**Filtering**:
```csharp
// In-memory filtering after load (acceptable for single-user scale)
series.Where(s => s.Tags.Contains("action"))
```

---

### 5. Reading Progress Tracking

**Decision**: Single ReadingProgress record per Series

**Rationale**:
- Single user - no need for user-series composite key
- Simple update on chapter open/complete
- Easy "continue reading" query

**Best Practices Applied**:
```csharp
public class ReadingProgress
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public Series Series { get; set; } = null!;
    public int LastReadChapterId { get; set; }
    public Chapter LastReadChapter { get; set; } = null!;
    public DateTime LastReadAt { get; set; }
    // Optional for future: int? LastPageNumber, double? ScrollPosition
}

// Continue reading query
var continueReading = await dbContext.ReadingProgress
    .Include(rp => rp.Series)
    .Include(rp => rp.LastReadChapter)
    .OrderByDescending(rp => rp.LastReadAt)
    .Take(10)
    .ToListAsync();
```

---

### 6. Mobile-First Reader CSS

**Decision**: Custom CSS with dark theme, no heavy framework

**Rationale**:
- Minimal CSS for reader performance
- Full control over vertical scroll behavior
- Dark theme reduces eye strain for long reading sessions

**Best Practices Applied**:
```css
/* reader.css */
.reader-container {
    background-color: #1a1a1a;
    min-height: 100vh;
    padding: 0;
}

.reader-page {
    display: flex;
    justify-content: center;
    width: 100%;
}

.reader-page img {
    max-width: 100%;
    height: auto;
    display: block;
}

/* Mobile optimization */
@media (max-width: 768px) {
    .reader-page img {
        width: 100%;
    }
}
```

---

### 7. Pagination Strategy

**Decision**: Offset-based pagination with configurable page size

**Rationale**:
- Simple to implement with EF Core Skip/Take
- Suitable for single-user scale (500 series, 100 chapters)
- Cursor-based pagination unnecessary for this scale

**Best Practices Applied**:
```csharp
public async Task<PaginatedResult<Series>> GetSeriesAsync(
    int page = 1, 
    int pageSize = 20,
    string? search = null,
    string? tag = null,
    SeriesStatus? status = null,
    SortOption sort = SortOption.LastRead)
{
    var query = dbContext.Series.AsQueryable();
    
    // Apply filters
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(s => s.Title.Contains(search));
    
    // ... more filters
    
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PaginatedResult<Series>(items, totalCount, page, pageSize);
}
```

---

### 8. Folder Import for Pages

**Decision**: Server-side file system scanning with pattern matching

**Rationale**:
- User specifies folder path relative to content root
- Service scans folder, filters by image extensions
- Sorts by filename naturally (001.jpg, 002.jpg, ...)

**Best Practices Applied**:
```csharp
public class FolderImportService
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    
    public IEnumerable<string> GetImagesFromFolder(string relativeFolderPath)
    {
        var fullPath = Path.Combine(_contentRoot, relativeFolderPath);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Folder not found: {relativeFolderPath}");
        
        return Directory.GetFiles(fullPath)
            .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f, new NaturalStringComparer())
            .Select(f => Path.GetRelativePath(_contentRoot, f).Replace('\\', '/'));
    }
}
```

---

### 9. Error Handling for Missing Images

**Decision**: Client-side fallback with onerror + server-side logging

**Rationale**:
- Image tags handle missing files gracefully with onerror event
- Server logs missing files for admin awareness
- Application never crashes due to missing images

**Best Practices Applied**:
```html
<!-- In Blazor component -->
<img src="@imageUrl" 
     alt="Page @pageNumber"
     onerror="this.onerror=null; this.src='/images/placeholder.png'; this.classList.add('error');" />
```

```csharp
// Service validates image existence
public bool ValidateImagePath(string relativePath)
{
    var fullPath = Path.Combine(_contentRoot, relativePath);
    if (!File.Exists(fullPath))
    {
        _logger.LogWarning("Image not found: {Path}", relativePath);
        return false;
    }
    return true;
}
```

---

### 10. Configuration Management

**Decision**: appsettings.json with strongly-typed options pattern

**Rationale**:
- Standard ASP.NET Core configuration pattern
- Environment-specific overrides possible
- Runtime settings UI optional (can update file and restart)

**Best Practices Applied**:
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/webtoon.db"
  },
  "WebtoonSettings": {
    "ContentRootPath": "C:/Webtoons",
    "DefaultPageSize": 20,
    "MaxImportFiles": 500
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
}
```

```csharp
// Options class
public class WebtoonSettings
{
    public string ContentRootPath { get; set; } = string.Empty;
    public int DefaultPageSize { get; set; } = 20;
    public int MaxImportFiles { get; set; } = 500;
}

// Registration
services.Configure<WebtoonSettings>(configuration.GetSection("WebtoonSettings"));
```

---

## Dependencies Summary

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.App | .NET 10 | ASP.NET Core framework |
| Microsoft.EntityFrameworkCore.Sqlite | 10.x | SQLite database provider |
| Microsoft.EntityFrameworkCore.Design | 10.x | EF Core migrations tooling |
| Microsoft.Extensions.FileProviders.Physical | 10.x | Static file serving from custom path |

**Testing Dependencies**:
| Package | Purpose |
|---------|---------|
| xUnit | Test framework |
| bUnit | Blazor component testing |
| Microsoft.EntityFrameworkCore.InMemory | In-memory DB for unit tests |
| FluentAssertions | Assertion library (optional) |

---

## Open Questions Resolved

| Question | Resolution |
|----------|------------|
| How to serve images from configurable folder? | PhysicalFileProvider with StaticFileMiddleware |
| How to store tags efficiently? | JSON array column with EF Core value converter |
| How to handle missing images? | Client-side onerror fallback + server logging |
| How to import pages from folder? | Server-side Directory.GetFiles with natural sort |
| How to track reading progress? | Single ReadingProgress record per series |

---

## Next Steps

1. **Phase 1**: Create data-model.md with detailed entity definitions
2. **Phase 1**: Create API contracts (internal routes documentation)
3. **Phase 1**: Create quickstart.md with setup instructions
4. **Phase 2**: Generate implementation tasks
