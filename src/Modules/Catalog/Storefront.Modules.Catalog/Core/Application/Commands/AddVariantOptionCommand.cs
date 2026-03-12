using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record AddVariantOptionCommand(
    string VariantGroupId,
    string Name,
    string Code,
    string? HexColor,
    string? ImageUrl,
    decimal? PriceAdjustment,
    bool IsAvailable,
    int DisplayOrder
) : IRequest<Result<string>>;

public sealed class AddVariantOptionCommandHandler : IRequestHandler<AddVariantOptionCommand, Result<string>>
{
    private readonly CatalogDbContext _context;

    public AddVariantOptionCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(AddVariantOptionCommand request, CancellationToken cancellationToken)
    {
        var groupExists = await _context.VariantGroups
            .AnyAsync(vg => vg.Id == request.VariantGroupId, cancellationToken);

        if (!groupExists)
            return Result<string>.Failure(Error.NotFound("VariantGroup.NotFound", "Variant group not found."));

        var codeExists = await _context.VariantOptions
            .AnyAsync(vo => vo.VariantGroupId == request.VariantGroupId && vo.Code == request.Code, cancellationToken);

        if (codeExists)
            return Result<string>.Failure(Error.Conflict("VariantOption.CodeExists",
                $"An option with code '{request.Code}' already exists in this group."));

        var option = new VariantOption
        {
            VariantGroupId = request.VariantGroupId,
            Name = request.Name,
            Code = request.Code,
            HexColor = request.HexColor,
            ImageUrl = request.ImageUrl,
            PriceAdjustment = request.PriceAdjustment,
            IsAvailable = request.IsAvailable,
            DisplayOrder = request.DisplayOrder,
        };

        _context.VariantOptions.Add(option);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(option.Id);
    }
}
