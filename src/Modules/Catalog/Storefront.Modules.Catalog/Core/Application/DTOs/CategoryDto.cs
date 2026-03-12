namespace Storefront.Modules.Catalog.Core.Application.DTOs;

public sealed record CategoryDto(
    string Id,
    string Name,
    string? Description,
    string? Slug,
    string? ImageUrl,
    string? ParentId,
    int DisplayOrder,
    bool IsActive,
    bool ShowInNavbar,
    int ProductCount
);

