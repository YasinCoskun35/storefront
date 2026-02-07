using MediatR;
using Storefront.Modules.Catalog.Core.Domain.Enums;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record CreateProductCommand(
    string Name,
    string SKU,
    string? Description,
    string? ShortDescription,
    ProductType ProductType,
    decimal? Price,  // Nullable for B2B
    decimal? CompareAtPrice,
    decimal? BundlePrice,  // For bundle products
    bool CanBeSoldSeparately,
    StockStatus StockStatus,
    int Quantity,
    string CategoryId,
    string? BrandId,
    decimal? Weight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    bool IsActive = true,
    bool IsFeatured = false,
    List<BundleItemDto>? BundleItems = null  // Components for bundle products
) : IRequest<Result<string>>;

public sealed record BundleItemDto(
    string ComponentProductId,
    int Quantity,
    decimal? PriceOverride = null,
    bool IsOptional = false,
    int DisplayOrder = 0
);

