using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Application.Interfaces;
using Storefront.Modules.Content.Core.Domain.ValueObjects;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Commands;

public sealed class UpdateBlogPostCommandHandler : IRequestHandler<UpdateBlogPostCommand, Result<string>>
{
    private readonly ContentDbContext _context;
    private readonly ISlugService _slugService;

    public UpdateBlogPostCommandHandler(ContentDbContext context, ISlugService slugService)
    {
        _context = context;
        _slugService = slugService;
    }

    public async Task<Result<string>> Handle(UpdateBlogPostCommand request, CancellationToken cancellationToken)
    {
        var blogPost = await _context.BlogPosts
            .FirstOrDefaultAsync(bp => bp.Id == request.Id, cancellationToken);

        if (blogPost is null)
        {
            return Result<string>.Failure(Error.NotFound("BlogPost.NotFound", $"Blog post with ID '{request.Id}' not found."));
        }

        // Regenerate slug if title changed
        if (blogPost.Title != request.Title)
        {
            blogPost.Slug = await _slugService.GenerateUniqueSlugAsync(request.Title, blogPost.Id, cancellationToken);
        }

        // Update properties
        blogPost.Title = request.Title;
        blogPost.Summary = request.Summary;
        blogPost.Body = request.Body;
        blogPost.FeaturedImage = request.FeaturedImage;
        blogPost.Author = request.Author;
        blogPost.Tags = request.Tags;
        blogPost.Category = request.Category;
        blogPost.UpdatedAt = DateTime.UtcNow;

        // Handle publication status change
        if (request.IsPublished && !blogPost.IsPublished)
        {
            blogPost.PublishedAt = DateTime.UtcNow;
        }
        blogPost.IsPublished = request.IsPublished;

        // Auto-fill SEO metadata
        blogPost.SeoMetadata = new SeoMetadata
        {
            MetaTitle = !string.IsNullOrWhiteSpace(request.MetaTitle) 
                ? request.MetaTitle 
                : request.Title,
            
            MetaDescription = !string.IsNullOrWhiteSpace(request.MetaDescription) 
                ? request.MetaDescription 
                : ExtractMetaDescription(request.Summary, request.Body),
            
            Keywords = request.Keywords,
            OgImage = request.OgImage ?? request.FeaturedImage,
            OgType = "article"
        };

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(blogPost.Id);
    }

    private static string? ExtractMetaDescription(string? summary, string body)
    {
        if (!string.IsNullOrWhiteSpace(summary))
        {
            return summary.Length > 160 ? summary.Substring(0, 160) + "..." : summary;
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            var plainText = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", " ");
            plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ").Trim();
            return plainText.Length > 160 ? plainText.Substring(0, 160) + "..." : plainText;
        }

        return null;
    }
}

