using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ultraReader.Models.Entities;

namespace ultraReader.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WebtoonInfo> Webtoons { get; set; }
        public DbSet<FavoriteWebtoon> FavoriteWebtoons { get; set; }
        public DbSet<ReadingHistory> ReadingHistories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<ReadingListItem> ReadingList { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<PageView> PageViews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Webtoon tablosu için konfigürasyon
            builder.Entity<WebtoonInfo>()
                .HasKey(w => w.Id);
                
            // Favori webtoon tablosu için konfigürasyon
            builder.Entity<FavoriteWebtoon>()
                .HasKey(f => f.Id);
                
            builder.Entity<FavoriteWebtoon>()
                .HasOne(f => f.Webtoon)
                .WithMany()
                .HasForeignKey(f => f.WebtoonId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Okuma geçmişi tablosu için konfigürasyon
            builder.Entity<ReadingHistory>()
                .HasKey(r => r.Id);
                
            builder.Entity<ReadingHistory>()
                .HasOne(r => r.Webtoon)
                .WithMany()
                .HasForeignKey(r => r.WebtoonId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Kullanıcı tercihleri tablosu için konfigürasyon
            builder.Entity<UserPreferences>()
                .HasKey(p => p.Id);
                
            // Kullanıcı favorileri tablosu için konfigürasyon
            builder.Entity<UserFavorite>()
                .HasKey(f => f.Id);
                
            // Okuma listesi tablosu için konfigürasyon
            builder.Entity<ReadingListItem>()
                .HasKey(r => r.Id);
                
            // Kullanıcı aktivite tablosu için konfigürasyon
            builder.Entity<UserActivity>()
                .HasKey(a => a.Id);
                
            // Sayfa görüntüleme tablosu için konfigürasyon
            builder.Entity<PageView>()
                .HasKey(p => p.Id);
                
            builder.Entity<UserActivity>()
                .HasIndex(a => a.Timestamp);
                
            builder.Entity<PageView>()
                .HasIndex(p => p.ViewedAt);
        }
    }
} 