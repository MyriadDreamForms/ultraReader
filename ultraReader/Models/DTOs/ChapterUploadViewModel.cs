using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ultraReader.Models.DTOs
{
    public class ChapterUploadViewModel
    {
        [Required(ErrorMessage = "Webtoon ID gereklidir.")]
        public string WebtoonId { get; set; }
        
        [Display(Name = "Webtoon Başlığı")]
        public string WebtoonTitle { get; set; }
        
        [Display(Name = "Webtoon Klasör Adı")]
        public string SafeFolderName { get; set; }
        
        [Required(ErrorMessage = "Bölüm adı (klasör adı) gereklidir.")]
        [Display(Name = "Bölüm Adı (Klasör)")]
        public string ChapterName { get; set; }
        
        [Display(Name = "Bölüm Numarası")]
        [Required(ErrorMessage = "Bölüm numarası gereklidir.")]
        public string ChapterNumber { get; set; } = "001";
        
        [Display(Name = "Bölüm Başlığı")]
        public string ChapterTitle { get; set; }
        
        [Display(Name = "Bölüm Açıklaması")]
        public string ChapterDescription { get; set; }
        
        [Required(ErrorMessage = "En az bir resim yüklemelisiniz.")]
        [Display(Name = "Resimler")]
        public List<IFormFile> Images { get; set; }
        
        [Display(Name = "Yayınla")]
        public bool IsPublished { get; set; } = true;
    }
} 