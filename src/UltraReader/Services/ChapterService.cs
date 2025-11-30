using Microsoft.EntityFrameworkCore;
using UltraReader.Data;
using UltraReader.Entities;
using UltraReader.Exceptions;
using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service for chapter operations including read status management.
/// </summary>
public class ChapterService : IChapterService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChapterService> _logger;

    public ChapterService(AppDbContext context, ILogger<ChapterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ChapterListItemDto>> GetChaptersAsync(int seriesId)
    {
        var series = await _context.Series
            .Include(s => s.Chapters.OrderBy(c => c.Number))
            .ThenInclude(c => c.Pages)
            .Include(s => s.ReadingProgress)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == seriesId);

        if (series == null)
        {
            throw new NotFoundException("Series", seriesId);
        }

        var lastReadChapter = series.ReadingProgress != null
            ? await _context.Chapters.FindAsync(series.ReadingProgress.LastReadChapterId)
            : null;

        return series.Chapters.Select(c => new ChapterListItemDto
        {
            Id = c.Id,
            Number = c.Number,
            Title = c.Title,
            IsRead = lastReadChapter != null && c.Number <= lastReadChapter.Number,
            LastReadAt = c.Id == series.ReadingProgress?.LastReadChapterId
                ? series.ReadingProgress.LastReadAt
                : null,
            PageCount = c.Pages.Count,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<ChapterEditDto?> GetChapterForEditAsync(int chapterId)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null) return null;

        return new ChapterEditDto
        {
            Id = chapter.Id,
            SeriesId = chapter.SeriesId,
            SeriesTitle = chapter.Series.Title,
            Number = chapter.Number,
            Title = chapter.Title,
            ReleaseDate = chapter.ReleaseDate
        };
    }

    public async Task<int> CreateChapterAsync(int seriesId, CreateChapterCommand command)
    {
        var series = await _context.Series.FindAsync(seriesId);
        if (series == null)
        {
            throw new NotFoundException("Series", seriesId);
        }

        // Check for duplicate chapter number
        var existingChapter = await _context.Chapters
            .AnyAsync(c => c.SeriesId == seriesId && c.Number == command.Number);
        if (existingChapter)
        {
            throw new ValidationException("Number", command.Number.ToString(), "Bu bölüm numarası zaten mevcut.");
        }

        var chapter = new Chapter
        {
            SeriesId = seriesId,
            Number = command.Number,
            Title = command.Title,
            ReleaseDate = command.ReleaseDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Chapters.Add(chapter);
        
        // Update series UpdatedAt
        series.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created chapter {Number} for series {SeriesId}", command.Number, seriesId);
        
        return chapter.Id;
    }

    public async Task UpdateChapterAsync(int chapterId, UpdateChapterCommand command)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        // Check for duplicate chapter number if changed
        if (chapter.Number != command.Number)
        {
            var existingChapter = await _context.Chapters
                .AnyAsync(c => c.SeriesId == chapter.SeriesId && c.Number == command.Number && c.Id != chapterId);
            if (existingChapter)
            {
                throw new ValidationException("Number", command.Number.ToString(), "Bu bölüm numarası zaten mevcut.");
            }
        }

        chapter.Number = command.Number;
        chapter.Title = command.Title;
        chapter.ReleaseDate = command.ReleaseDate;
        chapter.Series.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated chapter {ChapterId}", chapterId);
    }

    public async Task DeleteChapterAsync(int chapterId)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        _context.Chapters.Remove(chapter);
        chapter.Series.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted chapter {ChapterId}", chapterId);
    }

    public async Task MarkAsReadAsync(int chapterId)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .ThenInclude(s => s.ReadingProgress)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        // Get or create reading progress for this series
        var progress = chapter.Series.ReadingProgress;
        if (progress == null)
        {
            progress = new ReadingProgress
            {
                SeriesId = chapter.SeriesId,
                LastReadChapterId = chapterId,
                LastReadAt = DateTime.UtcNow,
                LastPageNumber = 1
            };
            _context.ReadingProgress.Add(progress);
        }
        else
        {
            // Only update if this chapter is later than current progress
            var currentChapter = await _context.Chapters.FindAsync(progress.LastReadChapterId);

            if (currentChapter == null || chapter.Number >= currentChapter.Number)
            {
                progress.LastReadChapterId = chapterId;
                progress.LastReadAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Marked chapter {ChapterId} as read", chapterId);
    }

    public async Task MarkAsUnreadAsync(int chapterId)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .ThenInclude(s => s.ReadingProgress)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        var progress = chapter.Series.ReadingProgress;
        if (progress != null)
        {
            var lastReadChapter = await _context.Chapters.FindAsync(progress.LastReadChapterId);
            
            // If marking a chapter as unread that is at or before the last read,
            // we need to set progress to the previous chapter
            if (lastReadChapter != null && chapter.Number <= lastReadChapter.Number)
            {
                // Find the chapter before this one
                var previousChapter = await _context.Chapters
                    .Where(c => c.SeriesId == chapter.SeriesId && c.Number < chapter.Number)
                    .OrderByDescending(c => c.Number)
                    .FirstOrDefaultAsync();

                if (previousChapter != null)
                {
                    progress.LastReadChapterId = previousChapter.Id;
                    progress.LastReadAt = DateTime.UtcNow;
                }
                else
                {
                    // No previous chapter, remove progress entirely
                    _context.ReadingProgress.Remove(progress);
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Marked chapter {ChapterId} as unread", chapterId);
    }

    public async Task<bool> ToggleReadStatusAsync(int chapterId)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .ThenInclude(s => s.ReadingProgress)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        var progress = chapter.Series.ReadingProgress;
        var lastReadChapter = progress != null
            ? await _context.Chapters.FindAsync(progress.LastReadChapterId)
            : null;

        // Check if chapter is currently read (its number is <= last read chapter number)
        bool isCurrentlyRead = lastReadChapter != null && chapter.Number <= lastReadChapter.Number;

        if (isCurrentlyRead)
        {
            await MarkAsUnreadAsync(chapterId);
            return false; // Now unread
        }
        else
        {
            await MarkAsReadAsync(chapterId);
            return true; // Now read
        }
    }
}
