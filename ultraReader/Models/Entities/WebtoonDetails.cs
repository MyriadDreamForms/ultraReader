using System.Collections.Generic;

namespace ultraReader.Models.Entities
{
    public class WebtoonDetails
    {
        public string FolderName { get; set; }
        public List<string> Chapters { get; set; } = new List<string>();
        public string CoverImage { get; set; }
        public Dictionary<string, int> ChapterImageCounts { get; set; } = new Dictionary<string, int>();
    }
} 