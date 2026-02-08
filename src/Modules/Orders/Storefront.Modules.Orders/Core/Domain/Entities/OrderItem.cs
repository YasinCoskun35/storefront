namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Order Reference
    public string OrderId { get; set; } = string.Empty;
    public virtual Order Order { get; set; } = null!;
    
    // Product Information (denormalized for history)
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    
    // Color Selection
    public string? ColorChartId { get; set; }
    public string? ColorChartName { get; set; }
    public string? ColorOptionId { get; set; }
    public string? ColorOptionName { get; set; }
    public string? ColorOptionCode { get; set; }
    public string? ColorOptionImageUrl { get; set; }
    
    // Quantity & Pricing
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; } // Set by admin
    public decimal? Discount { get; set; }
    public decimal? TotalPrice { get; set; } // (UnitPrice * Quantity) - Discount
    
    // Customization
    public string? CustomizationNotes { get; set; } // Partner's special requests
    
    // Display Order
    public int DisplayOrder { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
