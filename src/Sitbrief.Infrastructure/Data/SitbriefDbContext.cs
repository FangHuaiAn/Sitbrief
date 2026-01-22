using Microsoft.EntityFrameworkCore;
using Sitbrief.Core.Entities;

namespace Sitbrief.Infrastructure.Data;

public class SitbriefDbContext : DbContext
{
    public SitbriefDbContext(DbContextOptions<SitbriefDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<ArticleTopic> ArticleTopics => Set<ArticleTopic>();
    public DbSet<AIAnalysis> AIAnalyses => Set<AIAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Article
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.SourceUrl).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.SourceName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).HasMaxLength(50000);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
        });

        // Configure Topic
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Significance).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.LastUpdatedDate).HasDefaultValueSql("datetime('now')");
        });

        // Configure ArticleTopic (Many-to-Many)
        modelBuilder.Entity<ArticleTopic>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.TopicId });

            entity.HasOne(e => e.Article)
                .WithMany(a => a.ArticleTopics)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Topic)
                .WithMany(t => t.ArticleTopics)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Confidence).HasDefaultValue(1.0);
            entity.Property(e => e.IsConfirmed).HasDefaultValue(false);
            entity.Property(e => e.AddedDate).HasDefaultValueSql("datetime('now')");
        });

        // Configure AIAnalysis
        modelBuilder.Entity<AIAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Article)
                .WithOne(a => a.AIAnalysis)
                .HasForeignKey<AIAnalysis>(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SuggestedTopicsJson).IsRequired();
            entity.Property(e => e.KeyEntitiesJson).IsRequired();
            entity.Property(e => e.GeopoliticalTagsJson).IsRequired();
            entity.Property(e => e.SignificanceScore).HasDefaultValue(5);
            entity.Property(e => e.AnalyzedDate).HasDefaultValueSql("datetime('now')");
        });
    }
}
