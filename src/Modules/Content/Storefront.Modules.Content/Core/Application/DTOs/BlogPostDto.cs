namespace Storefront.Modules.Content.Core.Application.DTOs;

public sealed record BlogPostDto(
    string Id,
    string Title,
    string Slug,
    string? Summary,
    string Body,
    string? FeaturedImage,
    string? Author,
    bool IsPublished,
    DateTime? PublishedAt,
    int ViewCount,
    string? Tags,
    string? Category,
    DateTime CreatedAt
);

public sealed record BlogPostSummaryDto(
    string Id,
    string Title,
    string Slug,
    string? Summary,
    string? FeaturedImage,
    string? Author,
    DateTime? PublishedAt,
    int ViewCount,
    string? Category
);

public sealed record StaticPageDto(
    string Id,
    string Title,
    string Slug,
    string Body,
    bool IsPublished
);

public sealed record SitemapEntryDto(
    string Url,
    DateTime LastModified,
    string ChangeFrequency,
    double Priority
);

