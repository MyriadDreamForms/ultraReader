using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ultraReader.Data;
using ultraReader.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ultraReader.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;
        
        public CommentService(ApplicationDbContext context, IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }
        
        public async Task<List<Comment>> GetApprovedCommentsByWebtoonAsync(string webtoonId)
        {
            return await _context.Comments
                .Where(c => c.WebtoonId == webtoonId && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Comment>> GetCommentsForWebtoonAsync(string webtoonId, bool includeUnapproved = false)
        {
            var query = _context.Comments
                .Where(c => c.WebtoonId == webtoonId && c.ChapterId == null);
                
            if (!includeUnapproved)
            {
                query = query.Where(c => c.IsApproved);
            }
            
            return await query.OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Comment>> GetCommentsForChapterAsync(string webtoonId, string chapterId, bool includeUnapproved = false)
        {
            var query = _context.Comments
                .Where(c => c.WebtoonId == webtoonId && c.ChapterId == chapterId);
                
            if (!includeUnapproved)
            {
                query = query.Where(c => c.IsApproved);
            }
            
            return await query.OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            comment.Id = Guid.NewGuid().ToString();
            comment.CreatedAt = DateTime.UtcNow;
            
            // Eğer rating belirtilmemiş ise varsayılan değeri kullan
            if (comment.Rating <= 0 || comment.Rating > 5)
            {
                comment.Rating = 5; // Varsayılan değer
            }
            
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            
            return comment;
        }
        
        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            
            _context.Comments.Update(comment);
            var result = await _context.SaveChangesAsync();
            
            return result > 0;
        }
        
        public async Task<bool> DeleteCommentAsync(string commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return false;
            }
            
            _context.Comments.Remove(comment);
            var result = await _context.SaveChangesAsync();
            
            return result > 0;
        }
        
        public async Task<double> GetWebtoonRatingAsync(string webtoonId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.WebtoonId == webtoonId)
                .ToListAsync();
                
            if (ratings == null || !ratings.Any())
            {
                return 0;
            }
            
            return Math.Round(ratings.Average(r => r.Value), 1);
        }
        
        public async Task<Rating> GetUserRatingAsync(string webtoonId, string userId)
        {
            return await _context.Ratings
                .FirstOrDefaultAsync(r => r.WebtoonId == webtoonId && r.UserId == userId);
        }
        
        public async Task<bool> RateWebtoonAsync(string webtoonId, string userId, int value)
        {
            // Mevcut derecelendirmeyi kontrol et
            var existingRating = await GetUserRatingAsync(webtoonId, userId);
            
            if (existingRating != null)
            {
                existingRating.Value = value;
                existingRating.UpdatedAt = DateTime.UtcNow;
                
                _context.Ratings.Update(existingRating);
            }
            else
            {
                var rating = new Rating
                {
                    Id = Guid.NewGuid().ToString(),
                    WebtoonId = webtoonId,
                    UserId = userId,
                    Value = value,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _context.Ratings.AddAsync(rating);
            }
            
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<bool> UpdateRatingAsync(string ratingId, int value)
        {
            var rating = await _context.Ratings.FindAsync(ratingId);
            if (rating == null)
            {
                return false;
            }
            
            rating.Value = value;
            rating.UpdatedAt = DateTime.UtcNow;
            
            _context.Ratings.Update(rating);
            var result = await _context.SaveChangesAsync();
            
            return result > 0;
        }
        
        public async Task<bool> DeleteRatingAsync(string ratingId)
        {
            var rating = await _context.Ratings.FindAsync(ratingId);
            if (rating == null)
            {
                return false;
            }
            
            _context.Ratings.Remove(rating);
            var result = await _context.SaveChangesAsync();
            
            return result > 0;
        }

        public async Task<int> GetTotalCommentCountAsync()
        {
            return await _context.Comments.CountAsync();
        }

        public async Task<int> GetPendingCommentCountAsync()
        {
            return await _context.Comments
                .Where(c => !c.IsApproved)
                .CountAsync();
        }

        public async Task<List<Comment>> GetLatestCommentsAsync(int count)
        {
            return await _context.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetCommentsPerDayAsync(int days = 7)
        {
            // Önbellekten kontrol et
            string cacheKey = $"CommentsPerDay_{days}";
            string cachedData = await _distributedCache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(cachedData);
            }
            
            // Belirtilen gün sayısı kadar öncesine git
            var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
            var endDate = DateTime.UtcNow.Date.AddDays(1); // Bugünü dahil etmek için
            
            // Son N gün için yorumları getir
            var comments = await _context.Comments
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate)
                .ToListAsync();
            
            // Her gün için yorum sayısını hesapla
            var commentsPerDay = new Dictionary<string, int>();
            
            // Tüm günleri ekleyerek başla (veri yoksa bile 0 gösterilecek)
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dateStr = date.ToString("yyyy-MM-dd");
                commentsPerDay[dateStr] = 0;
            }
            
            // Yorumları günlere göre grupla
            foreach (var comment in comments)
            {
                var dateStr = comment.CreatedAt.ToString("yyyy-MM-dd");
                if (commentsPerDay.ContainsKey(dateStr))
                {
                    commentsPerDay[dateStr]++;
                }
            }
            
            // Önbelleğe al (30 dakika)
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            
            await _distributedCache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(commentsPerDay),
                cacheOptions);
            
            return commentsPerDay;
        }
        
        public async Task<bool> ApproveCommentAsync(string id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return false;
            }
            
            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.UtcNow;
            
            _context.Comments.Update(comment);
            var result = await _context.SaveChangesAsync();
            
            return result > 0;
        }
        
        public async Task<bool> RejectCommentAsync(string id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return false;
            }
            
            comment.IsApproved = false;
            comment.UpdatedAt = DateTime.UtcNow;
            
            _context.Comments.Update(comment);
            var result = await _context.SaveChangesAsync();
            
            return result > 0;
        }
        
        public async Task<List<Comment>> GetAllPendingCommentsAsync()
        {
            return await _context.Comments
                .Where(c => !c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<Comment> GetCommentByIdAsync(string id)
        {
            return await _context.Comments.FindAsync(id);
        }
    }
} 