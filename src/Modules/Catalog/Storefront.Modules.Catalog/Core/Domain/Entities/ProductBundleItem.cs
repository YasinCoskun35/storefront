using Storefront.Modules.Catalog.Core.Domain.Enums;

namespace Storefront.Modules.Catalog.Core.Domain.Entities;

/// <summary>
/// Represents a component/item within a bundle product.
/// Links a bundle product to its component products with quantity and pricing details.
/// </summary>
public class ProductBundleItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // The bundle/set product (parent)
    public string BundleProductId { get; set; } = null!;
    public virtual Product BundleProduct { get; set; } = null!;
    
    // The component product (child - what's included in the bundle)
    public string ComponentProductId { get; set; } = null!;
    public virtual Product ComponentProduct { get; set; } = null!;
    
    /// <summary>
    /// How many of this component are included in the bundle?
    /// Example: Living Room Set might include 2x Bergere chairs
    /// </summary>
    public int Quantity { get; set; } = 1;
    
    /// <summary>
    /// Optional: Override the component's price when sold as part of this bundle.
    /// If null, uses the component's regular price.
    /// Useful for bundle-specific discounts.
    /// </summary>
    public decimal? PriceOverride { get; set; }
    
    /// <summary>
    /// Can the customer customize/remove this component?
    /// Reserved for future "Build Your Set" functionality.
    /// </summary>
    public bool IsOptional { get; set; } = false;
    
    /// <summary>
    /// Display order in the bundle (for UI presentation)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
