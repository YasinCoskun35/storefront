using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class RemoveComponentFromBundleCommandHandler : IRequestHandler<RemoveComponentFromBundleCommand, Result<bool>>
{
    private readonly CatalogDbContext _context;

    public RemoveComponentFromBundleCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RemoveComponentFromBundleCommand request, CancellationToken cancellationToken)
    {
        var bundleItem = await _context.ProductBundleItems
            .FirstOrDefaultAsync(
                bi => bi.BundleProductId == request.BundleProductId 
                   && bi.ComponentProductId == request.ComponentProductId,
                cancellationToken);

        if (bundleItem is null)
        {
            return Result<bool>.Failure(
                Error.NotFound("BundleItem.NotFound", "The specified component is not part of this bundle."));
        }

        _context.ProductBundleItems.Remove(bundleItem);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
