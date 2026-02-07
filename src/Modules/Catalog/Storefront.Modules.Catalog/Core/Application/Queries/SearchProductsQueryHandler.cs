using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Application.DTOs;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly CatalogDbContext _context;

    public SearchProductsQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .AsQueryable();

        // Apply search using trigram similarity for fuzzy matching
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            
            // Use trigram similarity for fuzzy matching
            // This allows "drill bit" to match "bit for drill"
            query = query.Where(p =>
                EF.Functions.TrigramsSimilarity(p.Name, searchTerm) > 0.1 ||
                EF.Functions.TrigramsSimilarity(p.SKU, searchTerm) > 0.1 ||
                (p.Description != null && EF.Functions.TrigramsSimilarity(p.Description, searchTerm) > 0.05) ||
                p.Name.Contains(searchTerm) ||
                p.SKU.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm))
            )
            .OrderByDescending(p => 
                EF.Functions.TrigramsSimilarity(p.Name, searchTerm) +
                EF.Functions.TrigramsSimilarity(p.SKU, searchTerm) * 0.8
            );
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            query = query.Where(p => p.CategoryId == request.CategoryId);
        }

        if (!string.IsNullOrWhiteSpace(request.BrandId))
        {
            query = query.Where(p => p.BrandId == request.BrandId);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.SKU,
                p.Description,
                p.ShortDescription,
                p.ProductType,
                p.Price,
                p.CompareAtPrice,
                p.BundlePrice,
                p.CanBeSoldSeparately,
                p.StockStatus,
                p.Quantity,
                p.CategoryId,
                p.Category.Name,
                p.BrandId,
                p.Brand != null ? p.Brand.Name : null,
                p.IsActive,
                p.IsFeatured,
                p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                p.CreatedAt,
                false,  // PricingEnabled - will be set by handler
                null    // PriceLabel - will be set by handler
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<ProductDto>(
            products,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }
}

