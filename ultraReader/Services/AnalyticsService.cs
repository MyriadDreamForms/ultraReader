using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using ultraReader.Data;
using ultraReader.Models.Entities;
using ultraReader.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace ultraReader.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnalyticsService(
            ApplicationDbContext context,
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _distributedCache = distributedCache;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Popüler Webtoon İstatistikleri

        public async Task<List<WebtoonStatistics>> GetMostViewedWebtoonsAsync(int count = 10)
        {
            // Önbellekten kontrol et
            string cacheKey = $"MostViewedWebtoons_{count}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<WebtoonStatistics>>(cachedData);
            }

            // Veritabanından en çok görüntülenen webtoonları getir
            var pageViews = await _context.PageViews
                .Where(pv => pv.ViewedAt >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(pv => pv.WebtoonId)
                .Select(g => new { WebtoonId = g.Key, ViewCount = g.Count() })
                .OrderByDescending(x => x.ViewCount)
                .Take(count)
                .ToListAsync();

            var result = new List<WebtoonStatistics>();

            foreach (var view in pageViews)
            {
                var webtoon = await _context.Webtoons.FindAsync(view.WebtoonId);
                if (webtoon != null)
                {
                    // Günlük, haftalık ve aylık görüntüleme değişimlerini hesapla
                    var dailyViews = await GetViewChangePercentageAsync(view.WebtoonId, 1);
                    var weeklyViews = await GetViewChangePercentageAsync(view.WebtoonId, 7);
                    var monthlyViews = await GetViewChangePercentageAsync(view.WebtoonId, 30);

                    result.Add(new WebtoonStatistics
                    {
                        WebtoonId = webtoon.Id,
                        Title = webtoon.Title,
                        Author = webtoon.Author,
                        CoverImage = webtoon.CoverImage,
                        FolderName = webtoon.FolderName,
                        ViewCount = view.ViewCount,
                        FavoriteCount = await _context.UserFavorites.CountAsync(f => f.WebtoonId == webtoon.Id),
                        CompletionCount = await _context.ReadingList.CountAsync(r => r.WebtoonId == webtoon.Id && r.IsCompleted),
                        DailyViewChange = dailyViews,
                        WeeklyViewChange = weeklyViews,
                        MonthlyViewChange = monthlyViews
                    });
                }
            }

            // Önbelleğe al (1 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                cacheOptions);

            return result;
        }

        public async Task<List<WebtoonStatistics>> GetMostFavoritedWebtoonsAsync(int count = 10)
        {
            // Önbellekten kontrol et
            string cacheKey = $"MostFavoritedWebtoons_{count}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<WebtoonStatistics>>(cachedData);
            }

            // Veritabanından en çok favorilenen webtoonları getir
            var favorites = await _context.UserFavorites
                .GroupBy(f => f.WebtoonId)
                .Select(g => new { WebtoonId = g.Key, FavoriteCount = g.Count() })
                .OrderByDescending(x => x.FavoriteCount)
                .Take(count)
                .ToListAsync();

            var result = new List<WebtoonStatistics>();

            foreach (var fav in favorites)
            {
                var webtoon = await _context.Webtoons.FindAsync(fav.WebtoonId);
                if (webtoon != null)
                {
                    var viewCount = await _context.PageViews.CountAsync(pv => pv.WebtoonId == webtoon.Id);
                    
                    result.Add(new WebtoonStatistics
                    {
                        WebtoonId = webtoon.Id,
                        Title = webtoon.Title,
                        Author = webtoon.Author,
                        CoverImage = webtoon.CoverImage,
                        FolderName = webtoon.FolderName,
                        ViewCount = viewCount,
                        FavoriteCount = fav.FavoriteCount,
                        CompletionCount = await _context.ReadingList.CountAsync(r => r.WebtoonId == webtoon.Id && r.IsCompleted)
                    });
                }
            }

            // Önbelleğe al (1 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                cacheOptions);

            return result;
        }

        public async Task<List<WebtoonStatistics>> GetMostCompletedWebtoonsAsync(int count = 10)
        {
            // Önbellekten kontrol et
            string cacheKey = $"MostCompletedWebtoons_{count}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<WebtoonStatistics>>(cachedData);
            }

            // Veritabanından en çok tamamlanan webtoonları getir
            var completed = await _context.ReadingList
                .Where(r => r.IsCompleted)
                .GroupBy(r => r.WebtoonId)
                .Select(g => new { WebtoonId = g.Key, CompletionCount = g.Count() })
                .OrderByDescending(x => x.CompletionCount)
                .Take(count)
                .ToListAsync();

            var result = new List<WebtoonStatistics>();

            foreach (var item in completed)
            {
                var webtoon = await _context.Webtoons.FindAsync(item.WebtoonId);
                if (webtoon != null)
                {
                    var viewCount = await _context.PageViews.CountAsync(pv => pv.WebtoonId == webtoon.Id);
                    
                    result.Add(new WebtoonStatistics
                    {
                        WebtoonId = webtoon.Id,
                        Title = webtoon.Title,
                        Author = webtoon.Author,
                        CoverImage = webtoon.CoverImage,
                        FolderName = webtoon.FolderName,
                        ViewCount = viewCount,
                        FavoriteCount = await _context.UserFavorites.CountAsync(f => f.WebtoonId == webtoon.Id),
                        CompletionCount = item.CompletionCount
                    });
                }
            }

            // Önbelleğe al (1 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                cacheOptions);

            return result;
        }

        public async Task<List<WebtoonRatingStatistics>> GetHighestRatedWebtoonsAsync(int count = 10)
        {
            // Önbellekten kontrol et
            string cacheKey = $"HighestRatedWebtoons_{count}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<WebtoonRatingStatistics>>(cachedData);
            }

            // Veritabanından en yüksek puanlı webtoonları getir
            var ratings = await _context.Ratings
                .GroupBy(r => r.WebtoonId)
                .Select(g => new { 
                    WebtoonId = g.Key, 
                    AverageRating = g.Average(r => r.Value),
                    RatingCount = g.Count() 
                })
                .Where(r => r.RatingCount >= 5) // En az 5 oylama olsun
                .OrderByDescending(x => x.AverageRating)
                .Take(count)
                .ToListAsync();

            var result = new List<WebtoonRatingStatistics>();

            foreach (var rating in ratings)
            {
                var webtoon = await _context.Webtoons.FindAsync(rating.WebtoonId);
                if (webtoon != null)
                {
                    result.Add(new WebtoonRatingStatistics
                    {
                        WebtoonId = webtoon.Id,
                        Title = webtoon.Title,
                        AverageRating = Math.Round(rating.AverageRating, 1),
                        RatingCount = rating.RatingCount,
                        CoverImage = webtoon.CoverImage,
                        FolderName = webtoon.FolderName
                    });
                }
            }

            // Önbelleğe al (1 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                cacheOptions);

            return result;
        }

        private async Task<double> GetViewChangePercentageAsync(string webtoonId, int days)
        {
            var now = DateTime.UtcNow;
            var periodStart = now.AddDays(-days);
            var previousPeriodStart = periodStart.AddDays(-days);
            
            // Bu dönemdeki görüntülenme sayısı
            var currentViews = await _context.PageViews
                .CountAsync(pv => pv.WebtoonId == webtoonId && 
                           pv.ViewedAt >= periodStart && 
                           pv.ViewedAt <= now);
                           
            // Önceki dönemdeki görüntülenme sayısı
            var previousViews = await _context.PageViews
                .CountAsync(pv => pv.WebtoonId == webtoonId && 
                           pv.ViewedAt >= previousPeriodStart && 
                           pv.ViewedAt < periodStart);
                           
            // Değişim yüzdesini hesapla
            if (previousViews == 0)
            {
                return currentViews > 0 ? 100 : 0; // Önceki dönemde görüntülenme yoksa
            }
            
            return Math.Round(((double)currentViews - previousViews) / previousViews * 100, 2);
        }

        #endregion

        #region Tür İstatistikleri

        public async Task<Dictionary<string, int>> GetWebtoonsByGenreAsync()
        {
            // Önbellekten kontrol et
            string cacheKey = "WebtoonsByGenre";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(cachedData);
            }

            // Tüm türleri ve her türe ait webtoon sayısını getir
            var webtoons = await _context.Webtoons.ToListAsync();
            var genreCounts = new Dictionary<string, int>();

            foreach (var webtoon in webtoons)
            {
                if (webtoon.Genres != null)
                {
                    foreach (var genre in webtoon.Genres)
                    {
                        if (genreCounts.ContainsKey(genre))
                        {
                            genreCounts[genre]++;
                        }
                        else
                        {
                            genreCounts[genre] = 1;
                        }
                    }
                }
            }

            // Önbelleğe al (12 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(genreCounts),
                cacheOptions);

            return genreCounts;
        }

        public async Task<List<GenreTrend>> GetGenreTrendsAsync(int days = 30)
        {
            // Önbellekten kontrol et
            string cacheKey = $"GenreTrends_{days}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<GenreTrend>>(cachedData);
            }

            // Tüm türleri getir
            var allGenres = new HashSet<string>();
            var webtoons = await _context.Webtoons.ToListAsync();
            
            foreach (var webtoon in webtoons)
            {
                if (webtoon.Genres != null)
                {
                    foreach (var genre in webtoon.Genres)
                    {
                        allGenres.Add(genre);
                    }
                }
            }

            var result = new List<GenreTrend>();
            var now = DateTime.UtcNow;
            var startDate = now.AddDays(-days);
            var previousStartDate = startDate.AddDays(-days);

            // Her tür için popülerlik trendini hesapla
            foreach (var genre in allGenres)
            {
                // Türe sahip webtoonların ID'lerini getir
                var genreWebtoonIds = webtoons
                    .Where(w => w.Genres != null && w.Genres.Contains(genre))
                    .Select(w => w.Id)
                    .ToList();

                if (genreWebtoonIds.Any())
                {
                    // Bu dönemdeki görüntülenme sayısı
                    var currentViews = await _context.PageViews
                        .CountAsync(pv => genreWebtoonIds.Contains(pv.WebtoonId) && 
                                  pv.ViewedAt >= startDate && 
                                  pv.ViewedAt <= now);
                                  
                    // Önceki dönemdeki görüntülenme sayısı
                    var previousViews = await _context.PageViews
                        .CountAsync(pv => genreWebtoonIds.Contains(pv.WebtoonId) && 
                                  pv.ViewedAt >= previousStartDate && 
                                  pv.ViewedAt < startDate);
                                  
                    // Değişim yüzdesini hesapla
                    double changePercentage = 0;
                    if (previousViews > 0)
                    {
                        changePercentage = Math.Round(((double)currentViews - previousViews) / previousViews * 100, 2);
                    }
                    else if (currentViews > 0)
                    {
                        changePercentage = 100;
                    }

                    result.Add(new GenreTrend
                    {
                        Genre = genre,
                        CurrentPopularity = currentViews,
                        ChangePercentage = changePercentage,
                        Date = now
                    });
                }
            }

            // Popülerliğe göre sırala
            result = result.OrderByDescending(g => g.CurrentPopularity).ToList();

            // Önbelleğe al (12 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                cacheOptions);

            return result;
        }

        #endregion

        #region Kullanıcı Davranışları

        public async Task<UserActivityStatistics> GetUserActivityAsync(string userId)
        {
            // Önbellekten kontrol et
            string cacheKey = $"UserActivity_{userId}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<UserActivityStatistics>(cachedData);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Kullanıcı etkinliklerini getir
            var activities = await _context.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            var webtoonViews = activities
                .Where(a => a.ActivityType == "WebtoonView")
                .Select(a => a.RelatedId)
                .Distinct()
                .Count();

            var chapterViews = activities
                .Count(a => a.ActivityType == "ChapterView");

            var lastActive = activities.Any() 
                ? activities.First().Timestamp 
                : DateTime.MinValue;

            // Tamamlanan webtoon sayısı
            var completedCount = await _context.ReadingList
                .CountAsync(r => r.UserId == userId && r.IsCompleted);

            // Favori sayısı
            var favoritesCount = await _context.UserFavorites
                .CountAsync(f => f.UserId == userId);

            // Ortalama okuma süresi
            var readingDurations = activities
                .Where(a => a.ActivityType == "ChapterView")
                .Select(a => GetReadingDuration(a.Details))
                .Where(d => d > 0)
                .ToList();

            var averageReadingTime = readingDurations.Any()
                ? TimeSpan.FromSeconds(readingDurations.Average())
                : TimeSpan.Zero;

            // Favori türler
            var favoriteGenres = new List<string>();
            var userPreferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (userPreferences?.PreferredGenres != null)
            {
                favoriteGenres = userPreferences.PreferredGenres;
            }
            else
            {
                // Kullanıcının tercihleri yoksa, en çok okuduğu webtoonların türlerini kullan
                var viewedWebtoonIds = activities
                    .Where(a => a.ActivityType == "WebtoonView")
                    .Select(a => a.RelatedId)
                    .ToList();

                var genres = new Dictionary<string, int>();
                foreach (var webtoonId in viewedWebtoonIds)
                {
                    var webtoon = await _context.Webtoons.FindAsync(webtoonId);
                    if (webtoon?.Genres != null)
                    {
                        foreach (var genre in webtoon.Genres)
                        {
                            if (genres.ContainsKey(genre))
                            {
                                genres[genre]++;
                            }
                            else
                            {
                                genres[genre] = 1;
                            }
                        }
                    }
                }

                favoriteGenres = genres.OrderByDescending(g => g.Value)
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList();
            }

            var result = new UserActivityStatistics
            {
                UserId = userId,
                UserName = user.UserName,
                TotalWebtoonsViewed = webtoonViews,
                TotalChaptersRead = chapterViews,
                CompletedWebtoonsCount = completedCount,
                FavoritesCount = favoritesCount,
                LastActive = lastActive,
                AverageReadingTime = averageReadingTime,
                FavoriteGenres = favoriteGenres
            };

            // Önbelleğe al (3 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(3));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                cacheOptions);

            return result;
        }

        public async Task<UserActivitySummary> GetAverageUserActivityAsync()
        {
            // Önbellekten kontrol et
            string cacheKey = "AverageUserActivity";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<UserActivitySummary>(cachedData);
            }

            var totalUsers = await _context.Users.CountAsync();
            if (totalUsers == 0)
            {
                return new UserActivitySummary();
            }

            // Toplam görüntülenen webtoon ve bölüm sayısı
            var webtoonViewActivities = await _context.UserActivities
                .Where(a => a.ActivityType == "WebtoonView")
                .ToListAsync();

            var chapterViewActivities = await _context.UserActivities
                .Where(a => a.ActivityType == "ChapterView")
                .ToListAsync();

            var uniqueWebtoonsPerUser = webtoonViewActivities
                .GroupBy(a => a.UserId)
                .Select(g => new { 
                    UserId = g.Key, 
                    WebtoonCount = g.Select(a => a.RelatedId).Distinct().Count() 
                })
                .ToList();

            var chaptersPerUser = chapterViewActivities
                .GroupBy(a => a.UserId)
                .Select(g => new { 
                    UserId = g.Key, 
                    ChapterCount = g.Count() 
                })
                .ToList();

            // Ortalama webtoon ve bölüm sayısı
            double averageWebtoons = uniqueWebtoonsPerUser.Any()
                ? uniqueWebtoonsPerUser.Average(u => u.WebtoonCount)
                : 0;

            double averageChapters = chaptersPerUser.Any()
                ? chaptersPerUser.Average(u => u.ChapterCount)
                : 0;

            // Tamamlama oranı
            var totalReadingListItems = await _context.ReadingList.CountAsync();
            var totalCompletedItems = await _context.ReadingList.CountAsync(r => r.IsCompleted);

            double averageCompletionRate = totalReadingListItems > 0
                ? (double)totalCompletedItems / totalReadingListItems * 100
                : 0;

            // Ortalama oturum süresi
            var readingDurations = chapterViewActivities
                .Select(a => GetReadingDuration(a.Details))
                .Where(d => d > 0)
                .ToList();

            var averageSessionDuration = readingDurations.Any()
                ? TimeSpan.FromSeconds(readingDurations.Average())
                : TimeSpan.Zero;

            // Aktif kullanıcılar
            var now = DateTime.UtcNow;
            var activeUsers24h = await _context.UserActivities
                .Where(a => a.Timestamp >= now.AddHours(-24))
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            var activeUsers7d = await _context.UserActivities
                .Where(a => a.Timestamp >= now.AddDays(-7))
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            var activeUsers30d = await _context.UserActivities
                .Where(a => a.Timestamp >= now.AddDays(-30))
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            var result = new UserActivitySummary
            {
                AverageWebtoonsPerUser = Math.Round(averageWebtoons, 2),
                AverageChaptersPerUser = Math.Round(averageChapters, 2),
                AverageCompletionRate = Math.Round(averageCompletionRate, 2),
                AverageSessionDuration = averageSessionDuration,
                ActiveUsersLast24Hours = activeUsers24h,
                ActiveUsersLast7Days = activeUsers7d,
                ActiveUsersLast30Days = activeUsers30d
            };

            // Önbelleğe al (6 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(6));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                cacheOptions);

            return result;
        }

        public async Task<Dictionary<string, int>> GetReadingTimesByHourAsync()
        {
            // Önbellekten kontrol et
            string cacheKey = "ReadingTimesByHour";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(cachedData);
            }

            // Son 30 günlük aktiviteleri al
            var chapterViews = await _context.UserActivities
                .Where(a => a.ActivityType == "ChapterView" && a.Timestamp >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            var readingsByHour = new Dictionary<string, int>();
            for (int i = 0; i < 24; i++)
            {
                readingsByHour[i.ToString()] = 0;
            }

            foreach (var view in chapterViews)
            {
                var hour = view.Timestamp.Hour;
                readingsByHour[hour.ToString()]++;
            }

            // Önbelleğe al (12 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(readingsByHour),
                cacheOptions);

            return readingsByHour;
        }

        public async Task<Dictionary<DateTime, int>> GetDailyActivityAsync(int days = 30)
        {
            // Önbellekten kontrol et
            string cacheKey = $"DailyActivity_{days}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<Dictionary<DateTime, int>>(cachedData);
            }

            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-(days - 1));

            var dailyActivity = new Dictionary<DateTime, int>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                dailyActivity[date] = 0;
            }

            // Tüm aktiviteleri getir
            var activities = await _context.UserActivities
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate.AddDays(1))
                .ToListAsync();

            // Her gün için aktivite sayısını hesapla
            foreach (var activity in activities)
            {
                var activityDate = activity.Timestamp.Date;
                if (dailyActivity.ContainsKey(activityDate))
                {
                    dailyActivity[activityDate]++;
                }
            }

            // Önbelleğe al (12 saat)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(dailyActivity),
                cacheOptions);

            return dailyActivity;
        }

        private int GetReadingDuration(string details)
        {
            try
            {
                if (string.IsNullOrEmpty(details)) return 0;
                
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(details);
                if (data.ContainsKey("durationSeconds"))
                {
                    return Convert.ToInt32(data["durationSeconds"]);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Trafik İzleme

        public async Task<int> GetActiveReadersCountAsync()
        {
            var last5Minutes = DateTime.UtcNow.AddMinutes(-5);
            return await _context.UserActivities
                .Where(a => a.Timestamp >= last5Minutes)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> IncrementPageViewAsync(string webtoonId, string chapterId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            string userId = null;
            
            if (httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }
            
            var pageView = new PageView
            {
                WebtoonId = webtoonId,
                ChapterId = chapterId,
                UserId = userId,
                UserIp = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
            };
            
            await _context.PageViews.AddAsync(pageView);
            await _context.SaveChangesAsync();
            
            return await GetTotalPageViewsAsync(webtoonId);
        }

        public async Task<int> GetTotalPageViewsAsync(string webtoonId)
        {
            return await _context.PageViews.CountAsync(pv => pv.WebtoonId == webtoonId);
        }

        #endregion

        #region İzleme Olayları

        public async Task TrackWebtoonViewAsync(string userId, string webtoonId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = "WebtoonView",
                RelatedId = webtoonId,
                UserIp = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
            };
            
            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task TrackChapterViewAsync(string userId, string webtoonId, string chapterId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = "ChapterView",
                RelatedId = chapterId,
                Details = JsonSerializer.Serialize(new { 
                    webtoonId, 
                    startTime = DateTime.UtcNow,
                    durationSeconds = 0
                }),
                UserIp = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
            };
            
            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task TrackSearchQueryAsync(string userId, string query)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = "Search",
                Details = JsonSerializer.Serialize(new { query }),
                UserIp = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
            };
            
            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task TrackUserLoginAsync(string userId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = "Login",
                UserIp = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
            };
            
            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
} 