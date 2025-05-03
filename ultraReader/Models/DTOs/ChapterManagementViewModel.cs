using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class ChapterManagementViewModel
    {
        [Display(Name = "Webtoon ID")]
        public string WebtoonId { get; set; }
        
        [Display(Name = "Webtoon Başlığı")]
        public string WebtoonTitle { get; set; }
        
        [Display(Name = "Webtoon")]
        public WebtoonInfo Webtoon { get; set; }
        
        [Display(Name = "Güvenli Klasör Adı")]
        public string SafeFolderName { get; set; }
        
        [Display(Name = "Bölümler")]
        public List<string> Chapters { get; set; } = new List<string>();
        
        [Display(Name = "Bölüm Detayları")]
        public List<ChapterViewModel> ChapterDetails { get; set; } = new List<ChapterViewModel>();
    }
} 