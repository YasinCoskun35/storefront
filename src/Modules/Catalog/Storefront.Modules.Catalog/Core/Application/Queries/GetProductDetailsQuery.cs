using MediatR;
using Storefront.Modules.Catalog.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed record GetProductDetailsQuery(string ProductId) : IRequest<Result<ProductDetailDto>>;

public sealed record ProductDetailDto(
    string Id,
    string Name,
    string SKU,
    string? Description,
    string? ShortDescription,
    decimal? Price,  // Nullable for B2B
    decimal? CompareAtPrice,
    string StockStatus,
    int Quantity,
    decimal? Weight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    string? DimensionUnit,
    string? WeightUnit,
    string CategoryId,
    string CategoryName,
    string? BrandId,
    string? BrandName,
    bool IsActive,
    bool IsFeatured,
    IReadOnlyList<ProductImageDto> Images,
    DateTime CreatedAt
);

public sealed record ProductImageDto(
    string Id,
    string Url,
    string Type,
    bool IsPrimary,
    int DisplayOrder
);

