using System;
using System.Collections.Generic;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class Webtoon
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string FolderName { get; set; }
        public string CoverImage { get; set; }
        public string Status { get; set; }
        public List<string> Genres { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        
        // WebtoonInfo'dan dönüştürme metodu
        public static Webtoon FromWebtoonInfo(WebtoonInfo info)
        {
            if (info == null) return null;
            
            return new Webtoon
            {
                Id = info.Id,
                Title = info.Title,
                Author = info.Author,
                Description = info.Description,
                FolderName = info.FolderName,
                CoverImage = info.CoverImage,
                Status = info.Status,
                Genres = info.Genres,
                IsApproved = info.IsApproved,
                CreatedAt = info.CreatedAt,
                CreatedBy = info.CreatedBy,
                UpdatedAt = info.UpdatedAt,
                UpdatedBy = info.UpdatedBy
            };
        }
    }
}