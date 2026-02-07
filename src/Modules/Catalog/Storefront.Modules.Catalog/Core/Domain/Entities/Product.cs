using Storefront.Modules.Catalog.Core.Domain.Enums;

namespace Storefront.Modules.Catalog.Core.Domain.Entities;

public class Product
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string SKU { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    
    // Product Type (Simple, Bundle, Configurable)
    public ProductType ProductType { get; set; } = ProductType.Simple;
    
    // Pricing (nullable to support B2B quote-based pricing)
    public decimal? Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? Cost { get; set; }
    
    // Bundle-specific pricing
    /// <summary>
    /// Override price for bundle products. If null, price is calculated from components.
    /// Allows offering bundles at discount vs buying components separately.
    /// </summary>
    public decimal? BundlePrice { get; set; }
    
    /// <summary>
    /// Can this product be sold separately? 
    /// Useful for bundle components that are also standalone products.
    /// </summary>
    public bool CanBeSoldSeparately { get; set; } = true;
    
    // Stock Management
    public StockStatus StockStatus { get; set; } = StockStatus.InStock;
    public int Quantity { get; set; }
    public int? LowStockThreshold { get; set; }
    
    // Dimensions
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; }
    public string? WeightUnit { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Slug { get; set; }

    // Relationships
    public required string CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    public string? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public ICollection<ProductImage> Images { get; set; } = [];
    
    // Bundle relationships
    /// <summary>
    /// If this is a Bundle product, these are the components it contains
    /// </summary>
    public ICollection<ProductBundleItem> BundleItems { get; set; } = [];
    
    /// <summary>
    /// Bundles that this product is used in (if it's a component)
    /// </summary>
    public ICollection<ProductBundleItem> UsedInBundles { get; set; } = [];

    // Flags
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

