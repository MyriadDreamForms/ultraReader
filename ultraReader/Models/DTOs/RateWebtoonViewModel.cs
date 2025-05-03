using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.DTOs
{
    public class RateWebtoonViewModel
    {
        [Required]
        public string WebtoonId { get; set; }
        
        [Required]
        public string WebtoonFolderName { get; set; }
        
        [Required]
        [Range(1, 5, ErrorMessage = "Lütfen 1-5 arası bir değer girin")]
        public int Rating { get; set; }
    }
} 