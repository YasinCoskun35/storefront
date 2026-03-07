namespace Storefront.Modules.Content.Core.Domain.Entities;

public class HomeCategorySlide
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string ImageUrl { get; set; }
    public required string Link { get; set; }
    public int ProductCount { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
