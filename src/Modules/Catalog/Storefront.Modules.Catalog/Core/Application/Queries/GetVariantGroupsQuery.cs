using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed record VariantGroupSummaryDto(
    string Id,
    string Name,
    string Description,
    string DisplayType,
    bool IsRequired,
    bool AllowMultiple,
    int DisplayOrder,
    bool IsActive,
    int OptionCount
);

public sealed record GetVariantGroupsQuery(bool? IsActive = null) : IRequest<Result<List<VariantGroupSummaryDto>>>;

public sealed class GetVariantGroupsQueryHandler : IRequestHandler<GetVariantGroupsQuery, Result<List<VariantGroupSummaryDto>>>
{
    private readonly CatalogDbContext _context;

    public GetVariantGroupsQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<VariantGroupSummaryDto>>> Handle(GetVariantGroupsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VariantGroups.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(vg => vg.IsActive == request.IsActive.Value);

        var groups = await query
            .OrderBy(vg => vg.DisplayOrder)
            .ThenBy(vg => vg.Name)
            .Select(vg => new VariantGroupSummaryDto(
                vg.Id,
                vg.Name,
                vg.Description,
                vg.DisplayType,
                vg.IsRequired,
                vg.AllowMultiple,
                vg.DisplayOrder,
                vg.IsActive,
                vg.Options.Count
            ))
            .ToListAsync(cancellationToken);

        return Result<List<VariantGroupSummaryDto>>.Success(groups);
    }
}
