using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ultraReader.Models.DTOs;

namespace ultraReader.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult AdminLogin()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(model);
            }

            var isInRole = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isInRole)
            {
                ModelState.AddModelError(string.Empty, "Bu sayfaya erişim yetkiniz bulunmamaktadır.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Admin");
            }
            
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            
            return View(model);
        }

        [HttpGet]
        public IActionResult ModeratorLogin()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Moderator");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModeratorLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(model);
            }

            var isInRole = await _userManager.IsInRoleAsync(user, "Moderator") || await _userManager.IsInRoleAsync(user, "Admin");
            if (!isInRole)
            {
                ModelState.AddModelError(string.Empty, "Bu sayfaya erişim yetkiniz bulunmamaktadır.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Moderator");
            }
            
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "WebtoonList");
        }

        public IActionResult Index()
        {
            // Zaten girişli kullanıcıları Profil sayfasına yönlendir
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Profile", "User");
            }
            
            return View();
        }
        
        [Authorize]
        public IActionResult MySettings()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Settings", "User");
            }
            
            return View();
        }
    }
} 