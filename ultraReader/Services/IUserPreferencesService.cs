using System.Collections.Generic;
using System.Threading.Tasks;
using ultraReader.Models.Entities;
using ultraReader.Models.DTOs;

namespace ultraReader.Services
{
    public interface IUserPreferencesService
    {
        // Kullanıcı tercihleri
        Task<UserPreferences> GetUserPreferencesAsync(string? userId);
        Task<bool> UpdateUserPreferencesAsync(UserPreferences preferences);
        
        // Favoriler
        Task<List<Models.DTOs.Webtoon>> GetFavoriteWebtoonsAsync(string? userId);
        Task<bool> AddFavoriteWebtoonAsync(string? userId, string? webtoonId);
        Task<bool> RemoveFavoriteWebtoonAsync(string? userId, string? webtoonId);
        Task<bool> IsFavoriteWebtoonAsync(string? userId, string? webtoonId);
        
        // Okuma listesi
        Task<List<Models.DTOs.Webtoon>> GetReadingListAsync(string? userId);
        Task<bool> AddToReadingListAsync(string? userId, string? webtoonId);
        Task<bool> RemoveFromReadingListAsync(string? userId, string? webtoonId);
        Task<bool> IsInReadingListAsync(string? userId, string? webtoonId);
        
        // Okuma geçmişi
        Task<List<ReadingListItem>> GetReadingHistoryAsync(string? userId);
        Task<ReadingListItem?> GetReadingProgressAsync(string? userId, string? webtoonId, string? chapterId);
        Task<bool> UpdateReadingProgressAsync(string? userId, string? webtoonId, string? chapterId, int pageNumber, bool isCompleted);
        
        // Legacy metodlar
        Task<List<Models.DTOs.Webtoon>> GetUserFavoritesAsync(string? userId);
        Task<bool> AddToFavoritesAsync(string? userId, string? webtoonId);
        Task<bool> RemoveFromFavoritesAsync(string? userId, string? webtoonId);
    }
} 