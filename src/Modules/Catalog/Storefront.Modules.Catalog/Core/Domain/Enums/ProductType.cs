namespace Storefront.Modules.Catalog.Core.Domain.Enums;

/// <summary>
/// Defines the type of product
/// </summary>
public enum ProductType
{
    /// <summary>
    /// A single standalone product (e.g., "3-Seater Sofa")
    /// </summary>
    Simple = 0,
    
    /// <summary>
    /// A bundle/set containing multiple products (e.g., "Living Room Set")
    /// </summary>
    Bundle = 1,
    
    /// <summary>
    /// A configurable product with variants (e.g., different fabrics/colors)
    /// Reserved for future use
    /// </summary>
    Configurable = 2
}
