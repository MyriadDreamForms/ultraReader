using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ultraReader.Models.DTOs
{
    public class AssignRolesViewModel
    {
        [Required]
        public string UserId { get; set; }
        
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
        
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Display(Name = "Mevcut Roller")]
        public List<string> CurrentRoles { get; set; }
        
        [Display(Name = "Seçilen Rol")]
        public string SelectedRole { get; set; }
        
        [Display(Name = "Roller")]
        public List<SelectListItem> AvailableRoles { get; set; }
        
        [Display(Name = "Yönetici")]
        public bool IsAdmin { get; set; }
        
        [Display(Name = "Moderatör")]
        public bool IsModerator { get; set; }
    }
} 