namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class ColorChart
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Chart Information
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // e.g., "FABRIC-2024-SPRING"
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Fabric", "Leather", "Wood", "Metal"
    
    // Images
    public string? MainImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty; // Admin user ID
    
    // Navigation
    public virtual ICollection<ColorOption> ColorOptions { get; set; } = new List<ColorOption>();
    public virtual ICollection<ProductColorChart> ProductColorCharts { get; set; } = new List<ProductColorChart>();
}
