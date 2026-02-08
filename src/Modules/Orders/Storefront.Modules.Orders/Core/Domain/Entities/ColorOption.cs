namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class ColorOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Chart Reference
    public string ColorChartId { get; set; } = string.Empty;
    public virtual ColorChart ColorChart { get; set; } = null!;
    
    // Color Information
    public string Name { get; set; } = string.Empty; // e.g., "Royal Blue"
    public string Code { get; set; } = string.Empty; // e.g., "RB-001"
    public string? HexColor { get; set; } // e.g., "#1E3A8A" (for preview)
    public string? ImageUrl { get; set; } // Actual fabric/material photo
    
    // Availability
    public bool IsAvailable { get; set; } = true;
    public int StockLevel { get; set; } = 0; // 0 = unlimited
    
    // Pricing (optional - may affect product price)
    public decimal? PriceAdjustment { get; set; } // +/- price modification
    
    // Display
    public int DisplayOrder { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
