using System;

namespace ultraReader.Models.Entities
{
    public class UserFavorite
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string WebtoonId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public UserFavorite()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
} 