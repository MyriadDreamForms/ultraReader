using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.DTOs
{
    public class BatchUploadViewModel
    {
        [Required(ErrorMessage = "Webtoon başlığı gereklidir.")]
        [Display(Name = "Webtoon Başlığı")]
        public string WebtoonTitle { get; set; }
        
        [Required(ErrorMessage = "Yazar bilgisi gereklidir.")]
        [Display(Name = "Yazar")]
        public string Author { get; set; }
        
        [Required(ErrorMessage = "Açıklama gereklidir.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Tür bilgisi gereklidir.")]
        [Display(Name = "Türler (virgülle ayırın)")]
        public string Genres { get; set; }
        
        [Required(ErrorMessage = "Durum bilgisi gereklidir.")]
        [Display(Name = "Durum")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Bölüm numarası gereklidir.")]
        [Display(Name = "Bölüm Numarası")]
        public string ChapterNumber { get; set; } = "001";
        
        [Display(Name = "Bölüm Başlığı")]
        public string ChapterTitle { get; set; }
    }
} 