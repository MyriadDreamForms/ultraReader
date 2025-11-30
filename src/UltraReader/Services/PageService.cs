using Microsoft.EntityFrameworkCore;
using UltraReader.Data;
using UltraReader.Entities;
using UltraReader.Exceptions;
using UltraReader.Services.DTOs;

namespace UltraReader.Services;

/// <summary>
/// Service for page operations.
/// </summary>
public class PageService : IPageService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;
    private readonly IFolderImportService _folderImportService;
    private readonly ILogger<PageService> _logger;

    public PageService(
        AppDbContext context, 
        IImageService imageService,
        IFolderImportService folderImportService,
        ILogger<PageService> logger)
    {
        _context = context;
        _imageService = imageService;
        _folderImportService = folderImportService;
        _logger = logger;
    }

    public async Task<List<PageDto>> GetPagesAsync(int chapterId)
    {
        var pages = await _context.Pages
            .Where(p => p.ChapterId == chapterId)
            .OrderBy(p => p.PageNumber)
            .AsNoTracking()
            .ToListAsync();

        return pages.Select(p => new PageDto
        {
            Id = p.Id,
            PageNumber = p.PageNumber,
            ImagePath = p.ImagePath,
            ImageUrl = _imageService.GetImageUrl(p.ImagePath)
        }).ToList();
    }

    public async Task<int> AddPageAsync(int chapterId, AddPageCommand command)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Series)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        var page = new Page
        {
            ChapterId = chapterId,
            PageNumber = command.PageNumber,
            ImagePath = command.ImagePath
        };

        _context.Pages.Add(page);
        chapter.Series.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Added page {PageNumber} to chapter {ChapterId}", command.PageNumber, chapterId);

        return page.Id;
    }

    public async Task UpdatePageOrderAsync(int chapterId, List<int> pageIds)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Pages)
            .Include(c => c.Series)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        for (int i = 0; i < pageIds.Count; i++)
        {
            var page = chapter.Pages.FirstOrDefault(p => p.Id == pageIds[i]);
            if (page != null)
            {
                page.PageNumber = i + 1;
            }
        }

        chapter.Series.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Reordered {Count} pages in chapter {ChapterId}", pageIds.Count, chapterId);
    }

    public async Task DeletePageAsync(int pageId)
    {
        var page = await _context.Pages
            .Include(p => p.Chapter)
            .ThenInclude(c => c.Series)
            .FirstOrDefaultAsync(p => p.Id == pageId);

        if (page == null)
        {
            throw new NotFoundException("Page", pageId);
        }

        var chapterId = page.ChapterId;
        var deletedPageNumber = page.PageNumber;

        _context.Pages.Remove(page);
        page.Chapter.Series.UpdatedAt = DateTime.UtcNow;

        // Reorder remaining pages
        var remainingPages = await _context.Pages
            .Where(p => p.ChapterId == chapterId && p.PageNumber > deletedPageNumber)
            .ToListAsync();

        foreach (var p in remainingPages)
        {
            p.PageNumber--;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted page {PageId} and reordered remaining pages", pageId);
    }

    public async Task<FolderImportResult> ImportFromFolderAsync(int chapterId, string folderPath)
    {
        var chapter = await _context.Chapters
            .Include(c => c.Pages)
            .Include(c => c.Series)
            .FirstOrDefaultAsync(c => c.Id == chapterId);

        if (chapter == null)
        {
            throw new NotFoundException("Chapter", chapterId);
        }

        if (!_folderImportService.IsPathWithinContentRoot(folderPath))
        {
            return new FolderImportResult
            {
                Errors = ["Belirtilen klasör izin verilen içerik kökü dışında."]
            };
        }

        var imageFiles = _folderImportService.GetImageFilesFromFolder(folderPath);
        if (imageFiles.Count == 0)
        {
            return new FolderImportResult
            {
                Errors = ["Klasörde görsel dosyası bulunamadı."]
            };
        }

        var existingPaths = chapter.Pages.Select(p => p.ImagePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var startPageNumber = chapter.Pages.Count > 0 ? chapter.Pages.Max(p => p.PageNumber) + 1 : 1;

        var importedCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        foreach (var filePath in imageFiles)
        {
            var relativePath = _folderImportService.GetRelativePath(filePath);

            if (existingPaths.Contains(relativePath))
            {
                skippedCount++;
                continue;
            }

            try
            {
                var page = new Page
                {
                    ChapterId = chapterId,
                    PageNumber = startPageNumber++,
                    ImagePath = relativePath
                };

                _context.Pages.Add(page);
                importedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"'{relativePath}' içe aktarılamadı: {ex.Message}");
            }
        }

        if (importedCount > 0)
        {
            chapter.Series.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Imported {ImportedCount} pages, skipped {SkippedCount} for chapter {ChapterId}",
            importedCount, skippedCount, chapterId);

        return new FolderImportResult
        {
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            Errors = errors
        };
    }
}
