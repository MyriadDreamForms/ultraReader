using System;

namespace ultraReader.Models.Entities
{
    public class FavoriteWebtoon
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string WebtoonId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public WebtoonInfo Webtoon { get; set; }
    }
} 