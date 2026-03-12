namespace Storefront.Modules.Content.Core.Domain.Entities;

public class FeaturedBrand
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? LogoUrl { get; set; }
    public required string Link { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
