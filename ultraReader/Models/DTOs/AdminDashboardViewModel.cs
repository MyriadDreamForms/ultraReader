using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using ultraReader.Models.Entities;

namespace ultraReader.Models.DTOs
{
    public class AdminDashboardViewModel
    {
        // İstatistikler
        public int TotalWebtoons { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalModerators { get; set; }
        public int TotalComments { get; set; }
        public int PendingComments { get; set; }
        public int TotalChapters { get; set; }
        public int TotalViews { get; set; }
        public string DiskUsage { get; set; }
        public int ActiveUsers { get; set; }
        public System.DateTime? LastBackup { get; set; }
        
        // Son eklenen webtoonlar
        public List<Models.DTOs.Webtoon> RecentWebtoons { get; set; }
        
        // Son kaydolan kullanıcılar
        public List<IdentityUser> LatestUsers { get; set; }
        
        // Grafikler için yeni istatistikler
        public Dictionary<string, int> WebtoonsByGenre { get; set; }
        public Dictionary<string, int> WebtoonsByStatus { get; set; }
        public Dictionary<string, int> CommentsPerDay { get; set; }
        public Dictionary<string, int> NewUsersPerDay { get; set; }
        public Dictionary<string, int> ViewsPerDay { get; set; }
        
        public AdminDashboardViewModel()
        {
            RecentWebtoons = new List<Models.DTOs.Webtoon>();
            LatestUsers = new List<IdentityUser>();
            WebtoonsByGenre = new Dictionary<string, int>();
            WebtoonsByStatus = new Dictionary<string, int>();
            CommentsPerDay = new Dictionary<string, int>();
            NewUsersPerDay = new Dictionary<string, int>();
            ViewsPerDay = new Dictionary<string, int>();
            DiskUsage = "0 MB";
            ActiveUsers = 0;
        }
    }
} 