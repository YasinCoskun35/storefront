using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record UpdateVariantOptionCommand(
    string Id,
    string Name,
    string Code,
    string? HexColor,
    string? ImageUrl,
    decimal? PriceAdjustment,
    bool IsAvailable,
    int DisplayOrder
) : IRequest<Result>;

public sealed class UpdateVariantOptionCommandHandler : IRequestHandler<UpdateVariantOptionCommand, Result>
{
    private readonly CatalogDbContext _context;

    public UpdateVariantOptionCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateVariantOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await _context.VariantOptions
            .FirstOrDefaultAsync(vo => vo.Id == request.Id, cancellationToken);

        if (option is null)
            return Result.Failure(Error.NotFound("VariantOption.NotFound", "Variant option not found."));

        var codeConflict = await _context.VariantOptions
            .AnyAsync(vo => vo.VariantGroupId == option.VariantGroupId && vo.Code == request.Code && vo.Id != request.Id,
                cancellationToken);

        if (codeConflict)
            return Result.Failure(Error.Conflict("VariantOption.CodeExists",
                $"An option with code '{request.Code}' already exists in this group."));

        option.Name = request.Name;
        option.Code = request.Code;
        option.HexColor = request.HexColor;
        option.ImageUrl = request.ImageUrl;
        option.PriceAdjustment = request.PriceAdjustment;
        option.IsAvailable = request.IsAvailable;
        option.DisplayOrder = request.DisplayOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
