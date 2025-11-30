using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UltraReader.Entities;

namespace UltraReader.Data;

/// <summary>
/// Entity Framework Core database context for UltraReader.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Series> Series => Set<Series>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<ReadingProgress> ReadingProgress => Set<ReadingProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Series configuration
        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsFavorite);
            entity.HasIndex(e => e.UpdatedAt);

            // Store Tags as JSON
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );
        });

        // Chapter configuration
        modelBuilder.Entity<Chapter>(entity =>
        {
            // Unique chapter number per series
            entity.HasIndex(e => new { e.SeriesId, e.Number }).IsUnique();
            entity.HasIndex(e => e.SeriesId);

            // Cascade delete: deleting a series deletes all its chapters
            entity.HasOne(e => e.Series)
                .WithMany(s => s.Chapters)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Page configuration
        modelBuilder.Entity<Page>(entity =>
        {
            // Index for ordered page retrieval
            entity.HasIndex(e => new { e.ChapterId, e.PageNumber });
            entity.HasIndex(e => e.ChapterId);

            // Cascade delete: deleting a chapter deletes all its pages
            entity.HasOne(e => e.Chapter)
                .WithMany(c => c.Pages)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReadingProgress configuration
        modelBuilder.Entity<ReadingProgress>(entity =>
        {
            // One progress per series
            entity.HasIndex(e => e.SeriesId).IsUnique();
            entity.HasIndex(e => e.LastReadAt);

            // 1:1 relationship with Series
            entity.HasOne(e => e.Series)
                .WithOne(s => s.ReadingProgress)
                .HasForeignKey<ReadingProgress>(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reference to last read chapter (restrict delete to prevent orphaned references)
            entity.HasOne(e => e.LastReadChapter)
                .WithMany()
                .HasForeignKey(e => e.LastReadChapterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Override SaveChanges to automatically update UpdatedAt timestamps.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update UpdatedAt timestamps.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Series series)
                series.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Chapter chapter)
                chapter.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Page page)
                page.UpdatedAt = DateTime.UtcNow;
        }
    }
}
