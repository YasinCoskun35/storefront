namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class CartItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Cart Reference
    public string CartId { get; set; } = string.Empty;
    public virtual Cart Cart { get; set; } = null!;
    
    // Product Information
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
    
    // Quantity
    public int Quantity { get; set; } = 1;
    
    // Customization
    public string? CustomizationNotes { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
