using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using ultraReader.Data;
using ultraReader.Models.Entities;
using ultraReader.Models.DTOs;

namespace ultraReader.Services
{
    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly IWebtoonService _webtoonService;
        // Standart cache süreleri
        private static readonly TimeSpan ShortCacheTime = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromHours(1);
        private static readonly TimeSpan LongCacheTime = TimeSpan.FromHours(3);

        public UserPreferencesService(
            ApplicationDbContext context,
            IDistributedCache distributedCache,
            IWebtoonService webtoonService)
        {
            _context = context;
            _distributedCache = distributedCache;
            _webtoonService = webtoonService;
        }

        #region Kullanıcı Tercihleri

        public async Task<UserPreferences> GetUserPreferencesAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new UserPreferences(); // Varsayılan tercihler
                
            // Önbellekten kontrol et
            string cacheKey = $"UserPreferences_{userId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<UserPreferences>(cachedData);
            }

            // Veritabanından kontrol et (Include kullanarak ilişkili verileri tek sorguda getir)
            var preferences = await _context.UserPreferences
                .AsNoTracking() // İzlemeyi devre dışı bırakarak performans artışı
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Kullanıcı tercihi yoksa yeni oluştur
            if (preferences == null)
            {
                preferences = new UserPreferences
                {
                    UserId = userId
                };

                await _context.UserPreferences.AddAsync(preferences);
                await _context.SaveChangesAsync();
            }

            // Önbelleğe al (1 saat yerine 3 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(LongCacheTime);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(preferences),
                cacheOptions);

            return preferences;
        }

        public async Task<bool> UpdateUserPreferencesAsync(UserPreferences preferences)
        {
            if (preferences == null)
                return false;
                
            preferences.UpdatedAt = DateTime.UtcNow;

            _context.UserPreferences.Update(preferences);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // Önbelleği güncelle
                string cacheKey = $"UserPreferences_{preferences.UserId}";
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(LongCacheTime);

                await _distributedCache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(preferences),
                    cacheOptions);
            }

            return result;
        }

        #endregion

        #region Favoriler

        public async Task<List<Models.DTOs.Webtoon>> GetFavoriteWebtoonsAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<Models.DTOs.Webtoon>();
                
            // Önbellekten kontrol et
            string cacheKey = $"FavoriteWebtoons_{userId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<Models.DTOs.Webtoon>>(cachedData);
            }

            // Veritabanından favorileri ve webtoon bilgilerini tek sorguda çek
            var favoriteIds = await _context.FavoriteWebtoons
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.WebtoonId)
                .ToListAsync();

            // Performans iyileştirmesi: Tüm webtoon bilgilerini bir seferde çek
            var webtoonInfos = await Task.WhenAll(
                favoriteIds.Select(id => _webtoonService.GetWebtoonByIdAsync(id))
            );

            // Null kontrolü ve dönüştürme
            var favoriteWebtoons = webtoonInfos
                .Where(w => w != null)
                .Select(w => Models.DTOs.Webtoon.FromWebtoonInfo(w))
                .ToList();

            // Önbelleğe al (daha uzun süre)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DefaultCacheTime);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(favoriteWebtoons),
                cacheOptions);

            return favoriteWebtoons;
        }

        public async Task<bool> AddFavoriteWebtoonAsync(string? userId, string? webtoonId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId))
                return false;
                
            // Zaten ekli mi kontrol et
            var existingFavorite = await _context.FavoriteWebtoons
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.WebtoonId == webtoonId);

            if (existingFavorite != null)
            {
                return true; // Zaten ekli
            }

            // Ekle
            var userFavorite = new FavoriteWebtoon
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                WebtoonId = webtoonId
            };

            await _context.FavoriteWebtoons.AddAsync(userFavorite);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // İlgili tüm önbellekleri temizle
                string favoriteListCacheKey = $"FavoriteWebtoons_{userId}";
                string isFavoriteCacheKey = $"IsFavorite_{userId}_{webtoonId}";
                await _distributedCache.RemoveAsync(favoriteListCacheKey);
                await _distributedCache.RemoveAsync(isFavoriteCacheKey);
            }

            return result;
        }

        public async Task<bool> RemoveFavoriteWebtoonAsync(string? userId, string? webtoonId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId))
                return false;
                
            var favorite = await _context.FavoriteWebtoons
                .FirstOrDefaultAsync(f => f.UserId == userId && f.WebtoonId == webtoonId);

            if (favorite == null)
            {
                return false;
            }

            _context.FavoriteWebtoons.Remove(favorite);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // İlgili tüm önbellekleri temizle
                string favoriteListCacheKey = $"FavoriteWebtoons_{userId}";
                string isFavoriteCacheKey = $"IsFavorite_{userId}_{webtoonId}";
                await _distributedCache.RemoveAsync(favoriteListCacheKey);
                await _distributedCache.RemoveAsync(isFavoriteCacheKey);
            }

            return result;
        }

        public async Task<bool> IsFavoriteWebtoonAsync(string? userId, string? webtoonId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId))
                return false;
                
            // Bu işlem sık yapılacağından önbelleğe alıyoruz
            string cacheKey = $"IsFavorite_{userId}_{webtoonId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<bool>(cachedData);
            }
            
            bool isFavorite = await _context.FavoriteWebtoons
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.WebtoonId == webtoonId);
                
            // Sonucu önbelleğe al (kısa süre - favori durumu değişebilir)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(ShortCacheTime);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(isFavorite),
                cacheOptions);
                
            return isFavorite;
        }

        #endregion

        #region Okuma Listesi

        public async Task<List<Models.DTOs.Webtoon>> GetReadingListAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<Models.DTOs.Webtoon>();
                
            // Önbellekten kontrol et
            string cacheKey = $"ReadingList_{userId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<Models.DTOs.Webtoon>>(cachedData);
            }

            // Veritabanından tek bir sorguda benzersiz webtoon ID'lerini getir
            var readingListIds = await _context.ReadingList
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Select(r => r.WebtoonId)
                .Distinct()
                .ToListAsync();

            // Tüm webtoon bilgilerini paralel olarak getir
            var webtoonInfos = await Task.WhenAll(
                readingListIds.Select(id => _webtoonService.GetWebtoonByIdAsync(id))
            );

            // Null kontrolü ve dönüştürme
            var readingList = webtoonInfos
                .Where(w => w != null)
                .Select(w => Models.DTOs.Webtoon.FromWebtoonInfo(w))
                .ToList();

            // Önbelleğe al (daha uzun süre)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DefaultCacheTime);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(readingList),
                cacheOptions);

            return readingList;
        }

        public async Task<bool> AddToReadingListAsync(string? userId, string? webtoonId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId))
                return false;
                
            // Zaten ekli mi kontrol et
            var existingItem = await _context.ReadingList
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId && r.WebtoonId == webtoonId);

            if (existingItem != null)
            {
                return true; // Zaten ekli
            }

            // Ekle
            var readingListItem = new ReadingListItem
            {
                UserId = userId,
                WebtoonId = webtoonId
            };

            await _context.ReadingList.AddAsync(readingListItem);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // İlgili tüm önbellekleri temizle
                string readingListCacheKey = $"ReadingList_{userId}";
                string isInReadingListCacheKey = $"IsInReadingList_{userId}_{webtoonId}";
                await _distributedCache.RemoveAsync(readingListCacheKey);
                await _distributedCache.RemoveAsync(isInReadingListCacheKey);
            }

            return result;
        }

        public async Task<bool> RemoveFromReadingListAsync(string? userId, string? webtoonId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId))
                return false;
                
            var item = await _context.ReadingList
                .FirstOrDefaultAsync(r => r.UserId == userId && r.WebtoonId == webtoonId);

            if (item == null)
            {
                return false;
            }

            _context.ReadingList.Remove(item);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // İlgili tüm önbellekleri temizle
                string readingListCacheKey = $"ReadingList_{userId}";
                string isInReadingListCacheKey = $"IsInReadingList_{userId}_{webtoonId}";
                await _distributedCache.RemoveAsync(readingListCacheKey);
                await _distributedCache.RemoveAsync(isInReadingListCacheKey);
            }

            return result;
        }

        public async Task<bool> IsInReadingListAsync(string? userId, string? webtoonId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId))
                return false;
                
            // Bu işlem sık yapılacağından önbelleğe alıyoruz
            string cacheKey = $"IsInReadingList_{userId}_{webtoonId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<bool>(cachedData);
            }
            
            bool isInReadingList = await _context.ReadingList
                .AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.WebtoonId == webtoonId);
                
            // Sonucu önbelleğe al (kısa süre)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(ShortCacheTime);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(isInReadingList),
                cacheOptions);
                
            return isInReadingList;
        }

        #endregion

        #region Okuma Geçmişi

        public async Task<List<ReadingListItem>> GetReadingHistoryAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<ReadingListItem>();
                
            // Okuma geçmişi için önbellek kullanabiliriz
            string cacheKey = $"ReadingHistory_{userId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<ReadingListItem>>(cachedData);
            }
            
            var history = await _context.ReadingList
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
                
            // Önbelleğe al (kısa süre - okuma geçmişi sık güncellenir)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(ShortCacheTime);

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(history),
                cacheOptions);
                
            return history;
        }
        
        public async Task<ReadingListItem?> GetReadingProgressAsync(string? userId, string? webtoonId, string? chapterId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId) || string.IsNullOrEmpty(chapterId))
                return null;
            
            // Belirli bir bölüm ilerlemesi için önbellek
            string cacheKey = $"ReadingProgress_{userId}_{webtoonId}_{chapterId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<ReadingListItem>(cachedData);
            }
            
            // Kullanıcının bir webtoon ve bölüm için okuma ilerlemesini getir
            var progress = await _context.ReadingList
                .AsNoTracking()
                .FirstOrDefaultAsync(r => 
                    r.UserId == userId && 
                    r.WebtoonId == webtoonId && 
                    r.CurrentChapterId == chapterId);
                    
            if (progress != null)
            {
                // Önbelleğe al (kısa süre - ilerleme sık değişebilir)
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(ShortCacheTime);
    
                await _distributedCache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(progress),
                    cacheOptions);
            }
            
            return progress;
        }

        public async Task<bool> UpdateReadingProgressAsync(string? userId, string? webtoonId, string? chapterId, int pageNumber, bool isCompleted)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId) || string.IsNullOrEmpty(chapterId))
                return false;
                
            var item = await _context.ReadingList
                .FirstOrDefaultAsync(r => r.UserId == userId && r.WebtoonId == webtoonId);

            if (item == null)
            {
                // Okuma listesinde yoksa yeni ekle
                item = new ReadingListItem
                {
                    UserId = userId,
                    WebtoonId = webtoonId,
                    CurrentChapterId = chapterId,
                    CurrentPage = pageNumber,
                    IsCompleted = isCompleted
                };

                await _context.ReadingList.AddAsync(item);
            }
            else
            {
                // Mevcut kaydı güncelle
                item.CurrentChapterId = chapterId;
                item.CurrentPage = pageNumber;
                item.IsCompleted = isCompleted;
                item.UpdatedAt = DateTime.UtcNow;

                _context.ReadingList.Update(item);
            }

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // İlgili tüm önbellekleri temizle
                string readingListCacheKey = $"ReadingList_{userId}";
                string readingHistoryCacheKey = $"ReadingHistory_{userId}";
                string readingProgressCacheKey = $"ReadingProgress_{userId}_{webtoonId}_{chapterId}";
                
                await _distributedCache.RemoveAsync(readingListCacheKey);
                await _distributedCache.RemoveAsync(readingHistoryCacheKey);
                await _distributedCache.RemoveAsync(readingProgressCacheKey);
            }

            return result;
        }

        #endregion

        #region Legacy Metodlar

        public async Task<List<Models.DTOs.Webtoon>> GetUserFavoritesAsync(string? userId)
        {
            return await GetFavoriteWebtoonsAsync(userId);
        }

        public async Task<bool> AddToFavoritesAsync(string? userId, string? webtoonId)
        {
            return await AddFavoriteWebtoonAsync(userId, webtoonId);
        }

        public async Task<bool> RemoveFromFavoritesAsync(string? userId, string? webtoonId)
        {
            return await RemoveFavoriteWebtoonAsync(userId, webtoonId);
        }

        #endregion

        #region FavoriteWebtoon ve ReadingHistory Yardımcı Metodları
        
        // Bu metodlar ReadingHistory tablosu ile ilgili operasyonlar için
        private async Task<List<ReadingHistory>> GetUserReadingHistoryDetailAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<ReadingHistory>();
                
            return await _context.ReadingHistories
                .AsNoTracking()
                .Include(r => r.Webtoon)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.LastReadAt)
                .ToListAsync();
        }

        private async Task<ReadingHistory?> GetSpecificReadingHistoryAsync(string? userId, string? webtoonId, string? chapterId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId) || string.IsNullOrEmpty(chapterId))
                return null;
                
            return await _context.ReadingHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(r => 
                    r.UserId == userId && 
                    r.WebtoonId == webtoonId && 
                    r.ChapterId == chapterId);
        }

        private async Task UpdateSpecificReadingHistoryAsync(string? userId, string? webtoonId, string? chapterId, int lastReadPage, bool isCompleted)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(webtoonId) || string.IsNullOrEmpty(chapterId))
                return;
                
            var history = await _context.ReadingHistories
                .FirstOrDefaultAsync(r => 
                    r.UserId == userId && 
                    r.WebtoonId == webtoonId && 
                    r.ChapterId == chapterId);

            if (history == null)
            {
                history = new ReadingHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    WebtoonId = webtoonId,
                    ChapterId = chapterId,
                    LastReadPage = lastReadPage,
                    IsCompleted = isCompleted,
                    LastReadAt = DateTime.UtcNow
                };
                await _context.ReadingHistories.AddAsync(history);
            }
            else
            {
                history.LastReadPage = lastReadPage;
                history.IsCompleted = isCompleted;
                history.LastReadAt = DateTime.UtcNow;
                _context.ReadingHistories.Update(history);
            }

            await _context.SaveChangesAsync();
            
            // Okuma geçmişi güncellendiğinden ilgili önbelleği temizle
            string cacheKey = $"ReadingHistoryDetail_{userId}";
            await _distributedCache.RemoveAsync(cacheKey);
        }
        #endregion
    }
} 