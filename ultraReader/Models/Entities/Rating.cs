using System;

namespace ultraReader.Models.Entities
{
    public class Rating
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string WebtoonId { get; set; }
        public int Value { get; set; } // 1-5 arası değer
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public WebtoonInfo Webtoon { get; set; }
    }
} 