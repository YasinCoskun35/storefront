using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Domain.Entities;

namespace Storefront.Modules.Content.Infrastructure.Persistence;

public class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<StaticPage> StaticPages => Set<StaticPage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set default schema for the Content module
        builder.HasDefaultSchema("content");

        ConfigureBlogPost(builder);
        ConfigureStaticPage(builder);
    }

    private static void ConfigureBlogPost(ModelBuilder builder)
    {
        builder.Entity<BlogPost>(entity =>
        {
            entity.ToTable("BlogPosts");
            entity.HasKey(bp => bp.Id);
            entity.Property(bp => bp.Id).HasMaxLength(450);

            entity.Property(bp => bp.Title).IsRequired().HasMaxLength(500);
            entity.Property(bp => bp.Slug).IsRequired().HasMaxLength(500);
            entity.Property(bp => bp.Summary).HasMaxLength(1000);
            entity.Property(bp => bp.Body).IsRequired();
            entity.Property(bp => bp.FeaturedImage).HasMaxLength(1000);
            entity.Property(bp => bp.Author).HasMaxLength(200);
            entity.Property(bp => bp.Tags).HasMaxLength(500);
            entity.Property(bp => bp.Category).HasMaxLength(200);

            // Configure SeoMetadata as owned entity
            entity.OwnsOne(bp => bp.SeoMetadata, seo =>
            {
                seo.Property(s => s.MetaTitle).HasMaxLength(200).HasColumnName("SeoMetaTitle");
                seo.Property(s => s.MetaDescription).HasMaxLength(500).HasColumnName("SeoMetaDescription");
                seo.Property(s => s.Keywords).HasMaxLength(500).HasColumnName("SeoKeywords");
                seo.Property(s => s.OgImage).HasMaxLength(1000).HasColumnName("SeoOgImage");
                seo.Property(s => s.OgType).HasMaxLength(50).HasColumnName("SeoOgType");
                seo.Property(s => s.CanonicalUrl).HasMaxLength(1000).HasColumnName("SeoCanonicalUrl");
            });

            // Indexes
            entity.HasIndex(bp => bp.Slug).IsUnique();
            entity.HasIndex(bp => bp.IsPublished);
            entity.HasIndex(bp => bp.PublishedAt);
            entity.HasIndex(bp => bp.Category);
        });
    }

    private static void ConfigureStaticPage(ModelBuilder builder)
    {
        builder.Entity<StaticPage>(entity =>
        {
            entity.ToTable("StaticPages");
            entity.HasKey(sp => sp.Id);
            entity.Property(sp => sp.Id).HasMaxLength(450);

            entity.Property(sp => sp.Title).IsRequired().HasMaxLength(500);
            entity.Property(sp => sp.Slug).IsRequired().HasMaxLength(500);
            entity.Property(sp => sp.Body).IsRequired();

            // Configure SeoMetadata as owned entity
            entity.OwnsOne(sp => sp.SeoMetadata, seo =>
            {
                seo.Property(s => s.MetaTitle).HasMaxLength(200).HasColumnName("SeoMetaTitle");
                seo.Property(s => s.MetaDescription).HasMaxLength(500).HasColumnName("SeoMetaDescription");
                seo.Property(s => s.Keywords).HasMaxLength(500).HasColumnName("SeoKeywords");
                seo.Property(s => s.OgImage).HasMaxLength(1000).HasColumnName("SeoOgImage");
                seo.Property(s => s.OgType).HasMaxLength(50).HasColumnName("SeoOgType");
                seo.Property(s => s.CanonicalUrl).HasMaxLength(1000).HasColumnName("SeoCanonicalUrl");
            });

            // Indexes
            entity.HasIndex(sp => sp.Slug).IsUnique();
            entity.HasIndex(sp => sp.IsPublished);
            entity.HasIndex(sp => sp.DisplayOrder);
        });
    }
}

