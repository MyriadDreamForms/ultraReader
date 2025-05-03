using System;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class ReadingHistoryViewModel
    {
        public ReadingListItem? ReadingListItem { get; set; }
        public Webtoon? Webtoon { get; set; }
    }
} 