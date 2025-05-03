using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ultraReader.Models.DTOs;
using ultraReader.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using ultraReader.Models.Entities;
using System.Linq;
using ultraReader.Data;
using Microsoft.AspNetCore.Hosting;

namespace ultraReader.Controllers
{
    // HTTP İstek Uzantı Metodu
    public static class HttpRequestExtensions
    {
        public static IList<IFormFile> GetFiles(this IFormFileCollection collection, string key)
        {
            return collection.Where(x => x.Name == key).ToList();
        }
    }

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IWebtoonService _webtoonService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICommentService _commentService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(
            IWebtoonService webtoonService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICommentService commentService,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _webtoonService = webtoonService;
            _userManager = userManager;
            _roleManager = roleManager;
            _commentService = commentService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardViewModel = new AdminDashboardViewModel
            {
                TotalWebtoons = await _webtoonService.GetTotalWebtoonCountAsync(),
                TotalUsers = _userManager.Users.Count(),
                TotalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count,
                TotalModerators = (await _userManager.GetUsersInRoleAsync("Moderator")).Count,
                TotalComments = await _commentService.GetTotalCommentCountAsync(),
                PendingComments = await _commentService.GetPendingCommentCountAsync(),
                RecentWebtoons = (await _webtoonService.GetLatestWebtoonsAsync(5))
                             .Select(w => Models.DTOs.Webtoon.FromWebtoonInfo(w)).ToList(),
                LatestUsers = _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToList(),
                
                // Grafik verileri
                WebtoonsByGenre = await _webtoonService.GetWebtoonsByGenreAsync(),
                WebtoonsByStatus = await _webtoonService.GetWebtoonsByStatusAsync(),
                CommentsPerDay = await _commentService.GetCommentsPerDayAsync(7),
                NewUsersPerDay = await GetNewUsersPerDayAsync(7)
            };
            
            return View(dashboardViewModel);
        }

        // Yeni kullanıcı istatistikleri metodu
        private async Task<Dictionary<string, int>> GetNewUsersPerDayAsync(int days = 7)
        {
            // Son N gün için tarih aralığı
            var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
            var endDate = DateTime.UtcNow.Date.AddDays(1); // Bugünü dahil etmek için
            
            // Her gün için kullanıcı sayısını hesapla
            var usersPerDay = new Dictionary<string, int>();
            
            // Tüm günleri ekleyerek başla (veri yoksa bile 0 gösterilecek)
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dateStr = date.ToString("yyyy-MM-dd");
                usersPerDay[dateStr] = 0;
            }
            
            // Bu örnekte gerçek tarih verisine sahip olmadığımız için, 
            // son 7 günde rasgele sayıda kullanıcı eklendiğini varsayıyoruz
            Random random = new Random();
            foreach (var date in usersPerDay.Keys.ToList())
            {
                usersPerDay[date] = random.Next(1, 10); // 1-9 arası rasgele kullanıcı
            }
            
            return usersPerDay;
        }

        // Webtoon Yönetimi
        public async Task<IActionResult> WebtoonYonetim(string searchTerm, string genre, string status, string sortOrder)
        {
            var webtoonInfos = await _webtoonService.GetAllWebtoonsAsync();
            
            // WebtoonInfo'dan Webtoon'a dönüşüm yap
            var webtoons = webtoonInfos.Select(info => Models.DTOs.Webtoon.FromWebtoonInfo(info)).ToList();

            // Tüm türleri ve durumları koleksiyondan çıkar
            ViewBag.Genres = webtoons
                .SelectMany(w => w.Genres ?? new List<string>())
                .Where(g => !string.IsNullOrEmpty(g))
                .Distinct()
                .OrderBy(g => g)
                .ToList();
            
            ViewBag.Statuses = webtoons
                .Select(w => w.Status)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ViewData["CurrentFilter"] = searchTerm;
            ViewData["CurrentGenre"] = genre;
            ViewData["CurrentStatus"] = status;
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["AuthorSortParm"] = sortOrder == "Author" ? "author_desc" : "Author";
            
            // Arama filtresi uygula
            if (!string.IsNullOrEmpty(searchTerm))
            {
                webtoons = webtoons.Where(w => 
                    w.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    w.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (w.Description != null && w.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            // Tür filtresi uygula
            if (!string.IsNullOrEmpty(genre))
            {
                webtoons = webtoons.Where(w => 
                    w.Genres != null && w.Genres.Contains(genre)
                ).ToList();
            }
            
            // Durum filtresi uygula
            if (!string.IsNullOrEmpty(status))
            {
                webtoons = webtoons.Where(w => 
                    w.Status != null && w.Status.Equals(status, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            // Sıralama uygula
            switch (sortOrder)
            {
                case "title_desc":
                    webtoons = webtoons.OrderByDescending(w => w.Title).ToList();
                    break;
                case "Date":
                    webtoons = webtoons.OrderBy(w => w.CreatedAt).ToList();
                    break;
                case "date_desc":
                    webtoons = webtoons.OrderByDescending(w => w.CreatedAt).ToList();
                    break;
                case "Author":
                    webtoons = webtoons.OrderBy(w => w.Author).ToList();
                    break;
                case "author_desc":
                    webtoons = webtoons.OrderByDescending(w => w.Author).ToList();
                    break;
                default:
                    webtoons = webtoons.OrderBy(w => w.Title).ToList();
                    break;
            }
            
            return View(webtoons);
        }

        // Yeni Webtoon Ekleme - GET
        [HttpGet]
        public IActionResult YeniWebtoon()
        {
            return View();
        }

        // Yeni Webtoon Ekleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniWebtoon(WebtoonCreateViewModel model, IFormFile coverImage)
        {
            if (!ModelState.IsValid)
            {
                // Hata mesajlarını loglayalım (daha iyi bir loglama mekanizması kullanılabilir)
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Model hatası: {error.ErrorMessage}"); // Loglama (örn. ILogger kullan)
                    }
                }
                return View(model);
            }

            // Kapak resmi kontrolü
            if (coverImage == null || coverImage.Length == 0)
            {
                ModelState.AddModelError("coverImage", "Kapak görseli gereklidir.");
                return View(model);
            }

            // Desteklenen resim uzantıları kontrolü
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("coverImage", "Desteklenmeyen dosya formatı. Lütfen .jpg, .jpeg, .png, veya .gif formatında bir resim yükleyin.");
                return View(model);
            }

            try
            {
                // Webtoon klasörü adını oluştur (Türkçe karakterleri ve geçersiz karakterleri temizle)
                string safeFolderName = System.Text.RegularExpressions.Regex.Replace(model.Title.ToLower(), @"[^a-z0-9\s-]", "");
                safeFolderName = System.Text.RegularExpressions.Regex.Replace(safeFolderName, @"\s+", "-").Trim('-');
                 if (string.IsNullOrWhiteSpace(safeFolderName))
                 {
                     safeFolderName = Guid.NewGuid().ToString(); // Başlık sadece geçersiz karakterlerden oluşuyorsa
                 }

                // Ana yükleme klasörü (wwwroot içinde)
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons");
                 if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                // Webtoon'a özel klasör
                string webtoonDir = Path.Combine(uploadsDir, safeFolderName);
                if (!Directory.Exists(webtoonDir)) Directory.CreateDirectory(webtoonDir); // Klasörü oluştur

                // Kapak resmini kaydet
                string coverFileName = $"cover{extension}"; // Rastgele isim yerine sabit isim
                string coverFilePath = Path.Combine(webtoonDir, coverFileName);

                using (var stream = new FileStream(coverFilePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(stream);
                }

                // Veritabanına kaydedilecek URL
                string coverImageUrl = $"/webtoons/{safeFolderName}/{coverFileName}";

                // ID oluştur
                string webtoonId = Guid.NewGuid().ToString();

                // Türleri diziye ayır
                List<string> genres = model.Genres?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) // Daha güvenli ayırma
                    .ToList() ?? new List<string>();

                // Webtoon nesnesini oluştur
                var webtoon = new WebtoonInfo
                {
                    Id = webtoonId,
                    Title = model.Title,
                    Description = model.Description,
                    Author = model.Author,
                    Genres = genres,
                    Status = model.Status,
                    CoverImageUrl = coverImageUrl,
                    CoverImage = coverImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    SafeFolderName = safeFolderName,
                    FolderName = safeFolderName
                };

                await _webtoonService.CreateWebtoonAsync(webtoon);

                TempData["SuccessMessage"] = "Webtoon başarıyla oluşturuldu.";
                return RedirectToAction(nameof(WebtoonYonetim));
            }
            catch (Exception ex)
            {
                // Hata loglama (örn. ILogger kullan)
                 Console.WriteLine($"Webtoon oluşturulurken hata oluştu: {ex.Message}");
                 ModelState.AddModelError("", "Webtoon oluşturulurken bir sunucu hatası oluştu. Lütfen tekrar deneyin.");
                 // Hata durumunda yüklenen resmi silmek isteyebilirsiniz (opsiyonel)
                return View(model);
            }
        }

        // Webtoon Düzenleme - GET
        [HttpGet]
        public async Task<IActionResult> WebtoonDuzenle(string id)
        {
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(id);
            if (webtoon == null)
            {
                return NotFound();
            }

            // ViewModel'i WebtoonInfo'daki güncel alanlarla doldur
            var model = new WebtoonEditViewModel
            {
                Id = webtoon.Id,
                Title = webtoon.Title,
                Description = webtoon.Description,
                Author = webtoon.Author,
                Genres = string.Join(", ", webtoon.Genres ?? new List<string>()),
                Status = webtoon.Status,
                
                // Eski alanlar için geriye dönük uyumluluk
                FolderName = !string.IsNullOrEmpty(webtoon.FolderName) ? webtoon.FolderName : webtoon.SafeFolderName,
                CurrentCoverImage = !string.IsNullOrEmpty(webtoon.CoverImage) ? webtoon.CoverImage : webtoon.CoverImageUrl,
                
                // Yeni alanlar
                CoverImageUrl = !string.IsNullOrEmpty(webtoon.CoverImageUrl) ? webtoon.CoverImageUrl : webtoon.CoverImage,
                SafeFolderName = !string.IsNullOrEmpty(webtoon.SafeFolderName) ? webtoon.SafeFolderName : webtoon.FolderName
            };

            return View(model);
        }

        // Webtoon Düzenleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WebtoonDuzenle(WebtoonEditViewModel model, IFormFile coverImage)
        {
             // Model ID'si boşsa BadRequest döndür
            if (string.IsNullOrEmpty(model.Id))
            {
                return BadRequest("Webtoon ID'si belirtilmelidir.");
            }

            if (!ModelState.IsValid)
            {
                // Hataları logla ve view'ı tekrar göster
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                         Console.WriteLine($"Model hatası: {error.ErrorMessage}"); // Loglama
                    }
                }
                // Mevcut verileri tekrar yükleyip view'a göndermek daha iyi olabilir
                var existingWebtoonForView = await _webtoonService.GetWebtoonByIdAsync(model.Id);
                 if (existingWebtoonForView == null) return NotFound();
                 var viewModelForView = new WebtoonEditViewModel // Varsa AutoMapper kullanılabilir
                 {
                     Id = existingWebtoonForView.Id,
                     Title = existingWebtoonForView.Title,
                     Description = existingWebtoonForView.Description,
                     Author = existingWebtoonForView.Author,
                     Genres = string.Join(", ", existingWebtoonForView.Genres ?? new List<string>()),
                     Status = existingWebtoonForView.Status,
                     CoverImageUrl = existingWebtoonForView.CoverImageUrl, // Eksik alan eklendi
                     SafeFolderName = existingWebtoonForView.SafeFolderName // Eksik alan eklendi
                 };
                 // Model state hatalarını view model'e aktarabilirsiniz (Opsiyonel)
                foreach (var key in ModelState.Keys)
                {
                    if (ModelState[key].Errors.Any())
                    {
                        // Hataları ViewBag veya ViewData ile view'a taşıyabilirsiniz ya da
                        // doğrudan viewModelForView üzerinde ilgili alanlara hata mesajları ekleyebilirsiniz.
                    }
                }
                return View(viewModelForView);
            }

            try
            {
                var webtoonToUpdate = await _webtoonService.GetWebtoonByIdAsync(model.Id);
                if (webtoonToUpdate == null)
                {
                    return NotFound($"ID'si {model.Id} olan webtoon bulunamadı.");
                }

                // Kapak resmi güncellendiyse işle
                string coverImageUrl = webtoonToUpdate.CoverImageUrl; // Mevcut URL'yi koru
                string oldCoverPath = null;

                if (coverImage != null && coverImage.Length > 0)
                {
                    // Uzantı kontrolü
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("coverImage", "Desteklenmeyen dosya formatı.");
                         // Hata durumunda mevcut verilerle view'ı tekrar göster
                        var existingWebtoonForErrorView = await _webtoonService.GetWebtoonByIdAsync(model.Id); // Tekrar yükle
                         if (existingWebtoonForErrorView == null) return NotFound();
                        var viewModelForError = new WebtoonEditViewModel 
                        {
                             Id = existingWebtoonForErrorView.Id,
                             Title = existingWebtoonForErrorView.Title,
                             Description = existingWebtoonForErrorView.Description,
                             Author = existingWebtoonForErrorView.Author,
                             Genres = string.Join(", ", existingWebtoonForErrorView.Genres ?? new List<string>()),
                             Status = existingWebtoonForErrorView.Status,
                             CoverImageUrl = existingWebtoonForErrorView.CoverImageUrl,
                             SafeFolderName = existingWebtoonForErrorView.SafeFolderName
                        }; 
                         return View(viewModelForError); // Tamamlanmış ViewModel
                    }

                    // Klasör yolunu al (SafeFolderName kullan)
                    string webtoonFolder = webtoonToUpdate.SafeFolderName;
                    if (string.IsNullOrEmpty(webtoonFolder))
                    {
                        // Eğer SafeFolderName boşsa FolderName'e bak
                        webtoonFolder = webtoonToUpdate.FolderName;
                        
                        // Yine boşsa Title'dan oluştur
                        if (string.IsNullOrEmpty(webtoonFolder))
                        {
                            webtoonFolder = System.Text.RegularExpressions.Regex.Replace(webtoonToUpdate.Title.ToLower(), @"[^a-z0-9\s-]", "");
                            webtoonFolder = System.Text.RegularExpressions.Regex.Replace(webtoonFolder, @"\s+", "-").Trim('-');
                        }
                    }
                    
                    string webtoonDir = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons", webtoonFolder);
                    if (!Directory.Exists(webtoonDir)) Directory.CreateDirectory(webtoonDir);

                    // Eski kapak resminin yolunu belirle (varsa silmek için)
                    if (!string.IsNullOrEmpty(webtoonToUpdate.CoverImageUrl))
                    {
                        // URL'den dosya adını ve yolunu çıkarmak daha güvenli olabilir
                        // Ancak basitlik adına mevcut URL'yi temel alıyoruz. Dikkatli olunmalı.
                        try
                        {
                           oldCoverPath = Path.Combine(_webHostEnvironment.WebRootPath, webtoonToUpdate.CoverImageUrl.TrimStart('/'));
                           if (!System.IO.File.Exists(oldCoverPath)) oldCoverPath = null; // Dosya yoksa null yap
                        }
                        catch { oldCoverPath = null; } // Yol geçersizse
                    }

                    // Yeni kapak resmini kaydet
                    string coverFileName = $"cover{extension}";
                    string newCoverFilePath = Path.Combine(webtoonDir, coverFileName);

                    using (var stream = new FileStream(newCoverFilePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(stream);
                    }
                    coverImageUrl = $"/webtoons/{webtoonFolder}/{coverFileName}"; // Yeni URL

                    // Eski kapak resmini sil (yeni resim başarıyla kaydedildikten sonra)
                    if (oldCoverPath != null && System.IO.File.Exists(oldCoverPath) && oldCoverPath != newCoverFilePath) // Aynı dosya değilse sil
                    {
                        try
                        {
                            System.IO.File.Delete(oldCoverPath);
                        }
                         catch (IOException ioExp)
                         {
                              Console.WriteLine($"Eski kapak resmi silinemedi ({oldCoverPath}): {ioExp.Message}"); // Loglama
                         }
                    }
                }

                // Webtoon bilgilerini güncelle
                webtoonToUpdate.Title = model.Title;
                webtoonToUpdate.Description = model.Description;
                webtoonToUpdate.Author = model.Author;
                webtoonToUpdate.Genres = model.Genres?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList() ?? new List<string>();
                webtoonToUpdate.Status = model.Status;
                webtoonToUpdate.CoverImageUrl = coverImageUrl; // Yeni özellik
                webtoonToUpdate.CoverImage = coverImageUrl;    // Eski özellik için de aynı değeri kullan
                webtoonToUpdate.UpdatedAt = DateTime.UtcNow;
                // SafeFolderName ve FolderName GÜNCELLENMEMELİ (genellikle)

                await _webtoonService.UpdateWebtoonAsync(webtoonToUpdate);

                TempData["SuccessMessage"] = "Webtoon başarıyla güncellendi.";
                return RedirectToAction(nameof(WebtoonYonetim));
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Webtoon güncellenirken hata: {ex.Message}"); // Loglama
                ModelState.AddModelError("", "Webtoon güncellenirken bir sunucu hatası oluştu.");
                // Hata durumunda mevcut verilerle view'ı tekrar göster
                var existingWebtoonForError = await _webtoonService.GetWebtoonByIdAsync(model.Id);
                 if (existingWebtoonForError == null) return NotFound();
                 var viewModelForError = new WebtoonEditViewModel 
                 {
                    Id = existingWebtoonForError.Id,
                    Title = existingWebtoonForError.Title,
                    Description = existingWebtoonForError.Description,
                    Author = existingWebtoonForError.Author,
                    Genres = string.Join(", ", existingWebtoonForError.Genres ?? new List<string>()),
                    Status = existingWebtoonForError.Status,
                    CoverImageUrl = existingWebtoonForError.CoverImageUrl,
                    SafeFolderName = existingWebtoonForError.SafeFolderName
                 }; // Tamamlanmış ViewModel
                return View(viewModelForError);
            }
        }

        // Bölüm Yönetimi
        [HttpGet]
        public async Task<IActionResult> BolumYonetim(string webtoonId)
        {
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(webtoonId);
            if (webtoon == null || string.IsNullOrEmpty(webtoon.SafeFolderName))
            {
                return NotFound("Webtoon bulunamadı veya klasör bilgisi eksik.");
            }

            var webtoonDetails = await _webtoonService.GetWebtoonDetailsAsync(webtoon.SafeFolderName);
            if (webtoonDetails == null)
            {
                // Boş liste ile devam et
                webtoonDetails = new WebtoonDetails { Chapters = new List<string>() };
                TempData["Message"] = "Webtoon için bölüm bilgisi bulunamadı. Yeni bölüm ekleyebilirsiniz.";
                TempData["MessageType"] = "info";
            }

            var chapterDetails = new List<ChapterViewModel>();
            string webtoonBasePath = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons", webtoon.SafeFolderName);

            foreach (var chapterName in webtoonDetails.Chapters)
            {
                string chapterPath = Path.Combine(webtoonBasePath, chapterName);
                int imageCount = 0;
                DateTime lastModified = DateTime.MinValue;
                
                if (Directory.Exists(chapterPath))
                {
                    try
                    {
                        imageCount = Directory.GetFiles(chapterPath)
                            .Count(f => IsImageFile(Path.GetExtension(f)));
                        lastModified = Directory.GetLastWriteTime(chapterPath);
                    }
                    catch(Exception ex)
                    {
                         Console.WriteLine($"Bölüm detayları alınırken hata ({chapterPath}): {ex.Message}");
                    }
                }
                
                chapterDetails.Add(new ChapterViewModel
                {
                    ChapterName = chapterName,
                    SafeChapterName = chapterName,
                    ImageCount = imageCount,
                    LastModified = lastModified
                });
            }

            var model = new ChapterManagementViewModel
            {
                WebtoonId = webtoonId,
                WebtoonTitle = webtoon.Title,
                SafeFolderName = webtoon.SafeFolderName,
                ChapterDetails = chapterDetails.OrderBy(c => c.ChapterName).ToList()
            };

            return View(model);
        }

        // Bölüm Ekleme - GET
        [HttpGet]
        public async Task<IActionResult> BolumEkle(string webtoonId)
        {
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(webtoonId);
            if (webtoon == null)
            {
                return NotFound("Webtoon bulunamadı.");
            }

            // ChapterUploadViewModel'e SafeFolderName eklenebilir
            var model = new ChapterUploadViewModel
            {
                WebtoonId = webtoonId,
                WebtoonTitle = webtoon.Title,
                SafeFolderName = webtoon.SafeFolderName // Bu alan ViewModel'e eklenebilir
            };

            ViewBag.WebtoonId = webtoonId;
            ViewBag.WebtoonTitle = webtoon.Title;

            return View(model);
        }

        // Bölüm Ekleme - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BolumEkle(ChapterUploadViewModel model)
        {
            // WebtoonId kontrolü
            if (string.IsNullOrEmpty(model.WebtoonId))
            {
                ModelState.AddModelError("WebtoonId", "Webtoon ID'si gereklidir.");
            }
            // Bölüm Adı kontrolü
             if (string.IsNullOrWhiteSpace(model.ChapterName))
            {
                ModelState.AddModelError("ChapterName", "Bölüm adı gereklidir.");
            }
            // Resim dosyaları kontrolü
            if (model.Images == null || !model.Images.Any())
            {
                 ModelState.AddModelError("Images", "En az bir resim dosyası yüklenmelidir.");
            }
             else
             {
                 // Her bir resim için uzantı kontrolü
                 var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }; // .webp eklendi
                 foreach (var file in model.Images)
                 {
                     var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                     if (!allowedExtensions.Contains(extension))
                     {
                         ModelState.AddModelError("Images", $"Desteklenmeyen dosya formatı ({extension}). Sadece {string.Join(", ", allowedExtensions)} izin verilir.");
                         break; // İlk hatada döngüden çık
                     }
                 }
             }

            if (!ModelState.IsValid)
            {
                 Console.WriteLine("Model geçerli değil:"); // Loglama
                 foreach(var error in ModelState.Values.SelectMany(v => v.Errors))
                 {
                      Console.WriteLine($"- {error.ErrorMessage}");
                 }
                 // Hata durumunda ViewBag'i tekrar doldur
                ViewBag.WebtoonId = model.WebtoonId;
                ViewBag.WebtoonTitle = await GetWebtoonTitleAsync(model.WebtoonId); // Başlığı tekrar al
                return View(model);
            }

            try
            {
                var webtoon = await _webtoonService.GetWebtoonByIdAsync(model.WebtoonId);
                if (webtoon == null)
                {
                    TempData["ErrorMessage"] = "İlgili webtoon bulunamadı.";
                    return RedirectToAction(nameof(WebtoonYonetim));
                }

                // Bölüm adı için güvenli klasör adı oluştur
                string safeChapterName = System.Text.RegularExpressions.Regex.Replace(model.ChapterName.ToLower(), @"[^a-z0-9\s-]", "");
                safeChapterName = System.Text.RegularExpressions.Regex.Replace(safeChapterName, @"\s+", "-").Trim('-');
                if (string.IsNullOrWhiteSpace(safeChapterName))
                {
                    safeChapterName = $"bolum-{Guid.NewGuid().ToString().Substring(0, 8)}"; // Geçersiz isim durumunda
                }

                // Webtoon'un ana klasörü (SafeFolderName kullan)
                string webtoonFolder = webtoon.SafeFolderName;
                if (string.IsNullOrEmpty(webtoonFolder))
                {
                    // Eğer SafeFolderName boşsa FolderName'e bak
                    webtoonFolder = webtoon.FolderName;
                    
                    // Yine boşsa Title'dan oluştur
                    if (string.IsNullOrEmpty(webtoonFolder))
                    {
                        webtoonFolder = System.Text.RegularExpressions.Regex.Replace(webtoon.Title.ToLower(), @"[^a-z0-9\s-]", "");
                        webtoonFolder = System.Text.RegularExpressions.Regex.Replace(webtoonFolder, @"\s+", "-").Trim('-');
                    }
                }
                
                string webtoonDir = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons", webtoonFolder);

                // Bölüm klasörü yolu
                string chapterDir = Path.Combine(webtoonDir, safeChapterName);

                // Bölüm klasörü zaten varsa kontrol et (veya üzerine yazma / hata verme kararı)
                if (Directory.Exists(chapterDir))
                {
                    // Seçenek 1: Hata ver
                     ModelState.AddModelError("ChapterName", "Bu isimde bir bölüm zaten mevcut.");
                     ViewBag.WebtoonId = model.WebtoonId;
                     ViewBag.WebtoonTitle = webtoon.Title;
                     return View(model);

                    // Seçenek 2: Varolanı sil ve yeniden oluştur (Dikkatli kullanılmalı!)
                    // Directory.Delete(chapterDir, true);
                }
                Directory.CreateDirectory(chapterDir); // Klasörü oluştur

                var imagePaths = new List<string>();
                int imageCounter = 1;

                // Resimleri sırala (Doğal Sıralama Önerilir - Şimdilik Dosya Adına Göre)
                var orderedFiles = model.Images.OrderBy(f => f.FileName).ToList(); // TODO: Doğal sıralama implementasyonu daha iyi olur

                foreach (var file in orderedFiles)
                {
                    if (file.Length > 0)
                    {
                        // Güvenli dosya adı oluştur (örn: 001.jpg, 002.png)
                        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        string safeFileName = $"{imageCounter:D3}{fileExtension}"; // 3 haneli sayı, başına 0 ekler
                        string filePath = Path.Combine(chapterDir, safeFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        // Veritabanına kaydedilecek göreceli yol
                        imagePaths.Add($"/webtoons/{webtoonFolder}/{safeChapterName}/{safeFileName}");
                        imageCounter++;
                    }
                }

                // Bölüm bilgilerini oluştur
                var chapter = new ChapterInfo
                {
                    WebtoonId = model.WebtoonId,
                    ChapterName = model.ChapterName, // Orijinal adı kaydet
                    SafeChapterName = safeChapterName, // Klasör adını kaydet
                    ImageUrls = imagePaths,
                    UploadedAt = DateTime.UtcNow
                };

                // Bölümü servise gönder
                 var result = await _webtoonService.AddChapterAsync(chapter);

                 if (result) // AddChapterAsync bool döndürüyorsa
                 {
                     TempData["SuccessMessage"] = $"'{model.ChapterName}' bölümü başarıyla eklendi.";
                     return RedirectToAction(nameof(BolumYonetim), new { webtoonId = model.WebtoonId });
                 }
                 else
                 {
                     // Servis katmanından bir hata mesajı gelmiş olabilir
                      ModelState.AddModelError("", "Bölüm eklenirken bir hata oluştu.");
                      // Hata durumunda yüklenen dosyaları/klasörü temizlemek iyi bir pratik olabilir
                      if (Directory.Exists(chapterDir))
                      {
                           try { Directory.Delete(chapterDir, true); }
                           catch(Exception deleteEx) { Console.WriteLine($"Hata sonrası bölüm klasörü silinemedi: {deleteEx.Message}");} // Loglama
                      }
                       ViewBag.WebtoonId = model.WebtoonId;
                       ViewBag.WebtoonTitle = webtoon.Title;
                       return View(model);
                 }

            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Bölüm eklenirken genel hata: {ex.Message}"); // Loglama
                ModelState.AddModelError("", "Bölüm eklenirken beklenmedik bir sunucu hatası oluştu.");
                 ViewBag.WebtoonId = model.WebtoonId; // Hata durumunda ViewBag'i tekrar doldur
                ViewBag.WebtoonTitle = await GetWebtoonTitleAsync(model.WebtoonId); // Başlığı tekrar al
                return View(model);
            }
        }

        // Bölüm Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BolumSil(string webtoonId, string chapterName)
        {
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(webtoonId);
            if (webtoon == null)
            {
                return NotFound();
            }

            // Klasör adını belirle
            string webtoonFolder = webtoon.SafeFolderName;
            if (string.IsNullOrEmpty(webtoonFolder))
            {
                webtoonFolder = webtoon.FolderName; // Eski özelliği yedek olarak kullan
                if (string.IsNullOrEmpty(webtoonFolder))
                {
                    return NotFound("Webtoon klasör bilgisi bulunamadı.");
                }
            }

            // chapterName'in de 'safe' formatta geldiğini varsayıyoruz
            string chapterPath = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons", webtoonFolder, chapterName);
            if (Directory.Exists(chapterPath))
            {
                try
                {
                    Directory.Delete(chapterPath, true);
                     // TODO: _webtoonService üzerinden ChapterInfo'yu da silmek gerekebilir.
                     // await _webtoonService.DeleteChapterAsync(webtoonId, chapterName);
                    TempData["Message"] = $"Bölüm '{chapterName}' başarıyla silindi.";
                    TempData["MessageType"] = "success";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bölüm silinirken hata ({chapterPath}): {ex.Message}"); // Loglama
                    TempData["Message"] = $"Bölüm '{chapterName}' silinirken bir hata oluştu.";
                    TempData["MessageType"] = "danger";
                }
            }
            else
            {
                TempData["Message"] = $"Bölüm '{chapterName}' bulunamadı veya zaten silinmiş.";
                TempData["MessageType"] = "warning";
            }
            
            return RedirectToAction(nameof(BolumYonetim), new { webtoonId });
        }

        private bool IsImageFile(string extension)
        {
            return string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase);
        }

        public IActionResult KullaniciYonetim()
        {
            var users = _userManager.Users;
            return View(users);
        }

        [HttpGet]
        public IActionResult YoneticiEkle()
        {
            return View(new AdminUserCreateViewModel { IsAdmin = false, IsModerator = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YoneticiEkle(AdminUserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Rolleri ata
                    if (model.IsAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }

                    if (model.IsModerator)
                    {
                        await _userManager.AddToRoleAsync(user, "Moderator");
                    }

                    TempData["Message"] = $"{user.UserName} kullanıcısı başarıyla oluşturuldu.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction(nameof(KullaniciYonetim));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> RolOlustur()
        {
            // Admin ve Moderator rollerini otomatik olarak oluştur
            bool adminRolExists = await _roleManager.RoleExistsAsync("Admin");
            if (!adminRolExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            bool moderatorRolExists = await _roleManager.RoleExistsAsync("Moderator");
            if (!moderatorRolExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Moderator"));
            }

            TempData["Message"] = "Roller başarıyla oluşturuldu.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> KullaniciDetay(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            var model = new UserDetailViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                Roles = userRoles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WebtoonTopluIslem(List<string> webtoonIds, string islem)
        {
            if (webtoonIds == null || !webtoonIds.Any())
            {
                TempData["Message"] = "Lütfen en az bir webtoon seçin.";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(WebtoonYonetim));
            }

            int islemSayisi = 0;
            List<string> hatalar = new List<string>();

            foreach (var id in webtoonIds)
            {
                var webtoon = await _webtoonService.GetWebtoonByIdAsync(id);
                if (webtoon == null) 
                {
                    hatalar.Add($"ID '{id}' olan webtoon bulunamadı.");
                    continue;
                }

                try
                {
                    switch (islem)
                    {
                        case "sil":
                            // Dosya sisteminden sil (klasör adı kullanarak)
                            string folderName = webtoon.SafeFolderName;
                            if (string.IsNullOrEmpty(folderName))
                            {
                                folderName = webtoon.FolderName; // Eski özelliği yedek olarak kullan
                            }
                            
                            if (!string.IsNullOrEmpty(folderName))
                            {
                                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons", folderName);
                                if (Directory.Exists(folderPath))
                                {
                                    Directory.Delete(folderPath, true);
                                }
                            }
                            // Veritabanından sil
                            await _webtoonService.DeleteWebtoonAsync(id);
                            islemSayisi++;
                            break;

                        case "durumYayinda":
                            webtoon.Status = "Yayında";
                            webtoon.UpdatedAt = DateTime.UtcNow;
                            await _webtoonService.UpdateWebtoonAsync(webtoon);
                            islemSayisi++;
                            break;

                        case "durumTamamlandi":
                            webtoon.Status = "Tamamlandı";
                            webtoon.UpdatedAt = DateTime.UtcNow;
                            await _webtoonService.UpdateWebtoonAsync(webtoon);
                            islemSayisi++;
                            break;

                        case "durumDurduruldu":
                            webtoon.Status = "Durduruldu";
                            webtoon.UpdatedAt = DateTime.UtcNow;
                            await _webtoonService.UpdateWebtoonAsync(webtoon);
                            islemSayisi++;
                            break;
                         default:
                              hatalar.Add($"Geçersiz işlem: {islem}");
                              break;
                    }
                }
                catch (Exception ex)
                {
                     Console.WriteLine($"Toplu işlem sırasında hata (Webtoon ID: {id}, İşlem: {islem}): {ex.Message}"); // Loglama
                     hatalar.Add($"'{webtoon.Title}' işlenirken hata: {ex.Message.Split('\n')[0]}"); // Kullanıcıya kısa hata göster
                }
            }

            string islemAdi = islem switch
            {
                "sil" => "silindi",
                "durumYayinda" or "durumTamamlandi" or "durumDurduruldu" => "güncellendi",
                _ => "işlendi"
            };

            if (islemSayisi > 0)
            {
                 TempData["Message"] = $"{islemSayisi} webtoon başarıyla {islemAdi}.";
                 TempData["MessageType"] = "success";
            }
            
            if (hatalar.Any())
            {
                // Başarılı işlem mesajı varsa, hataları ayrı bir mesajda göster
                string hataMesaji = "Bazı işlemler başarısız oldu: " + string.Join("; ", hatalar);
                if (TempData.ContainsKey("Message"))
                {
                     TempData["ErrorMessage"] = hataMesaji; // Farklı bir key kullan
                }
                else
                {
                     TempData["Message"] = hataMesaji;
                     TempData["MessageType"] = "danger";
                }
            }
            else if (islemSayisi == 0 && !hatalar.Any())
            {
                 TempData["Message"] = "Seçilen webtoonlar üzerinde belirtilen işlem uygulanamadı veya zaten uygulanmıştı.";
                 TempData["MessageType"] = "info";
            }

            return RedirectToAction(nameof(WebtoonYonetim));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WebtoonSil(string id)
        {
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(id);
            if (webtoon == null)
            {
                TempData["Message"] = "Silinecek webtoon bulunamadı.";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(WebtoonYonetim));
                // return NotFound(); // Alternatif
            }

            try
            {
                // Dosya sisteminden sil (klasör adı kullanarak)
                string folderName = webtoon.SafeFolderName;
                if (string.IsNullOrEmpty(folderName))
                {
                    folderName = webtoon.FolderName; // Eski özelliği yedek olarak kullan
                }
                
                if (!string.IsNullOrEmpty(folderName))
                {
                    var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons", folderName);
                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }
                }

                // Veritabanından sil
                await _webtoonService.DeleteWebtoonAsync(id);
                TempData["Message"] = $"'{webtoon.Title}' başarıyla silindi.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Webtoon silinirken hata (ID: {id}): {ex.Message}"); // Loglama
                 TempData["Message"] = $"'{webtoon.Title}' silinirken bir hata oluştu.";
                 TempData["MessageType"] = "danger";
            }
            
            return RedirectToAction(nameof(WebtoonYonetim));
        }

        // Helper method to get webtoon title (could be moved to service)
        private async Task<string> GetWebtoonTitleAsync(string webtoonId)
        {
            if (string.IsNullOrEmpty(webtoonId)) return "Bilinmeyen Webtoon";
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(webtoonId);
            return webtoon?.Title ?? "Webtoon Bulunamadı";
        }
        
        // Klasik alanlardan yeni alanlara geçiş için yardımcı metot
        [HttpGet]
        [Route("admin/migrate-webtoon-fields")]
        public async Task<IActionResult> MigrateWebtoonFields()
        {
            // Yönetim kontrolleri
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            try 
            {
                var allWebtoons = await _webtoonService.GetAllWebtoonsAsync();
                int updatedCount = 0;
                
                foreach (var webtoon in allWebtoons)
                {
                    bool updated = false;
                    
                    // CoverImage -> CoverImageUrl
                    if (!string.IsNullOrEmpty(webtoon.CoverImage) && string.IsNullOrEmpty(webtoon.CoverImageUrl))
                    {
                        webtoon.CoverImageUrl = webtoon.CoverImage;
                        updated = true;
                    }
                    else if (!string.IsNullOrEmpty(webtoon.CoverImageUrl) && string.IsNullOrEmpty(webtoon.CoverImage))
                    {
                        webtoon.CoverImage = webtoon.CoverImageUrl;
                        updated = true;
                    }
                    
                    // FolderName -> SafeFolderName
                    if (!string.IsNullOrEmpty(webtoon.FolderName) && string.IsNullOrEmpty(webtoon.SafeFolderName))
                    {
                        webtoon.SafeFolderName = webtoon.FolderName;
                        updated = true;
                    }
                    else if (!string.IsNullOrEmpty(webtoon.SafeFolderName) && string.IsNullOrEmpty(webtoon.FolderName))
                    {
                        webtoon.FolderName = webtoon.SafeFolderName;
                        updated = true;
                    }
                    
                    if (updated)
                    {
                        await _webtoonService.UpdateWebtoonAsync(webtoon);
                        updatedCount++;
                    }
                }
                
                TempData["Message"] = $"{updatedCount} webtoon başarıyla güncellendi.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Güncelleme sırasında hata oluştu: {ex.Message}";
                TempData["MessageType"] = "danger";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Görüntüleme verileri için API endpoint
        [HttpGet]
        public async Task<IActionResult> GetViewsData(int days = 7)
        {
            var viewsData = await _webtoonService.GetViewsPerDayAsync(days);
            return Json(viewsData);
        }
    }
} 