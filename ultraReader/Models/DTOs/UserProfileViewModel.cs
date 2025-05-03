using System.Collections.Generic;

namespace ultraReader.Models.DTOs
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public int FavoritesCount { get; set; }
        public int ReadingListCount { get; set; }
        public int HistoryCount { get; set; }
    }
} 