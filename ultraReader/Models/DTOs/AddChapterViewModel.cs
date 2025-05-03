using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ultraReader.Models.DTOs
{
    public class AddChapterViewModel
    {
        [Required(ErrorMessage = "Webtoon adı gereklidir")]
        public string WebtoonName { get; set; }
        
        [Required(ErrorMessage = "Bölüm adı gereklidir")]
        [Display(Name = "Bölüm Adı")]
        public string ChapterName { get; set; }
        
        [Required(ErrorMessage = "En az bir resim eklemelisiniz")]
        [Display(Name = "Bölüm Resimleri")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        
        [Display(Name = "Açıklama")]
        public string Description { get; set; }
    }
} 