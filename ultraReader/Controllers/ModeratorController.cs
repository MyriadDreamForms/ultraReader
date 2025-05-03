using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using ultraReader.Models.Entities;
using ultraReader.Services;
using ultraReader.Data;
using Microsoft.EntityFrameworkCore;

namespace ultraReader.Controllers
{
    [Authorize(Roles = "Moderator,Admin")]
    public class ModeratorController : Controller
    {
        private readonly IWebtoonService _webtoonService;
        private readonly ApplicationDbContext _context;

        public ModeratorController(IWebtoonService webtoonService, ApplicationDbContext context)
        {
            _webtoonService = webtoonService;
            _context = context;
        }

        public IActionResult AccessDenied()
        {
            return RedirectToAction("ModeratorLogin", "Account");
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> IcerikOnay()
        {
            var webtoons = await _webtoonService.GetAllWebtoonsAsync();
            return View(webtoons);
        }

        public async Task<IActionResult> IcerikDetay(string id)
        {
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(id);
            if (webtoon == null)
            {
                return NotFound();
            }
            return View(webtoon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IcerikOnayla(string id)
        {
            try
            {
                var webtoon = await _webtoonService.GetWebtoonByIdAsync(id);
                if (webtoon == null)
                    return NotFound("Webtoon bulunamadı.");

                webtoon.IsApproved = true;
                webtoon.ApprovedBy = User.Identity?.Name ?? "Unknown";
                webtoon.ApprovedAt = DateTime.UtcNow;
                
                var success = await _webtoonService.UpdateWebtoonAsync(webtoon);
                if (success)
                {
                    TempData["Message"] = $"Webtoon onaylandı: {webtoon.Title}";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Onay sırasında bir hata oluştu.";
                    TempData["MessageType"] = "danger";
                }
                
                return RedirectToAction(nameof(IcerikOnay));
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Beklenmeyen bir hata oluştu: {ex.Message}";
                TempData["MessageType"] = "danger";
                return RedirectToAction(nameof(IcerikOnay));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IcerikReddet(string id, string rejectReason)
        {
            var webtoon = await _webtoonService.GetWebtoonByIdAsync(id);
            if (webtoon == null)
            {
                return NotFound();
            }

            webtoon.IsRejected = true;
            webtoon.RejectedAt = DateTime.UtcNow;
            webtoon.RejectedBy = User.Identity?.Name ?? "Unknown";
            webtoon.RejectReason = rejectReason;

            // Eğer daha önce onaylanmışsa, onay bilgilerini temizle
            if (webtoon.IsApproved)
            {
                webtoon.IsApproved = false;
            }

            await _webtoonService.UpdateWebtoonAsync(webtoon);
            TempData["Message"] = $"{webtoon.Title} reddedildi.";
            TempData["MessageType"] = "warning";

            return RedirectToAction(nameof(IcerikOnay));
        }

        public async Task<IActionResult> YorumYonetim()
        {
            var comments = await _context.Comments
                .Include(c => c.Webtoon)
                .Where(c => !c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
                
            return View(comments);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YorumOnayla(string id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            
            comment.IsApproved = true;
            comment.ApprovedAt = DateTime.UtcNow;
            comment.ApprovedBy = User.Identity?.Name ?? "Unknown";
            
            _context.Update(comment);
            await _context.SaveChangesAsync();
            
            TempData["Message"] = "Yorum başarıyla onaylandı.";
            TempData["MessageType"] = "success";
            
            return RedirectToAction(nameof(YorumYonetim));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YorumSil(string id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            
            TempData["Message"] = "Yorum başarıyla silindi.";
            TempData["MessageType"] = "warning";
            
            return RedirectToAction(nameof(YorumYonetim));
        }
        
        public async Task<IActionResult> YorumDetay(string id)
        {
            var comment = await _context.Comments
                .Include(c => c.Webtoon)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (comment == null)
            {
                return NotFound();
            }
            
            return View(comment);
        }
    }
} 