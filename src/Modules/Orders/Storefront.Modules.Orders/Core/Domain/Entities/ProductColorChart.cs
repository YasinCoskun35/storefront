namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class ProductColorChart
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // References
    public string ProductId { get; set; } = string.Empty; // From Catalog module
    public string ColorChartId { get; set; } = string.Empty;
    public virtual ColorChart ColorChart { get; set; } = null!;
    
    // Settings
    public bool IsRequired { get; set; } = true; // Must partner select a color?
    public bool AllowMultiple { get; set; } = false; // Can select multiple colors?
    
    // Display
    public int DisplayOrder { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
