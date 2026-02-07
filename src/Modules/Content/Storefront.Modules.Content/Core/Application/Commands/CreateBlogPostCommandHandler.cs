using MediatR;
using Storefront.Modules.Content.Core.Application.Interfaces;
using Storefront.Modules.Content.Core.Domain.Entities;
using Storefront.Modules.Content.Core.Domain.ValueObjects;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Commands;

public sealed class CreateBlogPostCommandHandler : IRequestHandler<CreateBlogPostCommand, Result<string>>
{
    private readonly ContentDbContext _context;
    private readonly ISlugService _slugService;

    public CreateBlogPostCommandHandler(ContentDbContext context, ISlugService slugService)
    {
        _context = context;
        _slugService = slugService;
    }

    public async Task<Result<string>> Handle(CreateBlogPostCommand request, CancellationToken cancellationToken)
    {
        // Generate unique slug
        var slug = await _slugService.GenerateUniqueSlugAsync(request.Title, null, cancellationToken);

        // Auto-fill SEO metadata
        var seoMetadata = new SeoMetadata
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

        var blogPost = new BlogPost
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Slug = slug,
            Summary = request.Summary,
            Body = request.Body,
            FeaturedImage = request.FeaturedImage,
            Author = request.Author,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            Tags = request.Tags,
            Category = request.Category,
            SeoMetadata = seoMetadata,
            CreatedAt = DateTime.UtcNow
        };

        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(blogPost.Id);
    }

    private static string? ExtractMetaDescription(string? summary, string body)
    {
        // Use summary if available
        if (!string.IsNullOrWhiteSpace(summary))
        {
            return summary.Length > 160 ? summary.Substring(0, 160) + "..." : summary;
        }

        // Otherwise, extract from body (strip HTML tags and limit to 160 chars)
        if (!string.IsNullOrWhiteSpace(body))
        {
            var plainText = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", " ");
            plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ").Trim();
            return plainText.Length > 160 ? plainText.Substring(0, 160) + "..." : plainText;
        }

        return null;
    }
}

