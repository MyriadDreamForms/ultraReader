using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ultraReader.Models.DTOs
{
    public class EditChapterViewModel
    {
        [Required(ErrorMessage = "Webtoon adı gereklidir")]
        public string WebtoonName { get; set; }
        
        [Display(Name = "Bölüm Adı")]
        [Required(ErrorMessage = "Bölüm adı gereklidir")]
        public string ChapterName { get; set; }
        
        [Display(Name = "Yeni Bölüm Adı")]
        public string NewChapterName { get; set; }
        
        [Display(Name = "Bölüm Numarası")]
        public int ChapterNumber { get; set; }
        
        [Display(Name = "Bölüm Başlığı")]
        public string ChapterTitle { get; set; }
        
        [Display(Name = "Bölüm Açıklaması")]
        public string ChapterDescription { get; set; }
        
        [Display(Name = "Yayında")]
        public bool IsPublished { get; set; } = true;
        
        [Display(Name = "Resimlerin Sıralaması")]
        public bool SortImagesAlphabetically { get; set; } = true;
        
        [Display(Name = "Mevcut Resimler")]
        public List<ChapterImageViewModel> ExistingImages { get; set; } = new List<ChapterImageViewModel>();
        
        [Display(Name = "Yeni Resimler")]
        public List<IFormFile> NewImages { get; set; }
    }
} 