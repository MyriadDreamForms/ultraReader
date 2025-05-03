using System;

namespace ultraReader.Models.Entities
{
    public class ReadingListItem
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string WebtoonId { get; set; }
        public string CurrentChapterId { get; set; }
        public int CurrentPage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public ReadingListItem()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 