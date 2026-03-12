using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record AssignVariantGroupToProductCommand(
    string ProductId,
    string VariantGroupId,
    bool IsRequired,
    int DisplayOrder
) : IRequest<Result<string>>;

public sealed class AssignVariantGroupToProductCommandHandler : IRequestHandler<AssignVariantGroupToProductCommand, Result<string>>
{
    private readonly CatalogDbContext _context;

    public AssignVariantGroupToProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(AssignVariantGroupToProductCommand request, CancellationToken cancellationToken)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
            return Result<string>.Failure(Error.NotFound("Product.NotFound", "Product not found."));

        var groupExists = await _context.VariantGroups
            .AnyAsync(vg => vg.Id == request.VariantGroupId, cancellationToken);

        if (!groupExists)
            return Result<string>.Failure(Error.NotFound("VariantGroup.NotFound", "Variant group not found."));

        var alreadyAssigned = await _context.ProductVariantGroups
            .AnyAsync(pvg => pvg.ProductId == request.ProductId && pvg.VariantGroupId == request.VariantGroupId,
                cancellationToken);

        if (alreadyAssigned)
            return Result<string>.Failure(Error.Conflict("ProductVariantGroup.AlreadyAssigned",
                "This variant group is already assigned to the product."));

        var assignment = new ProductVariantGroup
        {
            ProductId = request.ProductId,
            VariantGroupId = request.VariantGroupId,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder,
        };

        _context.ProductVariantGroups.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(assignment.Id);
    }
}
