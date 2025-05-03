using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ultraReader.Services;
using ultraReader.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ultraReader.Models;
using ultraReader.Data;
using ultraReader.Models.Entities;
using ultraReader.Models.Enums;

namespace ultraReader.Controllers
{
    public class ReaderController : Controller
    {
        private readonly IWebtoonService _webtoonService;
        private readonly IUserPreferencesService _userPreferencesService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ReaderController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IPreferencesService _preferencesService;

        public ReaderController(
            IWebtoonService webtoonService, 
            IUserPreferencesService userPreferencesService, 
            UserManager<IdentityUser> userManager,
            ILogger<ReaderController> logger,
            ApplicationDbContext context,
            IPreferencesService preferencesService)
        {
            _webtoonService = webtoonService;
            _userPreferencesService = userPreferencesService;
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _preferencesService = preferencesService;
        }

        [HttpGet]
        [Route("{name}/{chapter}")]
        public async Task<IActionResult> Read(string name, string chapter, int? page = null, ReadingMode? mode = null)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(chapter))
            {
                return NotFound();
            }

            try
            {
                var viewModel = await _webtoonService.GetChapterImagesAsync(name, chapter);
                if (viewModel == null)
                {
                    return NotFound();
                }

                if (mode.HasValue)
                {
                    viewModel.ReadingMode = mode.Value;
                }
                else
                {
                    // Kullanıcı tercihlerini al
                    if (User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        viewModel.ReadingMode = await _preferencesService.GetUserReadingModeAsync(User.Identity.Name);
                    }
                }

                // Kullanıcı ilerleme durumunu kontrol et
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var history = await _context.ReadingList
                        .FirstOrDefaultAsync(r => r.UserId == User.Identity.Name &&
                                            r.WebtoonId == viewModel.WebtoonInfo.Id &&
                                            r.CurrentChapterId == chapter);

                    if (history != null)
                    {
                        if (page.HasValue && page.Value >= 1 && page.Value <= viewModel.Images.Count)
                        {
                            viewModel.CurrentPage = page.Value;
                            viewModel.HasSavedPosition = false;
                        }
                        else
                        {
                            viewModel.CurrentPage = history.CurrentPage;
                            viewModel.HasSavedPosition = true;
                        }
                    }
                    else if (page.HasValue && page.Value >= 1 && page.Value <= viewModel.Images.Count)
                    {
                        viewModel.CurrentPage = page.Value;
                    }
                    else
                    {
                        viewModel.CurrentPage = 1;
                    }
                }
                else if (page.HasValue && page.Value >= 1 && page.Value <= viewModel.Images.Count)
                {
                    viewModel.CurrentPage = page.Value;
                }
                else
                {
                    viewModel.CurrentPage = 1;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hata: Bölüm okuma sayfası gösterilirken bir sorun oluştu: {Chapter} - {Name}", chapter, name);
                return View("Error", new ExtendedErrorViewModel { Message = "Bölüm görüntülenirken bir hata oluştu." });
            }
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveProgress(string webtoonId, string chapterId, int pageNumber, bool isCompleted = false)
        {
            try
            {
                if (string.IsNullOrEmpty(webtoonId) || string.IsNullOrEmpty(chapterId) || pageNumber < 1)
                {
                    return BadRequest();
                }

                // Okuma ilerlemesini kaydet
                var userId = User.Identity?.Name ?? string.Empty;
                
                // Mevcut kayıt var mı diye kontrol et
                var existingProgress = await _context.ReadingList
                    .FirstOrDefaultAsync(rl => rl.UserId == userId && 
                                               rl.WebtoonId == webtoonId && 
                                               rl.CurrentChapterId == chapterId);
                
                if (existingProgress != null)
                {
                    // Mevcut kaydı güncelle
                    existingProgress.CurrentPage = pageNumber;
                    existingProgress.IsCompleted = isCompleted;
                    existingProgress.UpdatedAt = DateTime.UtcNow;
                    
                    _context.ReadingList.Update(existingProgress);
                }
                else
                {
                    // Yeni kayıt oluştur
                    var readingListItem = new ReadingListItem
                    {
                        UserId = userId,
                        WebtoonId = webtoonId,
                        CurrentChapterId = chapterId,
                        CurrentPage = pageNumber,
                        IsCompleted = isCompleted,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _context.ReadingList.Add(readingListItem);
                }
                
                await _context.SaveChangesAsync();
                
                // Ajax çağrısı için JSON yanıtı
                return Json(new { success = true, message = "İlerleme kaydedildi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Okuma ilerlemesi kaydedilirken hata oluştu: WebtoonId={WebtoonId}, ChapterId={ChapterId}, Page={Page}", webtoonId, chapterId, pageNumber);
                return Json(new { success = false, message = "İlerleme kaydedilemedi" });
            }
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeReadingMode(string name, string chapter, int page, ReadingMode mode)
        {
            try
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(chapter))
                {
                    return BadRequest();
                }

                // Kullanıcı tercihlerini güncelle
                var userId = User.Identity?.Name ?? string.Empty;
                var preferences = await _preferencesService.GetUserPreferencesAsync(userId);
                
                if (preferences != null)
                {
                    // PreferencesService üzerinden dönüşüm yap
                    preferences.ReadingDirection = _preferencesService.ConvertReadingModeToDirection(mode);
                    await _preferencesService.UpdateUserPreferencesAsync(preferences);
                }
                
                // Aynı okuma sayfasına yönlendir
                return RedirectToAction("Read", new { name, chapter, page, mode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Okuma modu değiştirilirken hata oluştu: Name={Name}, Chapter={Chapter}, Mode={Mode}", name, chapter, mode);
                return View("Error", new ExtendedErrorViewModel { Message = "Okuma modu değiştirilirken bir hata oluştu." });
            }
        }
    }
    
    // Sayısal ve metinsel sıralama için karşılaştırıcı sınıf
    public class NumericAndTextComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            // Null kontrolleri
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Sayısal karşılaştırma
            if (int.TryParse(x, out int numX) && int.TryParse(y, out int numY))
                return numX.CompareTo(numY);

            // Karışık tip karşılaştırma
            if (int.TryParse(x, out _))
                return -1;
            if (int.TryParse(y, out _))
                return 1;

            // Metin karşılaştırma
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
} 