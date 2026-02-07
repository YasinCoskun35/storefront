using MediatR;
using Storefront.Modules.Catalog.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed record SearchProductsQuery(
    string? SearchTerm,
    string? CategoryId,
    string? BrandId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ProductDto>>>;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

