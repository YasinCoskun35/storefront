using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Core.Domain.Enums;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class AddComponentToBundleCommandHandler : IRequestHandler<AddComponentToBundleCommand, Result<string>>
{
    private readonly CatalogDbContext _context;

    public AddComponentToBundleCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(AddComponentToBundleCommand request, CancellationToken cancellationToken)
    {
        // Verify bundle product exists and is a bundle
        var bundleProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.BundleProductId, cancellationToken);

        if (bundleProduct is null)
        {
            return Result<string>.Failure(
                Error.NotFound("Bundle.NotFound", $"Bundle product with ID '{request.BundleProductId}' not found."));
        }

        if (bundleProduct.ProductType != ProductType.Bundle)
        {
            return Result<string>.Failure(
                Error.Validation("Bundle.NotABundle", "The specified product is not a bundle."));
        }

        // Verify component product exists
        var componentExists = await _context.Products
            .AnyAsync(p => p.Id == request.ComponentProductId, cancellationToken);

        if (!componentExists)
        {
            return Result<string>.Failure(
                Error.NotFound("Component.NotFound", $"Component product with ID '{request.ComponentProductId}' not found."));
        }

        // Check if component already exists in bundle
        var existingComponent = await _context.ProductBundleItems
            .AnyAsync(bi => bi.BundleProductId == request.BundleProductId 
                         && bi.ComponentProductId == request.ComponentProductId, 
                      cancellationToken);

        if (existingComponent)
        {
            return Result<string>.Failure(
                Error.Conflict("Bundle.ComponentExists", "This component is already part of the bundle."));
        }

        var bundleItem = new ProductBundleItem
        {
            Id = Guid.NewGuid().ToString(),
            BundleProductId = request.BundleProductId,
            ComponentProductId = request.ComponentProductId,
            Quantity = request.Quantity,
            PriceOverride = request.PriceOverride,
            IsOptional = request.IsOptional,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductBundleItems.Add(bundleItem);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(bundleItem.Id);
    }
}
