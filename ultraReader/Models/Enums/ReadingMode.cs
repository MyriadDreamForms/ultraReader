using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.Enums
{
    public enum ReadingMode
    {
        [Display(Name = "Sürekli (Dikey)")]
        Continuous = 0,
        
        [Display(Name = "Tek Sayfa")]
        SinglePage = 1,
        
        [Display(Name = "Çift Sayfa")]
        DoublePage = 2,
        
        [Display(Name = "Manga Modu (Sağdan Sola)")]
        MangaMode = 3,
        
        [Display(Name = "Yatay (Sağdan Sola)")]
        HorizontalRTL = 4,
        
        [Display(Name = "Yatay (Soldan Sağa)")]
        HorizontalLTR = 5,
        
        [Display(Name = "Dikey")]
        Vertical = 6
    }
} 