using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Application.DTOs;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Queries;

public sealed class GetBlogPostsQueryHandler : IRequestHandler<GetBlogPostsQuery, Result<PagedResult<BlogPostSummaryDto>>>
{
    private readonly ContentDbContext _context;

    public GetBlogPostsQueryHandler(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<BlogPostSummaryDto>>> Handle(GetBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BlogPosts.AsQueryable();

        // Apply filters
        if (request.IsPublished.HasValue)
        {
            query = query.Where(bp => bp.IsPublished == request.IsPublished.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(bp => bp.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            // Simple contains check for tag (since tags are comma-separated)
            query = query.Where(bp => bp.Tags != null && bp.Tags.Contains(request.Tag));
        }

        // Order by published date (newest first)
        query = query.OrderByDescending(bp => bp.PublishedAt ?? bp.CreatedAt);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var blogPosts = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(bp => new BlogPostSummaryDto(
                bp.Id,
                bp.Title,
                bp.Slug,
                bp.Summary,
                bp.FeaturedImage,
                bp.Author,
                bp.PublishedAt,
                bp.ViewCount,
                bp.Category
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<BlogPostSummaryDto>(
            blogPosts,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PagedResult<BlogPostSummaryDto>>.Success(pagedResult);
    }
}

