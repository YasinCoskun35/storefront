using Storefront.Modules.Content.Core.Domain.ValueObjects;

namespace Storefront.Modules.Content.Core.Domain.Entities;

public class StaticPage
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Body { get; set; }
    public bool IsPublished { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Owned entity for SEO
    public SeoMetadata SeoMetadata { get; set; } = SeoMetadata.Empty();
}

