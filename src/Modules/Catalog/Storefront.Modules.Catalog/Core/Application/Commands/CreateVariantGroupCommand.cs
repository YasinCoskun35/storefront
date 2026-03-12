using MediatR;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record CreateVariantGroupCommand(
    string Name,
    string Description,
    string DisplayType,
    bool IsRequired,
    bool AllowMultiple,
    int DisplayOrder
) : IRequest<Result<string>>;

public sealed class CreateVariantGroupCommandHandler : IRequestHandler<CreateVariantGroupCommand, Result<string>>
{
    private readonly CatalogDbContext _context;

    public CreateVariantGroupCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateVariantGroupCommand request, CancellationToken cancellationToken)
    {
        var validDisplayTypes = new[] { "Swatch", "Dropdown", "RadioButtons", "ImageGrid" };
        if (!validDisplayTypes.Contains(request.DisplayType))
            return Result<string>.Failure(Error.Validation("VariantGroup.InvalidDisplayType",
                $"DisplayType must be one of: {string.Join(", ", validDisplayTypes)}"));

        var group = new VariantGroup
        {
            Name = request.Name,
            Description = request.Description,
            DisplayType = request.DisplayType,
            IsRequired = request.IsRequired,
            AllowMultiple = request.AllowMultiple,
            DisplayOrder = request.DisplayOrder,
        };

        _context.VariantGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(group.Id);
    }
}
