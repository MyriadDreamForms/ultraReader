using System;
using System.Collections.Generic;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class WebtoonDetailsViewModel
    {
        public WebtoonInfo Webtoon { get; set; }
        public string CoverImageUrl { get; set; }
        public List<ChapterSummary> Chapters { get; set; } = new List<ChapterSummary>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int TotalViews { get; set; }
        public double AverageRating { get; set; }
        public int UserRating { get; set; }
    }

    public class ChapterSummary
    {
        public string FolderName { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
        public int Views { get; set; }
        public DateTime PublishedAt { get; set; }
    }
} 