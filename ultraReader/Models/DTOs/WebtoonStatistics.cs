using System;

namespace ultraReader.Models.DTOs
{
    public class WebtoonStatistics
    {
        public string WebtoonId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string CoverImage { get; set; }
        public string FolderName { get; set; }
        
        // İstatistikler
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public int CompletionCount { get; set; }
        public int ChapterCount { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        
        // Trendler
        public double DailyViewChange { get; set; } // Yüzde olarak
        public double WeeklyViewChange { get; set; } // Yüzde olarak
        public double MonthlyViewChange { get; set; } // Yüzde olarak
    }
    
    public class WebtoonRatingStatistics
    {
        public string WebtoonId { get; set; }
        public string Title { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public string CoverImage { get; set; }
        public string FolderName { get; set; }
    }
    
    public class GenreTrend
    {
        public string Genre { get; set; }
        public int CurrentPopularity { get; set; }
        public double ChangePercentage { get; set; }
        public DateTime Date { get; set; }
    }
    
    public class UserActivityStatistics
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int TotalWebtoonsViewed { get; set; }
        public int TotalChaptersRead { get; set; }
        public int CompletedWebtoonsCount { get; set; }
        public int FavoritesCount { get; set; }
        public DateTime LastActive { get; set; }
        public TimeSpan AverageReadingTime { get; set; }
        public List<string> FavoriteGenres { get; set; } = new List<string>();
    }
    
    public class UserActivitySummary
    {
        public double AverageWebtoonsPerUser { get; set; }
        public double AverageChaptersPerUser { get; set; }
        public double AverageCompletionRate { get; set; } // Yüzde olarak
        public TimeSpan AverageSessionDuration { get; set; }
        public int ActiveUsersLast24Hours { get; set; }
        public int ActiveUsersLast7Days { get; set; }
        public int ActiveUsersLast30Days { get; set; }
    }
} 