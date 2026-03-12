using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record RemoveVariantGroupFromProductCommand(
    string ProductId,
    string VariantGroupId
) : IRequest<Result>;

public sealed class RemoveVariantGroupFromProductCommandHandler : IRequestHandler<RemoveVariantGroupFromProductCommand, Result>
{
    private readonly CatalogDbContext _context;

    public RemoveVariantGroupFromProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveVariantGroupFromProductCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.ProductVariantGroups
            .FirstOrDefaultAsync(pvg => pvg.ProductId == request.ProductId && pvg.VariantGroupId == request.VariantGroupId,
                cancellationToken);

        if (assignment is null)
            return Result.Failure(Error.NotFound("ProductVariantGroup.NotFound",
                "This variant group is not assigned to the product."));

        _context.ProductVariantGroups.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
