using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ultraReader.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ultraReader.Models.Entities;
using ultraReader.Models.DTOs;
using Microsoft.AspNetCore.Hosting;

namespace ultraReader.Controllers
{
    public class WebtoonController : Controller
    {
        private readonly IWebtoonService _webtoonService;
        private readonly ICommentService _commentService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public WebtoonController(
            IWebtoonService webtoonService,
            ICommentService commentService,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env)
        {
            _webtoonService = webtoonService;
            _commentService = commentService;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Details(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return RedirectToAction("Index", "WebtoonList");
            }

            var webtoon = await _webtoonService.GetWebtoonDetailsByFolderNameAsync(name);
            if (webtoon == null)
            {
                return NotFound();
            }

            // Webtoon için yorumları al
            string webtoonId = webtoon.Id;
            var comments = await _commentService.GetCommentsForWebtoonAsync(webtoonId);
            
            // Webtoon için ortalama puanı al
            var averageRating = await _commentService.GetWebtoonRatingAsync(webtoonId);
            
            // Bölümleri al
            var chapters = await _webtoonService.GetChaptersAsync(name);
            
            // Toplam görüntülenme sayısını hesapla
            int totalViews = chapters.Sum(c => c.Views);

            // Kullanıcı girişi yapmışsa, kullanıcının verdiği puanı al
            int userRating = 0;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                string? userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var rating = await _commentService.GetUserRatingAsync(webtoonId, userId);
                    userRating = rating?.Value ?? 0;
                }
            }

            // Kapak resim URL'sini oluştur
            string coverImageUrl = $"/webtoons/{webtoon.FolderName}/{webtoon.CoverImage}";
            if (string.IsNullOrEmpty(webtoon.CoverImage))
            {
                coverImageUrl = "/images/no-cover.png";
            }

            var model = new WebtoonDetailsViewModel
            {
                Webtoon = webtoon,
                CoverImageUrl = coverImageUrl,
                Comments = comments,
                Chapters = chapters.Select(c => new ChapterSummary
                {
                    FolderName = c.FolderName,
                    ChapterNumber = c.ChapterNumber,
                    Title = c.Title,
                    Views = c.Views,
                    PublishedAt = c.PublishedAt
                }).ToList(),
                TotalViews = totalViews,
                AverageRating = averageRating,
                UserRating = userRating
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(AddCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Details), new { name = model.WebtoonFolderName });
            }

            var userId = _userManager.GetUserId(User);
            var userName = User.Identity?.Name ?? "Misafir";

            var comment = new Comment
            {
                UserId = userId,
                UserName = userName,
                WebtoonId = model.WebtoonId,
                ChapterId = model.ChapterId,
                Content = model.Content,
                Rating = model.Rating,
                IsApproved = false // Yorumlar moderatör onayı gerektiriyor
            };

            await _commentService.AddCommentAsync(comment);

            TempData["Message"] = "Yorumunuz gönderildi ve moderatör onayından sonra yayınlanacak.";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(Details), new { name = model.WebtoonFolderName });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateWebtoon(RateWebtoonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Details), new { name = model.WebtoonFolderName });
            }

            var userId = _userManager.GetUserId(User);
            await _commentService.RateWebtoonAsync(model.WebtoonId, userId, model.Rating);

            TempData["Message"] = "Puanınız başarıyla kaydedildi.";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(Details), new { name = model.WebtoonFolderName });
        }
        
        // Yeni Bölüm Ekleme - GET
        [Authorize(Roles = "Admin,Moderator")]
        [HttpGet]
        public IActionResult AddChapter(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Webtoon adı eksik.");

            var model = new AddChapterViewModel
            {
                WebtoonName = name
            };

            return View(model);
        }

        // Yeni Bölüm Ekleme - POST
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChapter(AddChapterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string chapterPath = Path.Combine(_env.WebRootPath, "webtoons", model.WebtoonName, model.ChapterName);

            if (Directory.Exists(chapterPath))
            {
                ModelState.AddModelError("", "Bu isimde bir bölüm zaten var.");
                return View(model);
            }

            Directory.CreateDirectory(chapterPath);

            int index = 1;
            foreach (var file in model.Images)
            {
                if (file.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = $"{index:D3}{extension}";
                    var filePath = Path.Combine(chapterPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    index++;
                }
            }

            TempData["Message"] = "Bölüm başarıyla eklendi.";
            TempData["MessageType"] = "success";
            
            return RedirectToAction("Details", new { name = model.WebtoonName });
        }

        // Webtoon Silme
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Webtoon ID'si gereklidir.");

            var webtoon = await _webtoonService.GetWebtoonByIdAsync(id);
            if (webtoon == null)
                return NotFound("Webtoon bulunamadı.");

            string webtoonPath = Path.Combine(_env.WebRootPath, "webtoons", webtoon.FolderName);

            if (Directory.Exists(webtoonPath))
            {
                try
                {
                    Directory.Delete(webtoonPath, true); // true: içeriğiyle birlikte sil
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"Dosya silme hatası: {ex.Message}";
                    TempData["MessageType"] = "danger";
                    return RedirectToAction("Index", "Admin");
                }
            }

            var result = await _webtoonService.DeleteWebtoonAsync(id);
            if (result)
            {
                TempData["Message"] = $"{webtoon.Title} başarıyla silindi.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Silme işlemi sırasında bir hata oluştu.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Index", "Admin");
        }

        // Bölüm Silme
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChapter(string webtoonId, string chapter)
        {
            if (string.IsNullOrEmpty(webtoonId) || string.IsNullOrEmpty(chapter))
                return BadRequest("Gerekli bilgiler eksik.");

            var webtoon = await _webtoonService.GetWebtoonByIdAsync(webtoonId);
            if (webtoon == null)
                return NotFound("Webtoon bulunamadı.");

            string chapterPath = Path.Combine(_env.WebRootPath, "webtoons", webtoon.FolderName, chapter);

            if (!Directory.Exists(chapterPath))
                return NotFound("Bölüm bulunamadı.");

            try
            {
                Directory.Delete(chapterPath, true); // klasör ve içeriği silinir
                TempData["Message"] = $"'{chapter}' bölümü silindi.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Silme hatası: {ex.Message}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("Details", new { name = webtoon.FolderName });
        }

        // Bölüm Düzenleme - GET
        [Authorize(Roles = "Admin,Moderator")]
        [HttpGet]
        public async Task<IActionResult> EditChapter(string webtoonId, string chapter)
        {
            if (string.IsNullOrEmpty(webtoonId) || string.IsNullOrEmpty(chapter))
                return BadRequest("Gerekli parametreler eksik.");

            var webtoon = await _webtoonService.GetWebtoonByIdAsync(webtoonId);
            if (webtoon == null)
                return NotFound("Webtoon bulunamadı.");

            string chapterPath = Path.Combine(_env.WebRootPath, "webtoons", webtoon.FolderName, chapter);

            // Dizin var mı kontrol edelim
            if (!Directory.Exists(chapterPath))
                return NotFound($"Bölüm klasörü bulunamadı.");

            var files = Directory.GetFiles(chapterPath)
                     .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                               f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                     .Select(Path.GetFileName)
                     .OrderBy(x => x)
                     .ToList();

            var existingImages = files.Select(f => new ChapterImageViewModel
            {
                FileName = f,
                NewName = f,
                Delete = false
            }).ToList();

            var model = new EditChapterViewModel
            {
                WebtoonName = webtoon.FolderName,
                ChapterName = chapter,
                ExistingImages = existingImages
            };

            return View(model);
        }

        // Bölüm Düzenleme - POST
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(EditChapterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
                
            var webtoon = await _webtoonService.GetWebtoonByFolderNameAsync(model.WebtoonName);
            if (webtoon == null)
                return NotFound("Webtoon bulunamadı.");

            // Mevcut bölüm klasörünü bul
            string currentChapterPath = Path.Combine(_env.WebRootPath, "webtoons", model.WebtoonName, model.ChapterName);
            if (!Directory.Exists(currentChapterPath))
                return NotFound("Bölüm klasörü bulunamadı.");

            // Bölüm adını değiştirmek istiyor mu?
            if (!string.IsNullOrWhiteSpace(model.NewChapterName) &&
                 !model.NewChapterName.Equals(model.ChapterName, StringComparison.OrdinalIgnoreCase))
            {
                string newChapterPath = Path.Combine(_env.WebRootPath, "webtoons", model.WebtoonName, model.NewChapterName);
                if (Directory.Exists(newChapterPath))
                {
                    ModelState.AddModelError("", "Yeni bölüm adı zaten kullanılıyor.");
                    return View(model);
                }
                
                // Klasörü yeniden adlandır
                Directory.Move(currentChapterPath, newChapterPath);
                
                // Güncellenmiş klasör yolunu kullan
                currentChapterPath = newChapterPath;
                model.ChapterName = model.NewChapterName;
            }

            // Mevcut resimleri işle
            foreach (var image in model.ExistingImages)
            {
                if (string.IsNullOrEmpty(image.FileName))
                    continue;

                string filePath = Path.Combine(currentChapterPath, image.FileName);
                
                // Silinecek mi?
                if (image.Delete)
                {
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                // Yeniden adlandırılacak mı?
                else if (!string.IsNullOrEmpty(image.NewName) && !image.NewName.Equals(image.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    string newFilePath = Path.Combine(currentChapterPath, image.NewName);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Move(filePath, newFilePath);
                }
            }

            // Yeni resimleri ekle
            if (model.NewImages != null && model.NewImages.Count > 0)
            {
                // Yeni dosyaların numaralandırması için mevcut dosya sayısını bul
                int index = Directory.GetFiles(currentChapterPath)
                                     .Count(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                                              f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                              f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                              f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)) + 1;
                                              
                foreach (var file in model.NewImages)
                {
                    if (file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var fileName = $"{index:D3}{extension}";
                        var filePath = Path.Combine(currentChapterPath, fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        
                        index++;
                    }
                }
            }

            TempData["Message"] = "Bölüm başarıyla güncellendi.";
            TempData["MessageType"] = "success";
            
            return RedirectToAction("Details", new { name = model.WebtoonName });
        }
    }
} 