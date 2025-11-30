# API Contracts: Webtoon/Manga Reader Web Application

**Feature**: 001-webtoon-manga-reader  
**Date**: 2024-11-30  
**Type**: Blazor Server Internal Routes

> **Note**: Since this is a Blazor Server application, there are no external REST APIs. 
> This document defines the internal page routes and component contracts.

---

## Page Routes

### Public Pages (Reader)

| Route | Page | Description |
|-------|------|-------------|
| `/` | Index.razor | Dashboard with continue reading, redirects to /library if empty |
| `/library` | Library.razor | Series grid with search, filter, sort |
| `/series/{slug}` | SeriesDetail.razor | Series info + chapter list |
| `/reader/{seriesSlug}/{chapterNumber:decimal}` | Reader.razor | Webtoon reader |

### Admin Pages (Management)

| Route | Page | Description |
|-------|------|-------------|
| `/admin/series` | Admin/SeriesManagement.razor | Series list with CRUD actions |
| `/admin/series/new` | Admin/SeriesEdit.razor | Create new series |
| `/admin/series/{id:int}/edit` | Admin/SeriesEdit.razor | Edit existing series |
| `/admin/series/{seriesId:int}/chapters` | Admin/ChapterManagement.razor | Chapters for a series |
| `/admin/chapters/{chapterId:int}/pages` | Admin/PageManagement.razor | Pages for a chapter |
| `/admin/settings` | Admin/Settings.razor | App configuration |

---

## Component Contracts

### SeriesCard Component

**File**: `Components/SeriesCard.razor`

**Parameters**:
```csharp
[Parameter] public required SeriesCardDto Series { get; set; }
[Parameter] public EventCallback<int> OnClick { get; set; }
```

**DTO**:
```csharp
public record SeriesCardDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? CoverImagePath { get; init; }
    public SeriesStatus Status { get; init; }
    public bool IsFavorite { get; init; }
    public int UnreadChapterCount { get; init; }
}
```

---

### ChapterListItem Component

**File**: `Components/ChapterListItem.razor`

**Parameters**:
```csharp
[Parameter] public required ChapterListItemDto Chapter { get; set; }
[Parameter] public EventCallback<int> OnReadClick { get; set; }
[Parameter] public EventCallback<int> OnToggleRead { get; set; }
```

**DTO**:
```csharp
public record ChapterListItemDto
{
    public int Id { get; init; }
    public decimal Number { get; init; }
    public string? Title { get; init; }
    public bool IsRead { get; init; }
    public DateTime? LastReadAt { get; init; }
}
```

---

### Pagination Component

**File**: `Components/Pagination.razor`

**Parameters**:
```csharp
[Parameter] public int CurrentPage { get; set; } = 1;
[Parameter] public int TotalPages { get; set; }
[Parameter] public int PageSize { get; set; } = 20;
[Parameter] public EventCallback<int> OnPageChanged { get; set; }
```

---

### SearchFilter Component

**File**: `Components/SearchFilter.razor`

**Parameters**:
```csharp
[Parameter] public string? SearchTerm { get; set; }
[Parameter] public string? SelectedTag { get; set; }
[Parameter] public SeriesStatus? SelectedStatus { get; set; }
[Parameter] public bool? FavoritesOnly { get; set; }
[Parameter] public SortOption SortBy { get; set; } = SortOption.LastRead;
[Parameter] public EventCallback<FilterChangedEventArgs> OnFilterChanged { get; set; }
```

**Event Args**:
```csharp
public record FilterChangedEventArgs
{
    public string? SearchTerm { get; init; }
    public string? Tag { get; init; }
    public SeriesStatus? Status { get; init; }
    public bool? FavoritesOnly { get; init; }
    public SortOption SortBy { get; init; }
}
```

---

### ConfirmDialog Component

**File**: `Components/ConfirmDialog.razor`

**Parameters**:
```csharp
[Parameter] public string Title { get; set; } = "Confirm";
[Parameter] public string Message { get; set; } = "Are you sure?";
[Parameter] public string ConfirmText { get; set; } = "Confirm";
[Parameter] public string CancelText { get; set; } = "Cancel";
[Parameter] public EventCallback OnConfirm { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

**Methods**:
```csharp
public void Show();
public void Hide();
```

---

### ImagePlaceholder Component

**File**: `Components/ImagePlaceholder.razor`

**Parameters**:
```csharp
[Parameter] public string? ImageUrl { get; set; }
[Parameter] public string AltText { get; set; } = "Image";
[Parameter] public string PlaceholderUrl { get; set; } = "/images/placeholder.png";
[Parameter] public string? CssClass { get; set; }
```

---

## Service Contracts

### ISeriesService

```csharp
public interface ISeriesService
{
    // Queries
    Task<PaginatedResult<SeriesCardDto>> GetLibraryAsync(LibraryFilter filter, int page, int pageSize);
    Task<SeriesDetailDto?> GetSeriesDetailAsync(string slug);
    Task<List<string>> GetAllTagsAsync();
    
    // Commands
    Task<int> CreateSeriesAsync(CreateSeriesCommand command);
    Task UpdateSeriesAsync(int id, UpdateSeriesCommand command);
    Task DeleteSeriesAsync(int id);
    Task ToggleFavoriteAsync(int id);
}
```

**DTOs**:
```csharp
public record LibraryFilter(
    string? Search,
    string? Tag,
    SeriesStatus? Status,
    bool? FavoritesOnly,
    SortOption Sort
);

public record SeriesDetailDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? OriginalTitle { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public SeriesStatus Status { get; init; }
    public bool IsFavorite { get; init; }
    public string? CoverImagePath { get; init; }
    public List<string> Tags { get; init; } = new();
    public List<ChapterListItemDto> Chapters { get; init; } = new();
    public int? LastReadChapterId { get; init; }
}

public record CreateSeriesCommand(
    string Title,
    string? OriginalTitle,
    string Slug,
    string? Description,
    SeriesStatus Status,
    string? CoverImagePath,
    List<string> Tags
);

public record UpdateSeriesCommand(
    string Title,
    string? OriginalTitle,
    string Slug,
    string? Description,
    SeriesStatus Status,
    string? CoverImagePath,
    List<string> Tags
);
```

---

### IChapterService

```csharp
public interface IChapterService
{
    // Queries
    Task<List<ChapterListItemDto>> GetChaptersAsync(int seriesId);
    Task<ChapterEditDto?> GetChapterForEditAsync(int chapterId);
    
    // Commands
    Task<int> CreateChapterAsync(int seriesId, CreateChapterCommand command);
    Task UpdateChapterAsync(int chapterId, UpdateChapterCommand command);
    Task DeleteChapterAsync(int chapterId);
    Task MarkAsReadAsync(int chapterId);
    Task MarkAsUnreadAsync(int chapterId);
}
```

**DTOs**:
```csharp
public record ChapterEditDto
{
    public int Id { get; init; }
    public int SeriesId { get; init; }
    public decimal Number { get; init; }
    public string? Title { get; init; }
    public DateTime? ReleaseDate { get; init; }
}

public record CreateChapterCommand(
    decimal Number,
    string? Title,
    DateTime? ReleaseDate
);

public record UpdateChapterCommand(
    decimal Number,
    string? Title,
    DateTime? ReleaseDate
);
```

---

### IPageService

```csharp
public interface IPageService
{
    // Queries
    Task<List<PageDto>> GetPagesAsync(int chapterId);
    
    // Commands
    Task<int> AddPageAsync(int chapterId, AddPageCommand command);
    Task UpdatePageOrderAsync(int pageId, int newPageNumber);
    Task DeletePageAsync(int pageId);
    Task<int> ImportFromFolderAsync(int chapterId, string relativeFolderPath);
}
```

**DTOs**:
```csharp
public record PageDto
{
    public int Id { get; init; }
    public int PageNumber { get; init; }
    public required string ImagePath { get; init; }
    public string ImageUrl => $"/content/{ImagePath}";
    public bool ImageExists { get; init; }
}

public record AddPageCommand(
    int PageNumber,
    string ImagePath
);
```

---

### IReaderService

```csharp
public interface IReaderService
{
    Task<ReaderDataDto?> GetReaderDataAsync(string seriesSlug, decimal chapterNumber);
    Task UpdateReadingProgressAsync(string seriesSlug, decimal chapterNumber);
}
```

**DTOs**:
```csharp
public record ReaderDataDto
{
    public required string SeriesTitle { get; init; }
    public required string SeriesSlug { get; init; }
    public decimal ChapterNumber { get; init; }
    public string? ChapterTitle { get; init; }
    public List<ReaderPageDto> Pages { get; init; } = new();
    public decimal? PreviousChapterNumber { get; init; }
    public decimal? NextChapterNumber { get; init; }
}

public record ReaderPageDto
{
    public int PageNumber { get; init; }
    public required string ImageUrl { get; init; }
}
```

---

### IDashboardService

```csharp
public interface IDashboardService
{
    Task<List<ContinueReadingDto>> GetContinueReadingAsync(int count = 10);
    Task<List<RecentlyUpdatedDto>> GetRecentlyUpdatedAsync(int count = 10);
}
```

**DTOs**:
```csharp
public record ContinueReadingDto
{
    public int SeriesId { get; init; }
    public required string SeriesTitle { get; init; }
    public required string SeriesSlug { get; init; }
    public string? CoverImagePath { get; init; }
    public decimal LastReadChapterNumber { get; init; }
    public decimal? NextChapterNumber { get; init; }
    public DateTime LastReadAt { get; init; }
}

public record RecentlyUpdatedDto
{
    public int SeriesId { get; init; }
    public required string SeriesTitle { get; init; }
    public required string SeriesSlug { get; init; }
    public string? CoverImagePath { get; init; }
    public int NewChapterCount { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

---

### ISettingsService

```csharp
public interface ISettingsService
{
    Task<AppSettingsDto> GetSettingsAsync();
    Task<bool> ValidateContentRootAsync(string path);
    Task UpdateContentRootAsync(string path);
}
```

**DTOs**:
```csharp
public record AppSettingsDto
{
    public required string ContentRootPath { get; init; }
    public bool ContentRootExists { get; init; }
}
```

---

### IImageService

```csharp
public interface IImageService
{
    bool ValidateImagePath(string relativePath);
    string GetFullPath(string relativePath);
    string GetImageUrl(string relativePath);
    IEnumerable<string> GetSupportedExtensions();
}
```

---

### IFolderImportService

```csharp
public interface IFolderImportService
{
    Task<List<string>> ScanFolderAsync(string relativeFolderPath);
    Task<bool> FolderExistsAsync(string relativeFolderPath);
}
```

---

## Shared Types

### PaginatedResult<T>

```csharp
public record PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

### SortOption

```csharp
public enum SortOption
{
    LastRead,
    LastUpdated,
    Alphabetical,
    AlphabeticalDesc
}
```

---

## Static File Routes

| Route Pattern | Source | Description |
|---------------|--------|-------------|
| `/content/{**path}` | Configurable content root | Webtoon images |
| `/_content/*` | wwwroot | App static assets |
| `/images/placeholder.png` | wwwroot/images | Missing image placeholder |
| `/css/app.css` | wwwroot/css | Main stylesheet |
| `/css/reader.css` | wwwroot/css | Reader-specific styles |

---

## Error Responses

All services throw exceptions for error conditions:

| Exception | HTTP Equivalent | Usage |
|-----------|-----------------|-------|
| `NotFoundException` | 404 | Entity not found |
| `ValidationException` | 400 | Invalid input (duplicate slug, etc.) |
| `InvalidOperationException` | 400 | Business rule violation |
| `DirectoryNotFoundException` | 400 | Folder import path invalid |

Blazor pages catch these and display appropriate error messages.
