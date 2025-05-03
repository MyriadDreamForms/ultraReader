using System;
using System.Collections.Generic;

namespace ultraReader.Models.Entities
{
    public class ChapterInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string WebtoonId { get; set; }
        public string ChapterName { get; set; }
        public string FolderName { get { return SafeChapterName ?? ChapterName?.Replace(" ", "_"); } }
        public string SafeChapterName { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public DateTime UploadedAt { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.Now;
        public bool IsPublished { get; set; } = true;
        public int ChapterNumber { get; set; } = 1;
        public string Title { get; set; }
        public string ChapterTitle { get; set; }
        public string ChapterDescription { get; set; }
        public int Views { get; set; } = 0;
        public int Order { get; set; } = 0;
    }
} 