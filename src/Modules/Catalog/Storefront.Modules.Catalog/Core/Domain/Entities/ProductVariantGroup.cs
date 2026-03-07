namespace Storefront.Modules.Catalog.Core.Domain.Entities;

public class ProductVariantGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ProductId { get; set; } = string.Empty;

    public string VariantGroupId { get; set; } = string.Empty;
    public virtual VariantGroup VariantGroup { get; set; } = null!;

    public bool IsRequired { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
