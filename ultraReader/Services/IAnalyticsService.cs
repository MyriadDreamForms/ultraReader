using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ultraReader.Models.Entities;
using ultraReader.Models.DTOs;

namespace ultraReader.Services
{
    public interface IAnalyticsService
    {
        // Popüler Webtoon İstatistikleri
        Task<List<WebtoonStatistics>> GetMostViewedWebtoonsAsync(int count = 10);
        Task<List<WebtoonStatistics>> GetMostFavoritedWebtoonsAsync(int count = 10);
        Task<List<WebtoonStatistics>> GetMostCompletedWebtoonsAsync(int count = 10);
        Task<List<WebtoonRatingStatistics>> GetHighestRatedWebtoonsAsync(int count = 10);
        
        // Tür İstatistikleri
        Task<Dictionary<string, int>> GetWebtoonsByGenreAsync();
        Task<List<GenreTrend>> GetGenreTrendsAsync(int days = 30);
        
        // Kullanıcı Davranışları
        Task<UserActivityStatistics> GetUserActivityAsync(string userId);
        Task<UserActivitySummary> GetAverageUserActivityAsync();
        Task<Dictionary<string, int>> GetReadingTimesByHourAsync();
        Task<Dictionary<DateTime, int>> GetDailyActivityAsync(int days = 30);
        
        // Trafik İzleme
        Task<int> GetActiveReadersCountAsync();
        Task<int> IncrementPageViewAsync(string webtoonId, string chapterId);
        Task<int> GetTotalPageViewsAsync(string webtoonId);
        
        // İzleme Olayları
        Task TrackWebtoonViewAsync(string userId, string webtoonId);
        Task TrackChapterViewAsync(string userId, string webtoonId, string chapterId);
        Task TrackSearchQueryAsync(string userId, string query);
        Task TrackUserLoginAsync(string userId);
    }
} 