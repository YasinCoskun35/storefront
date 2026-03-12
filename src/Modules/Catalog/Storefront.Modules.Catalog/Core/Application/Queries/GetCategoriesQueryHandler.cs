using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Application.DTOs;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly CatalogDbContext _context;

    public GetCategoriesQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories.AsQueryable();

        if (!request.All)
        {
            if (request.ParentId is not null)
            {
                query = query.Where(c => c.ParentId == request.ParentId);
            }
            else
            {
                query = query.Where(c => c.ParentId == null);
            }
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        if (request.ShowInNavbar.HasValue)
        {
            query = query.Where(c => c.ShowInNavbar == request.ShowInNavbar.Value);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.Slug,
                c.ImageUrl,
                c.ParentId,
                c.DisplayOrder,
                c.IsActive,
                c.ShowInNavbar,
                c.Products.Count(p => p.IsActive)
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CategoryDto>>.Success(categories);
    }
}

