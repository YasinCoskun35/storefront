using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed record GetBundleDetailsQuery(string BundleProductId) : IRequest<Result<BundleDetailDto>>;

public sealed record BundleDetailDto(
    string Id,
    string Name,
    string SKU,
    string? Description,
    decimal? BundlePrice,
    decimal? CalculatedPrice,  // Sum of component prices
    decimal? Savings,  // BundlePrice - CalculatedPrice (if bundlePrice is set)
    IReadOnlyList<BundleComponentDto> Components
);

public sealed record BundleComponentDto(
    string ComponentId,
    string Name,
    string SKU,
    string? ImageUrl,
    int Quantity,
    decimal? UnitPrice,
    decimal? PriceOverride,
    decimal? TotalPrice,  // (PriceOverride ?? UnitPrice) * Quantity
    bool IsOptional,
    int DisplayOrder
);
