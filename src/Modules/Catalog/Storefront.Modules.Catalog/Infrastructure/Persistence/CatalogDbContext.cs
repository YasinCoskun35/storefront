using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Core.Domain.Enums;

namespace Storefront.Modules.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductBundleItem> ProductBundleItems => Set<ProductBundleItem>();
    public DbSet<VariantGroup> VariantGroups => Set<VariantGroup>();
    public DbSet<VariantOption> VariantOptions => Set<VariantOption>();
    public DbSet<ProductVariantGroup> ProductVariantGroups => Set<ProductVariantGroup>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set default schema for the Catalog module
        builder.HasDefaultSchema("catalog");

        // Enable PostgreSQL extensions
        builder.HasPostgresExtension("pg_trgm");

        ConfigureProduct(builder);
        ConfigureCategory(builder);
        ConfigureBrand(builder);
        ConfigureProductImage(builder);
        ConfigureProductBundleItem(builder);
        ConfigureVariantGroup(builder);
        ConfigureVariantOption(builder);
        ConfigureProductVariantGroup(builder);
    }

    private static void ConfigureProduct(ModelBuilder builder)
    {
        builder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasMaxLength(450);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(500);
            entity.Property(p => p.SKU).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(5000);
            entity.Property(p => p.ShortDescription).HasMaxLength(500);
            entity.Property(p => p.Slug).HasMaxLength(500);

            // Pricing fields (now nullable for B2B)
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.CompareAtPrice).HasPrecision(18, 2);
            entity.Property(p => p.Cost).HasPrecision(18, 2);
            entity.Property(p => p.BundlePrice).HasPrecision(18, 2);

            entity.Property(p => p.Weight).HasPrecision(18, 2);
            entity.Property(p => p.Length).HasPrecision(18, 2);
            entity.Property(p => p.Width).HasPrecision(18, 2);
            entity.Property(p => p.Height).HasPrecision(18, 2);

            entity.Property(p => p.DimensionUnit).HasMaxLength(10);
            entity.Property(p => p.WeightUnit).HasMaxLength(10);

            entity.Property(p => p.MetaTitle).HasMaxLength(200);
            entity.Property(p => p.MetaDescription).HasMaxLength(500);

            entity.Property(p => p.StockStatus)
                .HasConversion<string>()
                .HasMaxLength(50);
                
            entity.Property(p => p.Quantity)
                .HasColumnName("StockQuantity");
                
            entity.Property(p => p.LowStockThreshold)
                .IsRequired(false);
                
            entity.Property(p => p.ProductType)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Relationships
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(p => p.BundleItems)
                .WithOne(bi => bi.BundleProduct)
                .HasForeignKey(bi => bi.BundleProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(p => p.UsedInBundles)
                .WithOne(bi => bi.ComponentProduct)
                .HasForeignKey(bi => bi.ComponentProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.HasIndex(p => p.Slug);
            entity.HasIndex(p => p.IsActive);
            entity.HasIndex(p => p.CategoryId);
            entity.HasIndex(p => p.BrandId);
            entity.HasIndex(p => p.StockStatus);
            entity.HasIndex(p => p.ProductType);

            // Trigram indexes for fuzzy search (GIN indexes)
            entity.HasIndex(p => p.Name)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasIndex(p => p.Description)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            entity.HasIndex(p => p.SKU)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        });
    }

    private static void ConfigureCategory(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasMaxLength(450);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(2000);
            entity.Property(c => c.Slug).HasMaxLength(200);
            entity.Property(c => c.ImageUrl).HasMaxLength(500);
            entity.Property(c => c.ShowInNavbar).HasDefaultValue(false);

            // Self-referencing relationship
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.ParentId);
            entity.HasIndex(c => c.IsActive);
            entity.HasIndex(c => c.DisplayOrder);
        });
    }

    private static void ConfigureBrand(ModelBuilder builder)
    {
        builder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).HasMaxLength(450);

            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Description).HasMaxLength(2000);
            entity.Property(b => b.LogoUrl).HasMaxLength(500);
            entity.Property(b => b.Website).HasMaxLength(500);

            // Indexes
            entity.HasIndex(b => b.Name).IsUnique();
            entity.HasIndex(b => b.IsActive);
        });
    }

    private static void ConfigureProductImage(ModelBuilder builder)
    {
        builder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).HasMaxLength(450);

            entity.Property(i => i.Url).IsRequired().HasMaxLength(1000);
            entity.Property(i => i.AltText).HasMaxLength(200);
            
            entity.Property(i => i.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Indexes
            entity.HasIndex(i => i.ProductId);
            entity.HasIndex(i => new { i.ProductId, i.IsPrimary });
            entity.HasIndex(i => i.Type);
        });
    }
    
    private static void ConfigureProductBundleItem(ModelBuilder builder)
    {
        builder.Entity<ProductBundleItem>(entity =>
        {
            entity.ToTable("ProductBundleItems");
            entity.HasKey(bi => bi.Id);
            entity.Property(bi => bi.Id).HasMaxLength(450);

            entity.Property(bi => bi.PriceOverride).HasPrecision(18, 2);

            entity.HasIndex(bi => bi.BundleProductId);
            entity.HasIndex(bi => bi.ComponentProductId);
            entity.HasIndex(bi => new { bi.BundleProductId, bi.DisplayOrder });
        });
    }

    private static void ConfigureVariantGroup(ModelBuilder builder)
    {
        builder.Entity<VariantGroup>(entity =>
        {
            entity.ToTable("VariantGroups");
            entity.HasKey(vg => vg.Id);
            entity.Property(vg => vg.Id).HasMaxLength(450);

            entity.Property(vg => vg.Name).IsRequired().HasMaxLength(200);
            entity.Property(vg => vg.Description).IsRequired().HasMaxLength(2000).HasDefaultValue(string.Empty);
            entity.Property(vg => vg.DisplayType).IsRequired().HasMaxLength(50).HasDefaultValue("Swatch");

            entity.HasIndex(vg => vg.IsActive);
            entity.HasIndex(vg => vg.DisplayType);
            entity.HasIndex(vg => vg.DisplayOrder);
        });
    }

    private static void ConfigureVariantOption(ModelBuilder builder)
    {
        builder.Entity<VariantOption>(entity =>
        {
            entity.ToTable("VariantOptions");
            entity.HasKey(vo => vo.Id);
            entity.Property(vo => vo.Id).HasMaxLength(450);

            entity.Property(vo => vo.VariantGroupId).IsRequired().HasMaxLength(450);
            entity.Property(vo => vo.Name).IsRequired().HasMaxLength(200);
            entity.Property(vo => vo.Code).IsRequired().HasMaxLength(100);
            entity.Property(vo => vo.HexColor).HasMaxLength(10);
            entity.Property(vo => vo.ImageUrl).HasMaxLength(1000);
            entity.Property(vo => vo.PriceAdjustment).HasColumnType("decimal(18,2)");

            entity.HasOne(vo => vo.VariantGroup)
                .WithMany(vg => vg.Options)
                .HasForeignKey(vo => vo.VariantGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(vo => vo.VariantGroupId);
            entity.HasIndex(vo => new { vo.VariantGroupId, vo.Code }).IsUnique();
            entity.HasIndex(vo => vo.IsAvailable);
        });
    }

    private static void ConfigureProductVariantGroup(ModelBuilder builder)
    {
        builder.Entity<ProductVariantGroup>(entity =>
        {
            entity.ToTable("ProductVariantGroups");
            entity.HasKey(pvg => pvg.Id);
            entity.Property(pvg => pvg.Id).HasMaxLength(450);

            entity.Property(pvg => pvg.ProductId).IsRequired().HasMaxLength(450);
            entity.Property(pvg => pvg.VariantGroupId).IsRequired().HasMaxLength(450);

            entity.HasOne(pvg => pvg.VariantGroup)
                .WithMany(vg => vg.ProductVariantGroups)
                .HasForeignKey(pvg => pvg.VariantGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pvg => pvg.ProductId);
            entity.HasIndex(pvg => pvg.VariantGroupId);
            entity.HasIndex(pvg => new { pvg.ProductId, pvg.VariantGroupId }).IsUnique();
        });
    }
}

