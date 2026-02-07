using Storefront.Modules.Content.Core.Domain.ValueObjects;

namespace Storefront.Modules.Content.Core.Domain.Entities;

public class BlogPost
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public string? Summary { get; set; }
    public required string Body { get; set; }
    public string? FeaturedImage { get; set; }
    public string? Author { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Tags stored as comma-separated values (simple approach for showcase)
    public string? Tags { get; set; }
    public string? Category { get; set; }

    // Owned entity for SEO
    public SeoMetadata SeoMetadata { get; set; } = SeoMetadata.Empty();
}

