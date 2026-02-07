using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Application.DTOs;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Queries;

public sealed class GetContentSitemapQueryHandler : IRequestHandler<GetContentSitemapQuery, Result<IReadOnlyList<SitemapEntryDto>>>
{
    private readonly ContentDbContext _context;

    public GetContentSitemapQueryHandler(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<SitemapEntryDto>>> Handle(GetContentSitemapQuery request, CancellationToken cancellationToken)
    {
        var sitemapEntries = new List<SitemapEntryDto>();

        // Add published blog posts
        var blogPosts = await _context.BlogPosts
            .Where(bp => bp.IsPublished)
            .Select(bp => new SitemapEntryDto(
                $"/blog/{bp.Slug}",
                bp.UpdatedAt ?? bp.PublishedAt ?? bp.CreatedAt,
                "weekly",
                0.8
            ))
            .ToListAsync(cancellationToken);

        sitemapEntries.AddRange(blogPosts);

        // Add published static pages
        var staticPages = await _context.StaticPages
            .Where(sp => sp.IsPublished)
            .Select(sp => new SitemapEntryDto(
                $"/{sp.Slug}",
                sp.UpdatedAt ?? sp.CreatedAt,
                "monthly",
                0.6
            ))
            .ToListAsync(cancellationToken);

        sitemapEntries.AddRange(staticPages);

        return Result<IReadOnlyList<SitemapEntryDto>>.Success(sitemapEntries);
    }
}

