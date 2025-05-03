using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.DTOs
{
    public class ChapterImageViewModel
    {
        // Mevcut dosya adı
        public string FileName { get; set; }
        
        // Düzenlendikten sonra dosya adı; varsayılan olarak mevcut ad
        [Display(Name = "Yeni Dosya Adı")]
        public string NewName { get; set; }
        
        // Sıra numarası
        [Display(Name = "Sıra Numarası")]
        public int OrderIndex { get; set; }
        
        // Silinmek istenip istenmediğini belirtmek için
        [Display(Name = "Sil")]
        public bool Delete { get; set; }
    }
} 