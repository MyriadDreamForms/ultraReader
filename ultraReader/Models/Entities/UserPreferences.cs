using System;
using System.Collections.Generic;

namespace ultraReader.Models.Entities
{
    public class UserPreferences
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        
        // Görünüm tercihleri
        public string Theme { get; set; } = "light"; // light, dark, sepia
        public string ReadingDirection { get; set; } = "vertical"; // vertical, horizontal-rtl, horizontal-ltr
        public int PageSize { get; set; } = 10;
        public bool AutoScroll { get; set; } = false;
        public int AutoScrollSpeed { get; set; } = 5; // 1-10 arası
        
        // Bildirim tercihleri
        public bool EmailNotifications { get; set; } = true;
        public bool NewChapterNotifications { get; set; } = true;
        public bool CommentNotifications { get; set; } = true;
        
        // Webtoon tercihleri
        public List<string> PreferredGenres { get; set; }
        public bool HideCompletedWebtoons { get; set; } = false;
        
        // Kayıt bilgileri
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public UserPreferences()
        {
            Id = Guid.NewGuid().ToString();
            PreferredGenres = new List<string>();
        }
    }
} 