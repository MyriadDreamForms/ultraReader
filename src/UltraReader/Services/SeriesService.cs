using Microsoft.EntityFrameworkCore;
using UltraReader.Data;
using UltraReader.Entities;
using UltraReader.Exceptions;
using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service for series operations.
/// </summary>
public class SeriesService : ISeriesService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;
    private readonly ILogger<SeriesService> _logger;

    public SeriesService(AppDbContext context, IImageService imageService, ILogger<SeriesService> logger)
    {
        _context = context;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<PaginatedResult<SeriesCardDto>> GetLibraryAsync(LibraryFilter filter, int page, int pageSize)
    {
        var query = _context.Series
            .Include(s => s.ReadingProgress)
            .Include(s => s.Chapters)
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(s => s.Title.ToLower().Contains(search) ||
                                     (s.OriginalTitle != null && s.OriginalTitle.ToLower().Contains(search)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(s => s.Status == filter.Status.Value);
        }

        if (filter.FavoritesOnly == true)
        {
            query = query.Where(s => s.IsFavorite);
        }

        // Get data first for tag filtering (SQLite JSON limitation)
        var allSeries = await query.ToListAsync();

        // Apply tag filter in memory
        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            allSeries = allSeries.Where(s => 
                s.Tags.Any(t => t.Equals(filter.Tag, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Apply sorting
        allSeries = filter.Sort switch
        {
            SortOption.LastRead => allSeries
                .OrderByDescending(s => s.ReadingProgress?.LastReadAt ?? DateTime.MinValue)
                .ToList(),
            SortOption.LastUpdated => allSeries
                .OrderByDescending(s => s.UpdatedAt)
                .ToList(),
            SortOption.Alphabetical => allSeries
                .OrderBy(s => s.Title)
                .ToList(),
            SortOption.AlphabeticalDesc => allSeries
                .OrderByDescending(s => s.Title)
                .ToList(),
            _ => allSeries
                .OrderByDescending(s => s.UpdatedAt)
                .ToList()
        };

        var totalCount = allSeries.Count;

        // Pagination
        var items = allSeries
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToCardDto(s))
            .ToList();

        return new PaginatedResult<SeriesCardDto>(items, totalCount, page, pageSize);
    }

    public async Task<SeriesDetailDto?> GetSeriesDetailAsync(string slug)
    {
        var series = await _context.Series
            .Include(s => s.Chapters.OrderBy(c => c.Number))
            .ThenInclude(c => c.Pages)
            .Include(s => s.ReadingProgress)
            .ThenInclude(rp => rp!.LastReadChapter)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == slug);

        return series == null ? null : MapToDetailDto(series);
    }

    public async Task<SeriesDetailDto?> GetSeriesDetailByIdAsync(int id)
    {
        var series = await _context.Series
            .Include(s => s.Chapters.OrderBy(c => c.Number))
            .ThenInclude(c => c.Pages)
            .Include(s => s.ReadingProgress)
            .ThenInclude(rp => rp!.LastReadChapter)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return series == null ? null : MapToDetailDto(series);
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        var allSeries = await _context.Series
            .AsNoTracking()
            .ToListAsync();

        return allSeries
            .SelectMany(s => s.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<int> CreateSeriesAsync(CreateSeriesCommand command)
    {
        // Validate slug uniqueness
        var existingSlug = await _context.Series.AnyAsync(s => s.Slug == command.Slug);
        if (existingSlug)
        {
            throw new ValidationException("Slug", command.Slug, "Bu slug zaten kullanılıyor.");
        }

        var series = new Series
        {
            Title = command.Title,
            OriginalTitle = command.OriginalTitle,
            Slug = command.Slug,
            Description = command.Description,
            Status = command.Status,
            CoverImagePath = command.CoverImagePath,
            Tags = command.Tags,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Series.Add(series);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created series: {Title} ({Slug})", series.Title, series.Slug);
        return series.Id;
    }

    public async Task UpdateSeriesAsync(int id, UpdateSeriesCommand command)
    {
        var series = await _context.Series.FindAsync(id);
        if (series == null)
        {
            throw new NotFoundException("Series", id);
        }

        // Check slug uniqueness if changed
        if (series.Slug != command.Slug)
        {
            var existingSlug = await _context.Series.AnyAsync(s => s.Slug == command.Slug && s.Id != id);
            if (existingSlug)
            {
                throw new ValidationException("Slug", command.Slug, "Bu slug zaten kullanılıyor.");
            }
        }

        series.Title = command.Title;
        series.OriginalTitle = command.OriginalTitle;
        series.Slug = command.Slug;
        series.Description = command.Description;
        series.Status = command.Status;
        series.CoverImagePath = command.CoverImagePath;
        series.Tags = command.Tags;
        series.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated series: {Title} ({Id})", series.Title, series.Id);
    }

    public async Task DeleteSeriesAsync(int id)
    {
        var series = await _context.Series.FindAsync(id);
        if (series == null)
        {
            throw new NotFoundException("Series", id);
        }

        _context.Series.Remove(series);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted series: {Title} ({Id})", series.Title, series.Id);
    }

    public async Task ToggleFavoriteAsync(int id)
    {
        var series = await _context.Series.FindAsync(id);
        if (series == null)
        {
            throw new NotFoundException("Series", id);
        }

        series.IsFavorite = !series.IsFavorite;
        series.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Toggled favorite for series: {Title} ({Id}) -> {IsFavorite}", 
            series.Title, series.Id, series.IsFavorite);
    }

    private SeriesCardDto MapToCardDto(Series series)
    {
        var lastReadChapterNumber = series.ReadingProgress?.LastReadChapter?.Number;
        var unreadCount = lastReadChapterNumber.HasValue
            ? series.Chapters.Count(c => c.Number > lastReadChapterNumber.Value)
            : series.Chapters.Count;

        return new SeriesCardDto
        {
            Id = series.Id,
            Title = series.Title,
            Slug = series.Slug,
            CoverImagePath = series.CoverImagePath,
            CoverImageUrl = string.IsNullOrEmpty(series.CoverImagePath) 
                ? "/images/placeholder.png" 
                : _imageService.GetImageUrl(series.CoverImagePath),
            Status = series.Status,
            IsFavorite = series.IsFavorite,
            TotalChapters = series.Chapters.Count,
            UnreadChapterCount = unreadCount,
            LastReadAt = series.ReadingProgress?.LastReadAt,
            UpdatedAt = series.UpdatedAt
        };
    }

    private SeriesDetailDto MapToDetailDto(Series series)
    {
        var lastReadChapterNumber = series.ReadingProgress?.LastReadChapter?.Number;

        return new SeriesDetailDto
        {
            Id = series.Id,
            Title = series.Title,
            OriginalTitle = series.OriginalTitle,
            Slug = series.Slug,
            Description = series.Description,
            Status = series.Status,
            IsFavorite = series.IsFavorite,
            CoverImagePath = series.CoverImagePath,
            CoverImageUrl = string.IsNullOrEmpty(series.CoverImagePath)
                ? "/images/placeholder.png"
                : _imageService.GetImageUrl(series.CoverImagePath),
            Tags = series.Tags,
            Chapters = series.Chapters.Select(c => new ChapterListItemDto
            {
                Id = c.Id,
                Number = c.Number,
                Title = c.Title,
                IsRead = lastReadChapterNumber.HasValue && c.Number <= lastReadChapterNumber.Value,
                LastReadAt = c.Id == series.ReadingProgress?.LastReadChapterId 
                    ? series.ReadingProgress.LastReadAt 
                    : null,
                PageCount = c.Pages.Count,
                CreatedAt = c.CreatedAt
            }).ToList(),
            LastReadChapterId = series.ReadingProgress?.LastReadChapterId,
            LastReadChapterNumber = lastReadChapterNumber,
            CreatedAt = series.CreatedAt,
            UpdatedAt = series.UpdatedAt
        };
    }
}
