using Microsoft.EntityFrameworkCore;
using UltraReader.Data;
using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service for dashboard operations.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(AppDbContext context, IImageService imageService, ILogger<DashboardService> logger)
    {
        _context = context;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<List<ContinueReadingDto>> GetContinueReadingAsync(int limit = 10)
    {
        var progressList = await _context.ReadingProgress
            .Include(rp => rp.Series)
            .ThenInclude(s => s.Chapters)
            .Include(rp => rp.LastReadChapter)
            .OrderByDescending(rp => rp.LastReadAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<ContinueReadingDto>();

        foreach (var progress in progressList)
        {
            var lastReadChapterNumber = progress.LastReadChapter.Number;
            var nextChapter = progress.Series.Chapters
                .Where(c => c.Number > lastReadChapterNumber)
                .OrderBy(c => c.Number)
                .FirstOrDefault();

            result.Add(new ContinueReadingDto
            {
                SeriesId = progress.SeriesId,
                SeriesTitle = progress.Series.Title,
                SeriesSlug = progress.Series.Slug,
                CoverImageUrl = string.IsNullOrEmpty(progress.Series.CoverImagePath)
                    ? "/images/placeholder.png"
                    : progress.Series.CoverImagePath.StartsWith("data:")
                        ? progress.Series.CoverImagePath
                        : _imageService.GetImageUrl(progress.Series.CoverImagePath),
                NextChapterNumber = nextChapter?.Number ?? lastReadChapterNumber,
                NextChapterId = nextChapter?.Id,
                LastReadAt = progress.LastReadAt
            });
        }

        return result;
    }

    public async Task<List<RecentlyUpdatedDto>> GetRecentlyUpdatedAsync(int limit = 10)
    {
        var recentSeries = await _context.Series
            .Include(s => s.Chapters)
            .Where(s => s.Chapters.Any())
            .OrderByDescending(s => s.UpdatedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return recentSeries.Select(s => new RecentlyUpdatedDto
        {
            SeriesId = s.Id,
            SeriesTitle = s.Title,
            SeriesSlug = s.Slug,
            CoverImageUrl = string.IsNullOrEmpty(s.CoverImagePath)
                ? "/images/placeholder.png"
                : s.CoverImagePath.StartsWith("data:")
                    ? s.CoverImagePath
                    : _imageService.GetImageUrl(s.CoverImagePath),
            LatestChapterNumber = s.Chapters.Max(c => c.Number),
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }
}
