using System;
using System.Collections.Generic;

namespace ultraReader.Models.Entities
{
    public class Webtoon
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string FolderName { get; set; } = null!;
        public string CoverImage { get; set; } = null!;
        public string Status { get; set; } = null!;
        public List<string> Genres { get; set; } = null!;
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = null!;
        
        public Webtoon()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            Genres = new List<string>();
        }
    }
} 