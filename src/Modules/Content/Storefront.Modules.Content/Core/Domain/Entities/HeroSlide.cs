namespace Storefront.Modules.Content.Core.Domain.Entities;

public class HeroSlide
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public required string ImageUrl { get; set; }
    public required string Link { get; set; }
    public required string LinkText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
