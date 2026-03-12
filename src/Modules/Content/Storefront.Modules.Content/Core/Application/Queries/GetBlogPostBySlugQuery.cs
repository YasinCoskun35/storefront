using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Application.DTOs;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Queries;

public record GetBlogPostBySlugQuery(string Slug) : IRequest<Result<BlogPostDto>>;

public class GetBlogPostBySlugQueryHandler : IRequestHandler<GetBlogPostBySlugQuery, Result<BlogPostDto>>
{
    private readonly ContentDbContext _context;

    public GetBlogPostBySlugQueryHandler(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BlogPostDto>> Handle(GetBlogPostBySlugQuery request, CancellationToken cancellationToken)
    {
        var post = await _context.BlogPosts
            .FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken);

        if (post is null)
            return Result<BlogPostDto>.Failure(Error.NotFound("BlogPost.NotFound", $"Blog post '{request.Slug}' not found."));

        var dto = new BlogPostDto(
            post.Id, post.Title, post.Slug, post.Summary,
            post.Body, post.FeaturedImage, post.Author,
            post.IsPublished, post.PublishedAt, post.ViewCount,
            post.Tags, post.Category, post.CreatedAt
        );

        return Result<BlogPostDto>.Success(dto);
    }
}
