using System;
using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.DTOs
{
    public class ChapterViewModel
    {
        [Display(Name = "Bölüm Adı")]
        public string ChapterName { get; set; }
        
        [Display(Name = "Güvenli Bölüm Adı (Klasör)")]
        public string SafeChapterName { get; set; }
        
        [Display(Name = "Resim Sayısı")]
        public int ImageCount { get; set; }
        
        [Display(Name = "Son Güncelleme")]
        public DateTime LastModified { get; set; }
    }
} 