using System;

namespace ultraReader.Models.Entities
{
    public class UserActivity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ActivityType { get; set; } // Login, WebtoonView, ChapterView, Search, Comment, Rate
        public string RelatedId { get; set; } // İlgili webtoon, bölüm, yorum ID'si
        public string Details { get; set; } // JSON veri
        public string UserIp { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        
        public UserActivity()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
        }
    }
} 