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
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<HeroSlide> HeroSlides => Set<HeroSlide>();
    public DbSet<HomeCategorySlide> HomeCategorySlides => Set<HomeCategorySlide>();
    public DbSet<FeaturedBrand> FeaturedBrands => Set<FeaturedBrand>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set default schema for the Content module
        builder.HasDefaultSchema("content");

        ConfigureBlogPost(builder);
        ConfigureStaticPage(builder);
        ConfigureAppSetting(builder);
        ConfigureHeroSlide(builder);
        ConfigureHomeCategorySlide(builder);
        ConfigureFeaturedBrand(builder);
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

    private static void ConfigureAppSetting(ModelBuilder builder)
    {
        builder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("AppSettings");
            entity.HasKey(s => s.Key);
            entity.Property(s => s.Key).HasMaxLength(100);

            entity.Property(s => s.Value).IsRequired().HasMaxLength(2000);
            entity.Property(s => s.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.Property(s => s.Category).IsRequired().HasMaxLength(50);
            entity.Property(s => s.DataType).IsRequired().HasMaxLength(20);
            entity.Property(s => s.UpdatedBy).HasMaxLength(450);

            // Index for faster category lookups
            entity.HasIndex(s => s.Category);
        });
    }

    private static void ConfigureHeroSlide(ModelBuilder builder)
    {
        builder.Entity<HeroSlide>(entity =>
        {
            entity.ToTable("HeroSlides");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasMaxLength(450);
            entity.Property(s => s.Title).IsRequired().HasMaxLength(500);
            entity.Property(s => s.Subtitle).HasMaxLength(500);
            entity.Property(s => s.ImageUrl).IsRequired().HasMaxLength(1000);
            entity.Property(s => s.Link).IsRequired().HasMaxLength(500);
            entity.Property(s => s.LinkText).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => s.DisplayOrder);
        });
    }

    private static void ConfigureHomeCategorySlide(ModelBuilder builder)
    {
        builder.Entity<HomeCategorySlide>(entity =>
        {
            entity.ToTable("HomeCategorySlides");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasMaxLength(450);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Slug).IsRequired().HasMaxLength(200);
            entity.Property(s => s.ImageUrl).IsRequired().HasMaxLength(1000);
            entity.Property(s => s.Link).IsRequired().HasMaxLength(500);
            entity.HasIndex(s => s.DisplayOrder);
        });
    }

    private static void ConfigureFeaturedBrand(ModelBuilder builder)
    {
        builder.Entity<FeaturedBrand>(entity =>
        {
            entity.ToTable("FeaturedBrands");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).HasMaxLength(450);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.Property(b => b.LogoUrl).HasMaxLength(1000);
            entity.Property(b => b.Link).IsRequired().HasMaxLength(500);
            entity.HasIndex(b => b.DisplayOrder);
        });
    }
}

