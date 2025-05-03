using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ultraReader.Data;
using ultraReader.Models.Entities;
using ultraReader.Models.DTOs;
using ultraReader.Models.Enums;

namespace ultraReader.Services
{
    public class PreferencesService : IPreferencesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PreferencesService> _logger;

        public PreferencesService(ApplicationDbContext context, ILogger<PreferencesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserPreferences> GetUserPreferencesAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return new UserPreferences(); // Varsayılan tercihler

                var preferences = await _context.UserPreferences
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (preferences == null)
                {
                    preferences = new UserPreferences
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserPreferences.Add(preferences);
                    await _context.SaveChangesAsync();
                }

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı tercihleri getirilirken hata oluştu: {UserId}", userId);
                return new UserPreferences(); // Hata durumunda varsayılan tercihleri döndür
            }
        }

        public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || preferences == null)
                    return false;

                preferences.UpdatedAt = DateTime.UtcNow;

                var existingPreferences = await _context.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (existingPreferences != null)
                {
                    // Mevcut tercihleri güncelle
                    existingPreferences.Theme = preferences.Theme;
                    existingPreferences.ReadingDirection = preferences.ReadingDirection;
                    existingPreferences.PageSize = preferences.PageSize;
                    existingPreferences.AutoScroll = preferences.AutoScroll;
                    existingPreferences.AutoScrollSpeed = preferences.AutoScrollSpeed;
                    existingPreferences.EmailNotifications = preferences.EmailNotifications;
                    existingPreferences.NewChapterNotifications = preferences.NewChapterNotifications;
                    existingPreferences.CommentNotifications = preferences.CommentNotifications;
                    existingPreferences.PreferredGenres = preferences.PreferredGenres;
                    existingPreferences.HideCompletedWebtoons = preferences.HideCompletedWebtoons;
                    existingPreferences.UpdatedAt = DateTime.UtcNow;

                    _context.UserPreferences.Update(existingPreferences);
                }
                else
                {
                    // Yeni tercih oluştur
                    preferences.Id = Guid.NewGuid().ToString();
                    preferences.UserId = userId;
                    preferences.CreatedAt = DateTime.UtcNow;
                    
                    _context.UserPreferences.Add(preferences);
                }

                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı tercihleri güncellenirken hata oluştu: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// ReadingMode'dan kullanıcı tercihine uygun ReadingDirection değerini döndürür
        /// </summary>
        public string ConvertReadingModeToDirection(ReadingMode mode)
        {
            return mode switch
            {
                ReadingMode.Continuous => "vertical",
                ReadingMode.MangaMode => "horizontal-rtl",
                ReadingMode.SinglePage => "horizontal-ltr",
                ReadingMode.DoublePage => "horizontal-ltr",
                ReadingMode.HorizontalRTL => "horizontal-rtl",
                ReadingMode.HorizontalLTR => "horizontal-ltr",
                ReadingMode.Vertical => "vertical",
                _ => "vertical"
            };
        }

        /// <summary>
        /// ReadingDirection değerinden uygun ReadingMode değerini döndürür
        /// </summary>
        public ReadingMode ConvertDirectionToReadingMode(string direction)
        {
            return direction switch
            {
                "vertical" => ReadingMode.Continuous,
                "horizontal-rtl" => ReadingMode.MangaMode,
                "horizontal-ltr" => ReadingMode.SinglePage,
                _ => ReadingMode.Continuous
            };
        }

        /// <summary>
        /// Kullanıcı tercihlerine göre ReadingMode döndürür
        /// </summary>
        public async Task<ReadingMode> GetUserReadingModeAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return ReadingMode.Continuous; // Varsayılan mod

                var preferences = await GetUserPreferencesAsync(userId);
                if (preferences == null)
                    return ReadingMode.Continuous;

                return ConvertDirectionToReadingMode(preferences.ReadingDirection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı okuma modu alınırken hata: {UserId}", userId);
                return ReadingMode.Continuous; // Hata durumunda varsayılan
            }
        }

        public Task UpdateUserPreferencesAsync(UserPreferences preferences)
        {
            throw new NotImplementedException();
        }
    }
} 