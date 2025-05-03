using System;

namespace ultraReader.Models.Entities
{
    public class Comment
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string WebtoonId { get; set; }
        public string ChapterId { get; set; } // Eğer bölüme yorum yapılıyorsa, null değilse bölüm yorumu
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int Rating { get; set; } = 5; // Yorum ile birlikte verilen puan (1-5 arası)
        
        // Navigation property
        public WebtoonInfo Webtoon { get; set; }
    }
} 