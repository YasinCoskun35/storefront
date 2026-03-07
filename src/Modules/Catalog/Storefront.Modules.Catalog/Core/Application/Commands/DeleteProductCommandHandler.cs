using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly CatalogDbContext _context;

    public DeleteProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.BundleItems)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            return Result.Failure(Error.NotFound("Product.NotFound", $"Product with ID '{request.ProductId}' not found."));
        }

        // Check if this product is used in any bundles
        var usedInBundles = await _context.ProductBundleItems
            .AnyAsync(bi => bi.ComponentProductId == request.ProductId, cancellationToken);

        if (usedInBundles)
        {
            return Result.Failure(Error.Conflict(
                "Product.UsedInBundles", 
                "Cannot delete product because it is used as a component in one or more bundle products."));
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
