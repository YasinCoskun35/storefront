using MediatR;
using Storefront.Modules.Content.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Queries;

public sealed record GetBlogPostsQuery(
    string? Category = null,
    string? Tag = null,
    bool? IsPublished = true,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<BlogPostSummaryDto>>>;

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

