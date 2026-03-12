namespace Storefront.Modules.Catalog.Core.Domain.Entities;

public class VariantOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string VariantGroupId { get; set; } = string.Empty;
    public virtual VariantGroup VariantGroup { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    // For Swatch display type
    public string? HexColor { get; set; }
    public string? ImageUrl { get; set; }

    public decimal? PriceAdjustment { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
