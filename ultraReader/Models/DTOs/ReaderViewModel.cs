using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ultraReader.Models.Entities;
using ultraReader.Models.Enums;

namespace ultraReader.Models.DTOs
{
    public class ReaderViewModel
    {
        public WebtoonInfo WebtoonInfo { get; set; }
        public string CurrentChapter { get; set; }
        public List<string> Images { get; set; }
        public string PreviousChapter { get; set; }
        public string NextChapter { get; set; }
        
        // Okuma sayfası için yeni özellikler
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => Images?.Count ?? 0;
        
        // Okuma modu seçenekleri
        [Display(Name = "Okuma Modu")]
        public ReadingMode ReadingMode { get; set; } = ReadingMode.Continuous;
        
        // Okuma ilerleme durumu
        public bool IsCompleted { get; set; }
        public double ReadingProgress => TotalPages > 0 ? (double)CurrentPage / TotalPages * 100 : 0;
        
        // Kaydedilen okuma pozisyonu
        public bool HasSavedPosition { get; set; }
    }
} 