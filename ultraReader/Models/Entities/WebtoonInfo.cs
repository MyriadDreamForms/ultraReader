using System;
using System.Collections.Generic;

namespace ultraReader.Models.Entities
{
    public class WebtoonInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public List<string> Genres { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string CoverImage { get; set; }
        public string FolderName { get; set; }
        
        // Oluşturma ve güncelleme bilgileri
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        
        // Onay bilgileri
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovedBy { get; set; }
        
        // Red bilgileri
        public bool IsRejected { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectedBy { get; set; }
        public string RejectReason { get; set; }
        public string CoverImageUrl { get; internal set; }
        public string SafeFolderName { get; internal set; }
    }
} 