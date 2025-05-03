using System;

namespace ultraReader.Models.Entities
{
    public class PageView
    {
        public string Id { get; set; }
        public string WebtoonId { get; set; }
        public string ChapterId { get; set; }
        public string UserId { get; set; }
        public string UserIp { get; set; }
        public string UserAgent { get; set; }
        public DateTime ViewedAt { get; set; }
        public int DurationSeconds { get; set; }
        
        public PageView()
        {
            Id = Guid.NewGuid().ToString();
            ViewedAt = DateTime.UtcNow;
        }
    }
} 