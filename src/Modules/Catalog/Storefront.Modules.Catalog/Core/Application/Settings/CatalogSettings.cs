namespace Storefront.Modules.Catalog.Core.Application.Settings;

/// <summary>
/// Configuration settings for the Catalog module
/// </summary>
public class CatalogSettings
{
    public const string SectionName = "CatalogSettings";
    
    /// <summary>
    /// Global toggle for pricing feature.
    /// When false, prices are hidden from customers (B2B quote-based model).
    /// When true, prices are displayed (e-commerce model).
    /// </summary>
    public bool PricingEnabled { get; set; } = false;
    
    /// <summary>
    /// Require price when creating/updating products.
    /// When false, products can be created without prices.
    /// Admin can still enter prices for internal tracking.
    /// </summary>
    public bool RequirePriceForProducts { get; set; } = false;
    
    /// <summary>
    /// Text to display when price is hidden (e.g., "Contact for Quote", "Request Price").
    /// Shown to customers in mobile app when PricingEnabled = false.
    /// </summary>
    public string ShowPriceLabel { get; set; } = "Contact for Quote";
    
    /// <summary>
    /// Show "Request Quote" button when prices are hidden.
    /// Enables quote request workflow in mobile app.
    /// </summary>
    public bool AllowPriceInquiry { get; set; } = true;
}
