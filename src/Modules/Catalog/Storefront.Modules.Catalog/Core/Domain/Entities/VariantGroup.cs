namespace Storefront.Modules.Catalog.Core.Domain.Entities;

public class VariantGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Swatch | Dropdown | RadioButtons | ImageGrid
    public string DisplayType { get; set; } = "Swatch";

    public bool IsRequired { get; set; } = true;
    public bool AllowMultiple { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<VariantOption> Options { get; set; } = new List<VariantOption>();
    public virtual ICollection<ProductVariantGroup> ProductVariantGroups { get; set; } = new List<ProductVariantGroup>();
}
