using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using ultraReader.Services;
using System.Collections.Generic;
using ultraReader.Models.DTOs;
using System.Linq;

namespace ultraReader.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserPreferencesService _userPreferencesService;
        private readonly IWebtoonService _webtoonService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(
            IUserPreferencesService userPreferencesService,
            IWebtoonService webtoonService,
            UserManager<IdentityUser> userManager)
        {
            _userPreferencesService = userPreferencesService;
            _webtoonService = webtoonService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Favorites()
        {
            var userId = _userManager.GetUserId(User);
            var favorites = await _userPreferencesService.GetUserFavoritesAsync(userId);
            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorites(string webtoonId, string? returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            await _userPreferencesService.AddToFavoritesAsync(userId, webtoonId);
            
            TempData["Message"] = "Webtoon favorilere eklendi.";
            TempData["MessageType"] = "success";
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Favorites));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites(string webtoonId, string? returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            await _userPreferencesService.RemoveFromFavoritesAsync(userId, webtoonId);
            
            TempData["Message"] = "Webtoon favorilerden kaldırıldı.";
            TempData["MessageType"] = "warning";
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Favorites));
        }

        public async Task<IActionResult> ReadingHistory()
        {
            try 
            {
                var userId = _userManager.GetUserId(User);
                var history = await _userPreferencesService.GetReadingHistoryAsync(userId);
                
                // Okuma geçmişi için gerekli webtoon bilgilerini getirelim
                var readingHistoryViewModel = new List<ReadingHistoryViewModel>();
                
                foreach (var item in history)
                {
                    var webtoonInfo = await _webtoonService.GetWebtoonByIdAsync(item.WebtoonId);
                    if (webtoonInfo != null)
                    {
                        readingHistoryViewModel.Add(new ReadingHistoryViewModel
                        {
                            ReadingListItem = item,
                            Webtoon = Models.DTOs.Webtoon.FromWebtoonInfo(webtoonInfo)
                        });
                    }
                }
                
                return View(readingHistoryViewModel);
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste dön
                TempData["Message"] = "Okuma geçmişi yüklenirken bir hata oluştu.";
                TempData["MessageType"] = "danger";
                return View(new List<ReadingHistoryViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReadingProgress(string webtoonId, string chapterId, int lastReadPage, bool isCompleted)
        {
            var userId = _userManager.GetUserId(User);
            await _userPreferencesService.UpdateReadingProgressAsync(userId, webtoonId, chapterId, lastReadPage, isCompleted);
            
            return Json(new { success = true });
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Profil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Tercihler()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcı tercihlerini getir
            var preferences = await _userPreferencesService.GetUserPreferencesAsync(user.Id);
            
            // Tür listesini getir
            var genres = await _webtoonService.GetAllGenresAsync();

            // Favori webtoonlar ve okuma listesini getir
            var favoriteWebtoons = await _userPreferencesService.GetFavoriteWebtoonsAsync(user.Id);
            var readingList = await _userPreferencesService.GetReadingListAsync(user.Id);

            var model = new UserPreferencesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                
                // Görünüm tercihleri
                Theme = preferences.Theme ?? "light",
                ReadingDirection = preferences.ReadingDirection ?? "vertical",
                PageSize = preferences.PageSize,
                AutoScroll = preferences.AutoScroll,
                AutoScrollSpeed = preferences.AutoScrollSpeed,
                
                // Bildirim tercihleri
                EmailNotifications = preferences.EmailNotifications,
                NewChapterNotifications = preferences.NewChapterNotifications,
                CommentNotifications = preferences.CommentNotifications,
                
                // Favori webtoonlar ve okuma listesi
                FavoriteWebtoons = favoriteWebtoons,
                ReadingList = readingList,
                
                // Webtoon tercihleri
                PreferredGenres = preferences.PreferredGenres ?? new List<string>(),
                HideCompletedWebtoons = preferences.HideCompletedWebtoons
            };

            ViewBag.AvailableGenres = genres;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tercihler(UserPreferencesViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kullanıcı tercihlerini güncelle
                var preferences = await _userPreferencesService.GetUserPreferencesAsync(user.Id);
                
                // Görünüm tercihleri
                preferences.Theme = model.Theme;
                preferences.ReadingDirection = model.ReadingDirection;
                preferences.PageSize = model.PageSize;
                preferences.AutoScroll = model.AutoScroll;
                preferences.AutoScrollSpeed = model.AutoScrollSpeed;
                
                // Bildirim tercihleri
                preferences.EmailNotifications = model.EmailNotifications;
                preferences.NewChapterNotifications = model.NewChapterNotifications;
                preferences.CommentNotifications = model.CommentNotifications;
                
                // Webtoon tercihleri
                preferences.PreferredGenres = model.PreferredGenres;
                preferences.HideCompletedWebtoons = model.HideCompletedWebtoons;
                
                // Tercihleri kaydet
                await _userPreferencesService.UpdateUserPreferencesAsync(preferences);
                
                TempData["Message"] = "Tercihleriniz başarıyla güncellendi.";
                TempData["MessageType"] = "success";
                
                return RedirectToAction(nameof(Tercihler));
            }

            // Tür listesini getir
            var genres = await _webtoonService.GetAllGenresAsync();
            ViewBag.AvailableGenres = genres;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TemaEkle(string theme)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcı tercihlerini getir
            var preferences = await _userPreferencesService.GetUserPreferencesAsync(user.Id);
            
            // Temayı güncelle
            preferences.Theme = theme;
            
            // Tercihleri kaydet
            await _userPreferencesService.UpdateUserPreferencesAsync(preferences);
            
            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FavoriEkle(string webtoonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await _userPreferencesService.AddFavoriteWebtoonAsync(user.Id, webtoonId);
            
            return RedirectToAction("Detay", "Webtoon", new { id = webtoonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FavoriKaldir(string webtoonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await _userPreferencesService.RemoveFavoriteWebtoonAsync(user.Id, webtoonId);
            
            return RedirectToAction(nameof(Tercihler));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OkumaListesineEkle(string webtoonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await _userPreferencesService.AddToReadingListAsync(user.Id, webtoonId);
            
            return RedirectToAction("Detay", "Webtoon", new { id = webtoonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OkumaListesindenKaldir(string webtoonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await _userPreferencesService.RemoveFromReadingListAsync(user.Id, webtoonId);
            
            return RedirectToAction(nameof(Tercihler));
        }
    }
} 