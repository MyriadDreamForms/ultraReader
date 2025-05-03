using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ultraReader.Models.DTOs;
using ultraReader.Models.Entities;

namespace ultraReader.Services
{
    public interface ICommentService
    {
        // Yorum alma metodları
        Task<List<Comment>> GetApprovedCommentsByWebtoonAsync(string webtoonId);
        Task<List<Comment>> GetCommentsForWebtoonAsync(string webtoonId, bool includeUnapproved = false);
        Task<List<Comment>> GetCommentsForChapterAsync(string webtoonId, string chapterId, bool includeUnapproved = false);
        Task<List<Comment>> GetLatestCommentsAsync(int count);
        Task<List<Comment>> GetAllPendingCommentsAsync();
        Task<Comment> GetCommentByIdAsync(string id);
        
        // Yorum yönetim metodları
        Task<Comment> AddCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(string id);
        Task<bool> ApproveCommentAsync(string id);
        Task<bool> RejectCommentAsync(string id);
        
        // İstatistik metodları
        Task<int> GetTotalCommentCountAsync();
        Task<int> GetPendingCommentCountAsync();
        Task<Dictionary<string, int>> GetCommentsPerDayAsync(int days = 7);
        
        // Derecelendirme metodları
        Task<double> GetWebtoonRatingAsync(string webtoonId);
        Task<Rating> GetUserRatingAsync(string webtoonId, string userId);
        Task<bool> RateWebtoonAsync(string webtoonId, string userId, int value);
        Task<bool> UpdateRatingAsync(string ratingId, int value);
        Task<bool> DeleteRatingAsync(string ratingId);
    }
} 