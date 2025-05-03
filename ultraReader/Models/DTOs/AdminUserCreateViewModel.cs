using System.ComponentModel.DataAnnotations;

namespace ultraReader.Models.DTOs
{
    public class AdminUserCreateViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
        
        [Display(Name = "Yönetici Olarak Ekle")]
        public bool IsAdmin { get; set; }
        
        [Display(Name = "Moderatör Olarak Ekle")]
        public bool IsModerator { get; set; }
    }
} 