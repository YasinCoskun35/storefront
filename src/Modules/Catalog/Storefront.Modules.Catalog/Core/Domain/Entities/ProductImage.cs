using Storefront.Modules.Catalog.Core.Domain.Enums;

namespace Storefront.Modules.Catalog.Core.Domain.Entities;

public class ProductImage
{
    public required string Id { get; set; }
    public required string ProductId { get; set; }
    public required string Url { get; set; }
    public ImageType Type { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? AltText { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}

