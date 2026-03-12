using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record DeleteVariantGroupCommand(string Id) : IRequest<Result>;

public sealed class DeleteVariantGroupCommandHandler : IRequestHandler<DeleteVariantGroupCommand, Result>
{
    private readonly CatalogDbContext _context;

    public DeleteVariantGroupCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteVariantGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.VariantGroups
            .FirstOrDefaultAsync(vg => vg.Id == request.Id, cancellationToken);

        if (group is null)
            return Result.Failure(Error.NotFound("VariantGroup.NotFound", "Variant group not found."));

        var hasAssignments = await _context.ProductVariantGroups
            .AnyAsync(pvg => pvg.VariantGroupId == request.Id, cancellationToken);

        if (hasAssignments)
            return Result.Failure(Error.Conflict("VariantGroup.HasAssignments",
                "Cannot delete a variant group that is assigned to products. Remove all product assignments first."));

        _context.VariantGroups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
