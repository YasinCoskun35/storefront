using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed record VariantOptionDto(
    string Id,
    string VariantGroupId,
    string Name,
    string Code,
    string? HexColor,
    string? ImageUrl,
    decimal? PriceAdjustment,
    bool IsAvailable,
    int DisplayOrder
);

public sealed record VariantGroupDetailsDto(
    string Id,
    string Name,
    string Description,
    string DisplayType,
    bool IsRequired,
    bool AllowMultiple,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<VariantOptionDto> Options
);

public sealed record GetVariantGroupDetailsQuery(string Id) : IRequest<Result<VariantGroupDetailsDto>>;

public sealed class GetVariantGroupDetailsQueryHandler : IRequestHandler<GetVariantGroupDetailsQuery, Result<VariantGroupDetailsDto>>
{
    private readonly CatalogDbContext _context;

    public GetVariantGroupDetailsQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<VariantGroupDetailsDto>> Handle(GetVariantGroupDetailsQuery request, CancellationToken cancellationToken)
    {
        var group = await _context.VariantGroups
            .Include(vg => vg.Options)
            .FirstOrDefaultAsync(vg => vg.Id == request.Id, cancellationToken);

        if (group is null)
            return Result<VariantGroupDetailsDto>.Failure(Error.NotFound("VariantGroup.NotFound", "Variant group not found."));

        var dto = new VariantGroupDetailsDto(
            group.Id,
            group.Name,
            group.Description,
            group.DisplayType,
            group.IsRequired,
            group.AllowMultiple,
            group.DisplayOrder,
            group.IsActive,
            group.CreatedAt,
            group.UpdatedAt,
            group.Options
                .OrderBy(o => o.DisplayOrder)
                .ThenBy(o => o.Name)
                .Select(o => new VariantOptionDto(
                    o.Id,
                    o.VariantGroupId,
                    o.Name,
                    o.Code,
                    o.HexColor,
                    o.ImageUrl,
                    o.PriceAdjustment,
                    o.IsAvailable,
                    o.DisplayOrder
                ))
                .ToList()
        );

        return Result<VariantGroupDetailsDto>.Success(dto);
    }
}
