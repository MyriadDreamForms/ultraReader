using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.DTOs
{
    public class WebtoonCreateViewModel
    {
        [Required(ErrorMessage = "Başlık gereklidir.")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Yazar bilgisi gereklidir.")]
        [Display(Name = "Yazar")]
        public string Author { get; set; }
        
        [Required(ErrorMessage = "Açıklama gereklidir.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Durum bilgisi gereklidir.")]
        [Display(Name = "Durum")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Tür bilgisi gereklidir.")]
        [Display(Name = "Türler (virgülle ayırın)")]
        public string Genres { get; set; }

        [Display(Name = "Kapak Görseli")]
        public string CoverImage { get; set; }
    }
} 