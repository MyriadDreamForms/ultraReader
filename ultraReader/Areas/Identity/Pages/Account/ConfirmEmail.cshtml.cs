using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ultraReader.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmEmailModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }
        
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                StatusMessage = "E-posta onay bağlantısı geçersiz veya eksik.";
                IsSuccess = false;
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Kullanıcı bulunamadı.";
                IsSuccess = false;
                return Page();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            
            if (result.Succeeded)
            {
                StatusMessage = "E-posta adresiniz onaylandı.";
                IsSuccess = true;
            }
            else
            {
                StatusMessage = "E-posta adresiniz onaylanırken bir hata oluştu.";
                IsSuccess = false;
            }
            
            return Page();
        }
    }
} 