# Data Model: Webtoon/Manga Reader Web Application

**Feature**: 001-webtoon-manga-reader  
**Date**: 2024-11-30  
**Database**: SQLite via Entity Framework Core

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│  ┌─────────────┐         ┌─────────────┐         ┌─────────────┐           │
│  │   Series    │ 1     * │   Chapter   │ 1     * │    Page     │           │
│  ├─────────────┤─────────├─────────────┤─────────├─────────────┤           │
│  │ Id (PK)     │         │ Id (PK)     │         │ Id (PK)     │           │
│  │ Title       │         │ SeriesId(FK)│         │ ChapterId(FK│           │
│  │ OrigTitle   │         │ Number      │         │ PageNumber  │           │
│  │ Slug (UQ)   │         │ Title       │         │ ImagePath   │           │
│  │ Description │         │ ReleaseDate │         │ CreatedAt   │           │
│  │ Status      │         │ CreatedAt   │         │ UpdatedAt   │           │
│  │ IsFavorite  │         │ UpdatedAt   │         └─────────────┘           │
│  │ CoverPath   │         └─────────────┘                                   │
│  │ Tags (JSON) │  ← Stored as JSON column, no separate Tag entity          │
│  │ CreatedAt   │         ┌──────────────────┐                              │
│  │ UpdatedAt   │ 1     1 │ ReadingProgress  │                              │
│  └─────────────┘─────────├──────────────────┤                              │
│                          │ Id (PK)          │                              │
│                          │ SeriesId (FK,UQ) │                              │
│                          │ LastChapterId(FK)│                              │
│                          │ LastReadAt       │                              │
│                          │ LastPageNumber   │                              │
│                          └──────────────────┘                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Entity Definitions

### Series

Represents a manga/webtoon title in the library.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | int | PK, Identity | Primary key |
| `Title` | string | Required, MaxLength(500) | Display title |
| `OriginalTitle` | string? | MaxLength(500) | Original language title (optional) |
| `Slug` | string | Required, Unique, MaxLength(200) | URL-safe identifier (e.g., `tower-of-god`) |
| `Description` | string? | MaxLength(4000) | Synopsis or description |
| `Status` | SeriesStatus | Required, Default: NotStarted | Reading status enum |
| `IsFavorite` | bool | Required, Default: false | Favorite flag for filtering |
| `CoverImagePath` | string? | MaxLength(1000) | Relative path to cover image |
| `Tags` | List\<string\> | JSON column | Genre/category tags |
| `CreatedAt` | DateTime | Required, Default: UtcNow | Record creation timestamp |
| `UpdatedAt` | DateTime | Required | Last modification timestamp |

**Navigation Properties**:
- `Chapters`: Collection of Chapter entities
- `ReadingProgress`: Optional ReadingProgress entity

**Indexes**:
- `IX_Series_Slug` (Unique) - For URL routing
- `IX_Series_Status` - For status filtering
- `IX_Series_IsFavorite` - For favorites filtering
- `IX_Series_UpdatedAt` - For "recently updated" sorting

---

### SeriesStatus (Enum)

```csharp
public enum SeriesStatus
{
    NotStarted = 0,   // Haven't started reading
    Reading = 1,      // Currently reading
    Completed = 2,    // Finished reading
    OnHold = 3,       // Paused
    Dropped = 4       // Abandoned
}
```

---

### Chapter

Represents a chapter/episode within a series.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | int | PK, Identity | Primary key |
| `SeriesId` | int | FK → Series.Id, Required | Parent series |
| `Number` | decimal | Required | Chapter number (supports 10.5, etc.) |
| `Title` | string? | MaxLength(500) | Optional chapter title |
| `ReleaseDate` | DateTime? | | Original release date (optional) |
| `CreatedAt` | DateTime | Required, Default: UtcNow | Record creation timestamp |
| `UpdatedAt` | DateTime | Required | Last modification timestamp |

**Navigation Properties**:
- `Series`: Parent Series entity
- `Pages`: Collection of Page entities

**Indexes**:
- `IX_Chapter_SeriesId_Number` (Unique) - Ensures unique chapter numbers per series
- `IX_Chapter_SeriesId` - For chapter list queries

**Cascade Delete**: When Series is deleted, all Chapters are deleted.

---

### Page

Represents a single image page within a chapter.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | int | PK, Identity | Primary key |
| `ChapterId` | int | FK → Chapter.Id, Required | Parent chapter |
| `PageNumber` | int | Required | Display order (1-based) |
| `ImagePath` | string | Required, MaxLength(1000) | Relative path from content root |
| `CreatedAt` | DateTime | Required, Default: UtcNow | Record creation timestamp |
| `UpdatedAt` | DateTime | Required | Last modification timestamp |

**Navigation Properties**:
- `Chapter`: Parent Chapter entity

**Indexes**:
- `IX_Page_ChapterId_PageNumber` - For ordered page retrieval
- `IX_Page_ChapterId` - For page list queries

**Cascade Delete**: When Chapter is deleted, all Pages are deleted.

---

### ReadingProgress

Tracks reading state for a series.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | int | PK, Identity | Primary key |
| `SeriesId` | int | FK → Series.Id, Unique | One progress per series |
| `LastReadChapterId` | int | FK → Chapter.Id, Required | Last read chapter |
| `LastReadAt` | DateTime | Required | When chapter was last read |
| `LastPageNumber` | int? | | Last viewed page (optional, for resume) |

**Navigation Properties**:
- `Series`: Related Series entity
- `LastReadChapter`: Related Chapter entity

**Indexes**:
- `IX_ReadingProgress_SeriesId` (Unique) - One progress per series
- `IX_ReadingProgress_LastReadAt` - For "continue reading" sorting

**Cascade Delete**: When Series is deleted, ReadingProgress is deleted.

---

## EF Core Configuration

### DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Series> Series => Set<Series>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<ReadingProgress> ReadingProgress => Set<ReadingProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Series configuration
        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsFavorite);
            entity.HasIndex(e => e.UpdatedAt);
            
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );
        });

        // Chapter configuration
        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasIndex(e => new { e.SeriesId, e.Number }).IsUnique();
            entity.HasIndex(e => e.SeriesId);
            
            entity.HasOne(e => e.Series)
                .WithMany(s => s.Chapters)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Page configuration
        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasIndex(e => new { e.ChapterId, e.PageNumber });
            entity.HasIndex(e => e.ChapterId);
            
            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.Pages)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReadingProgress configuration
        modelBuilder.Entity<ReadingProgress>(entity =>
        {
            entity.HasIndex(e => e.SeriesId).IsUnique();
            entity.HasIndex(e => e.LastReadAt);
            
            entity.HasOne(e => e.Series)
                .WithOne(s => s.ReadingProgress)
                .HasForeignKey<ReadingProgress>(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.LastReadChapter)
                .WithMany()
                .HasForeignKey(e => e.LastReadChapterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

---

## Common Queries

### Library View (with filters and sorting)

```csharp
public async Task<PaginatedResult<SeriesDto>> GetLibraryAsync(
    string? search = null,
    string? tag = null,
    SeriesStatus? status = null,
    bool? isFavorite = null,
    SortOption sort = SortOption.LastRead,
    int page = 1,
    int pageSize = 20)
{
    var query = _context.Series
        .Include(s => s.ReadingProgress)
        .AsNoTracking()
        .AsQueryable();

    // Filters
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(s => s.Title.Contains(search) || 
                                  (s.OriginalTitle != null && s.OriginalTitle.Contains(search)));
    
    if (status.HasValue)
        query = query.Where(s => s.Status == status.Value);
    
    if (isFavorite.HasValue)
        query = query.Where(s => s.IsFavorite == isFavorite.Value);

    // Tag filter (in-memory after load for SQLite JSON compatibility)
    var filtered = query;
    
    // Sorting
    query = sort switch
    {
        SortOption.LastRead => query.OrderByDescending(s => s.ReadingProgress != null ? s.ReadingProgress.LastReadAt : DateTime.MinValue),
        SortOption.LastUpdated => query.OrderByDescending(s => s.UpdatedAt),
        SortOption.Alphabetical => query.OrderBy(s => s.Title),
        SortOption.AlphabeticalDesc => query.OrderByDescending(s => s.Title),
        _ => query.OrderByDescending(s => s.UpdatedAt)
    };

    // Pagination
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    // Tag filter post-load
    if (!string.IsNullOrWhiteSpace(tag))
        items = items.Where(s => s.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();

    return new PaginatedResult<SeriesDto>(items.Select(MapToDto), totalCount, page, pageSize);
}
```

### Chapter List with Read Status

```csharp
public async Task<ChapterListResult> GetChaptersAsync(int seriesId)
{
    var series = await _context.Series
        .Include(s => s.ReadingProgress)
        .FirstOrDefaultAsync(s => s.Id == seriesId);
    
    if (series == null) return null;
    
    var chapters = await _context.Chapters
        .Where(c => c.SeriesId == seriesId)
        .OrderBy(c => c.Number)
        .AsNoTracking()
        .ToListAsync();
    
    var lastReadChapterId = series.ReadingProgress?.LastReadChapterId;
    var lastReadNumber = lastReadChapterId.HasValue 
        ? chapters.FirstOrDefault(c => c.Id == lastReadChapterId)?.Number 
        : null;
    
    return new ChapterListResult
    {
        Chapters = chapters.Select(c => new ChapterDto
        {
            Id = c.Id,
            Number = c.Number,
            Title = c.Title,
            IsRead = lastReadNumber.HasValue && c.Number <= lastReadNumber.Value
        }).ToList(),
        LastReadChapterId = lastReadChapterId
    };
}
```

### Reader - Get Chapter with Pages

```csharp
public async Task<ReaderData?> GetReaderDataAsync(string seriesSlug, decimal chapterNumber)
{
    var chapter = await _context.Chapters
        .Include(c => c.Series)
        .Include(c => c.Pages.OrderBy(p => p.PageNumber))
        .FirstOrDefaultAsync(c => c.Series.Slug == seriesSlug && c.Number == chapterNumber);
    
    if (chapter == null) return null;
    
    // Get prev/next chapter info
    var allChapters = await _context.Chapters
        .Where(c => c.SeriesId == chapter.SeriesId)
        .OrderBy(c => c.Number)
        .Select(c => new { c.Id, c.Number })
        .ToListAsync();
    
    var currentIndex = allChapters.FindIndex(c => c.Id == chapter.Id);
    
    return new ReaderData
    {
        SeriesTitle = chapter.Series.Title,
        SeriesSlug = chapter.Series.Slug,
        ChapterNumber = chapter.Number,
        ChapterTitle = chapter.Title,
        Pages = chapter.Pages.Select(p => new PageDto
        {
            PageNumber = p.PageNumber,
            ImageUrl = $"/content/{p.ImagePath}"
        }).ToList(),
        PreviousChapterNumber = currentIndex > 0 ? allChapters[currentIndex - 1].Number : null,
        NextChapterNumber = currentIndex < allChapters.Count - 1 ? allChapters[currentIndex + 1].Number : null
    };
}
```

### Continue Reading (Dashboard)

```csharp
public async Task<List<ContinueReadingDto>> GetContinueReadingAsync(int count = 10)
{
    return await _context.ReadingProgress
        .Include(rp => rp.Series)
        .Include(rp => rp.LastReadChapter)
        .OrderByDescending(rp => rp.LastReadAt)
        .Take(count)
        .Select(rp => new ContinueReadingDto
        {
            SeriesId = rp.SeriesId,
            SeriesTitle = rp.Series.Title,
            SeriesSlug = rp.Series.Slug,
            CoverImagePath = rp.Series.CoverImagePath,
            LastReadChapterNumber = rp.LastReadChapter.Number,
            LastReadAt = rp.LastReadAt
        })
        .AsNoTracking()
        .ToListAsync();
}
```

---

## Validation Rules

### Series
- `Title`: Required, 1-500 characters
- `Slug`: Required, 1-200 characters, must match `^[a-z0-9]+(-[a-z0-9]+)*$`
- `Slug`: Must be unique across all series
- `CoverImagePath`: If provided, should be valid relative path

### Chapter
- `Number`: Required, must be > 0
- `Number`: Must be unique within the series
- `SeriesId`: Must reference existing series

### Page
- `PageNumber`: Required, must be > 0
- `ImagePath`: Required, 1-1000 characters
- `ChapterId`: Must reference existing chapter

---

## Migration Strategy

1. **Initial Migration**: Create all tables with indexes
2. **Seed Data** (optional): Example series for testing
3. **Schema Updates**: Use EF Core migrations for any changes

```bash
# Create migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

---

## Data Integrity

- **Cascade Deletes**: Series → Chapters → Pages (automatic cleanup)
- **Unique Constraints**: Prevent duplicate slugs, chapter numbers
- **Required Fields**: Enforce at database level via NOT NULL
- **Index Strategy**: Optimized for common query patterns
