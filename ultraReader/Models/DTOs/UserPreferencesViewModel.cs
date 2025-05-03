using System.Collections.Generic;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class UserPreferencesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        
        // Görünüm tercihleri
        public string Theme { get; set; } // light, dark
        public string ReadingDirection { get; set; } // vertical, horizontal-rtl, horizontal-ltr
        public int PageSize { get; set; } = 10; // Sayfa başına gösterilen içerik sayısı
        public bool AutoScroll { get; set; } = false;
        public int AutoScrollSpeed { get; set; } = 5; // 1-10 arası
        
        // Bildirim tercihleri
        public bool EmailNotifications { get; set; } = true;
        public bool NewChapterNotifications { get; set; } = true;
        public bool CommentNotifications { get; set; } = true;
        
        // Favori webtoonlar ve okuma listesi
        public List<Webtoon> FavoriteWebtoons { get; set; }
        public List<Webtoon> ReadingList { get; set; }
        
        // Tercihler.cshtml sayfasında kullanılan özellikler
        public List<WebtoonInfo> Favorites { get; set; }
        
        public class ReadingHistoryItem
        {
            public string WebtoonTitle { get; set; }
            public string WebtoonFolder { get; set; }
            public string WebtoonCover { get; set; }
            public string ChapterName { get; set; }
            public System.DateTime LastReadDate { get; set; }
        }
        
        public List<ReadingHistoryItem> ReadingHistory { get; set; }
        
        // Webtoon tercihleri
        public List<string> PreferredGenres { get; set; }
        public bool HideCompletedWebtoons { get; set; } = false;
        
        public UserPreferencesViewModel()
        {
            FavoriteWebtoons = new List<Webtoon>();
            ReadingList = new List<Webtoon>();
            PreferredGenres = new List<string>();
            Favorites = new List<WebtoonInfo>();
            ReadingHistory = new List<ReadingHistoryItem>();
        }
    }
} 