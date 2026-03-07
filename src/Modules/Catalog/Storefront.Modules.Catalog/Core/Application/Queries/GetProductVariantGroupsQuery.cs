using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed record ProductVariantGroupDto(
    string Id,
    string ProductId,
    string VariantGroupId,
    bool IsRequired,
    int DisplayOrder,
    VariantGroupDetailsDto VariantGroup
);

public sealed record GetProductVariantGroupsQuery(string ProductId) : IRequest<Result<List<ProductVariantGroupDto>>>;

public sealed class GetProductVariantGroupsQueryHandler : IRequestHandler<GetProductVariantGroupsQuery, Result<List<ProductVariantGroupDto>>>
{
    private readonly CatalogDbContext _context;

    public GetProductVariantGroupsQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductVariantGroupDto>>> Handle(GetProductVariantGroupsQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _context.ProductVariantGroups
            .Include(pvg => pvg.VariantGroup)
                .ThenInclude(vg => vg.Options)
            .Where(pvg => pvg.ProductId == request.ProductId)
            .OrderBy(pvg => pvg.DisplayOrder)
            .ToListAsync(cancellationToken);

        var result = assignments
            .Select(pvg => new ProductVariantGroupDto(
                pvg.Id,
                pvg.ProductId,
                pvg.VariantGroupId,
                pvg.IsRequired,
                pvg.DisplayOrder,
                new VariantGroupDetailsDto(
                    pvg.VariantGroup.Id,
                    pvg.VariantGroup.Name,
                    pvg.VariantGroup.Description,
                    pvg.VariantGroup.DisplayType,
                    pvg.VariantGroup.IsRequired,
                    pvg.VariantGroup.AllowMultiple,
                    pvg.VariantGroup.DisplayOrder,
                    pvg.VariantGroup.IsActive,
                    pvg.VariantGroup.CreatedAt,
                    pvg.VariantGroup.UpdatedAt,
                    pvg.VariantGroup.Options
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
                )
            ))
            .ToList();

        return Result<List<ProductVariantGroupDto>>.Success(result);
    }
}
