using Microsoft.EntityFrameworkCore;
using UltraReader.Data;
using UltraReader.Entities;
using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service for reader functionality.
/// </summary>
public class ReaderService : IReaderService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;
    private readonly ILogger<ReaderService> _logger;

    public ReaderService(AppDbContext context, IImageService imageService, ILogger<ReaderService> logger)
    {
        _context = context;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<ReaderDataDto?> GetReaderDataAsync(string seriesSlug, decimal chapterNumber)
    {
        // Get chapter with series and pages
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .Include(c => c.Pages.OrderBy(p => p.PageNumber).ThenBy(p => p.Id))
            .FirstOrDefaultAsync(c => c.Series.Slug == seriesSlug && c.Number == chapterNumber);

        if (chapter == null)
        {
            _logger.LogWarning("Chapter not found: {Slug} #{Number}", seriesSlug, chapterNumber);
            return null;
        }

        // Get all chapter numbers for prev/next navigation
        var allChapterNumbers = await _context.Chapters
            .Where(c => c.SeriesId == chapter.SeriesId)
            .OrderBy(c => c.Number)
            .Select(c => c.Number)
            .ToListAsync();

        var currentIndex = allChapterNumbers.IndexOf(chapterNumber);

        return new ReaderDataDto
        {
            SeriesTitle = chapter.Series.Title,
            SeriesSlug = chapter.Series.Slug,
            SeriesId = chapter.Series.Id,
            ChapterNumber = chapter.Number,
            ChapterTitle = chapter.Title,
            ChapterId = chapter.Id,
            Pages = chapter.Pages.Select(p => new ReaderPageDto
            {
                PageNumber = p.PageNumber,
                ImageUrl = _imageService.GetImageUrl(p.ImagePath)
            }).ToList(),
            PreviousChapterNumber = currentIndex > 0 ? allChapterNumbers[currentIndex - 1] : null,
            NextChapterNumber = currentIndex < allChapterNumbers.Count - 1 ? allChapterNumbers[currentIndex + 1] : null
        };
    }

    public async Task UpdateReadingProgressAsync(string seriesSlug, decimal chapterNumber)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .ThenInclude(s => s.ReadingProgress)
            .FirstOrDefaultAsync(c => c.Series.Slug == seriesSlug && c.Number == chapterNumber);

        if (chapter == null)
        {
            _logger.LogWarning("Cannot update progress - chapter not found: {Slug} #{Number}", seriesSlug, chapterNumber);
            return;
        }

        var series = chapter.Series;
        var progress = series.ReadingProgress;

        if (progress == null)
        {
            // Create new progress
            progress = new ReadingProgress
            {
                SeriesId = series.Id,
                LastReadChapterId = chapter.Id,
                LastReadAt = DateTime.UtcNow
            };
            _context.ReadingProgress.Add(progress);

            // Update series status to Reading if not started
            if (series.Status == SeriesStatus.NotStarted)
            {
                series.Status = SeriesStatus.Reading;
            }
        }
        else
        {
            // Update existing progress
            progress.LastReadChapterId = chapter.Id;
            progress.LastReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated reading progress: {Slug} #{Number}", seriesSlug, chapterNumber);
    }
}
