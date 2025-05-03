using System;

namespace ultraReader.Models.Entities
{
    public class ReadingHistory
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string WebtoonId { get; set; } = null!;
        public string ChapterId { get; set; } = null!;
        public int LastReadPage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastReadAt { get; set; }
        
        // Navigation properties
        public WebtoonInfo? Webtoon { get; set; }
        
        public ReadingHistory()
        {
            Id = Guid.NewGuid().ToString();
            LastReadAt = DateTime.UtcNow;
        }
    }
} 