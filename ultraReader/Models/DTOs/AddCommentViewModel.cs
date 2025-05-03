using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.DTOs
{
    public class AddCommentViewModel
    {
        [Required]
        public string WebtoonId { get; set; }
        
        public string ChapterId { get; set; } // Bölüm yorumu ise doldurulacak
        
        [Required]
        public string WebtoonFolderName { get; set; }
        
        [Required(ErrorMessage = "Yorum içeriği gereklidir")]
        [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir")]
        [Display(Name = "Yorumunuz")]
        public string Content { get; set; }
        
        [Range(1, 5, ErrorMessage = "Lütfen 1-5 arası bir puan verin")]
        [Display(Name = "Puan")]
        public int Rating { get; set; } = 5;
    }
} 