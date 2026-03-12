using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record UpdateVariantGroupCommand(
    string Id,
    string Name,
    string Description,
    string DisplayType,
    bool IsRequired,
    bool AllowMultiple,
    int DisplayOrder,
    bool IsActive
) : IRequest<Result>;

public sealed class UpdateVariantGroupCommandHandler : IRequestHandler<UpdateVariantGroupCommand, Result>
{
    private readonly CatalogDbContext _context;

    public UpdateVariantGroupCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateVariantGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.VariantGroups
            .FirstOrDefaultAsync(vg => vg.Id == request.Id, cancellationToken);

        if (group is null)
            return Result.Failure(Error.NotFound("VariantGroup.NotFound", "Variant group not found."));

        var validDisplayTypes = new[] { "Swatch", "Dropdown", "RadioButtons", "ImageGrid" };
        if (!validDisplayTypes.Contains(request.DisplayType))
            return Result.Failure(Error.Validation("VariantGroup.InvalidDisplayType",
                $"DisplayType must be one of: {string.Join(", ", validDisplayTypes)}"));

        group.Name = request.Name;
        group.Description = request.Description;
        group.DisplayType = request.DisplayType;
        group.IsRequired = request.IsRequired;
        group.AllowMultiple = request.AllowMultiple;
        group.DisplayOrder = request.DisplayOrder;
        group.IsActive = request.IsActive;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
