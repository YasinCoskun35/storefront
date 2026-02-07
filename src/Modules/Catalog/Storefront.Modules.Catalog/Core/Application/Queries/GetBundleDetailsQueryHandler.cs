using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Domain.Enums;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed class GetBundleDetailsQueryHandler : IRequestHandler<GetBundleDetailsQuery, Result<BundleDetailDto>>
{
    private readonly CatalogDbContext _context;

    public GetBundleDetailsQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BundleDetailDto>> Handle(GetBundleDetailsQuery request, CancellationToken cancellationToken)
    {
        var bundle = await _context.Products
            .Include(p => p.BundleItems)
                .ThenInclude(bi => bi.ComponentProduct)
                    .ThenInclude(cp => cp.Images)
            .FirstOrDefaultAsync(p => p.Id == request.BundleProductId, cancellationToken);

        if (bundle is null)
        {
            return Result<BundleDetailDto>.Failure(
                Error.NotFound("Bundle.NotFound", $"Bundle with ID '{request.BundleProductId}' not found."));
        }

        if (bundle.ProductType != ProductType.Bundle)
        {
            return Result<BundleDetailDto>.Failure(
                Error.Validation("Bundle.NotABundle", "The specified product is not a bundle."));
        }

        var components = bundle.BundleItems
            .OrderBy(bi => bi.DisplayOrder)
            .Select(bi =>
            {
                var componentPrice = bi.PriceOverride ?? bi.ComponentProduct.Price;
                var totalPrice = componentPrice.HasValue ? componentPrice.Value * bi.Quantity : (decimal?)null;
                var primaryImage = bi.ComponentProduct.Images
                    .Where(i => i.IsPrimary)
                    .Select(i => i.Url)
                    .FirstOrDefault();

                return new BundleComponentDto(
                    bi.ComponentProduct.Id,
                    bi.ComponentProduct.Name,
                    bi.ComponentProduct.SKU,
                    primaryImage,
                    bi.Quantity,
                    bi.ComponentProduct.Price,
                    bi.PriceOverride,
                    totalPrice,
                    bi.IsOptional,
                    bi.DisplayOrder
                );
            })
            .ToList();

        // Calculate total price from components
        var calculatedPrice = components
            .Where(c => c.TotalPrice.HasValue)
            .Sum(c => c.TotalPrice!.Value);

        // Calculate savings if bundle price is set
        decimal? savings = null;
        if (bundle.BundlePrice.HasValue && calculatedPrice > 0)
        {
            savings = calculatedPrice - bundle.BundlePrice.Value;
        }

        var result = new BundleDetailDto(
            bundle.Id,
            bundle.Name,
            bundle.SKU,
            bundle.Description,
            bundle.BundlePrice,
            calculatedPrice > 0 ? calculatedPrice : null,
            savings,
            components
        );

        return Result<BundleDetailDto>.Success(result);
    }
}
