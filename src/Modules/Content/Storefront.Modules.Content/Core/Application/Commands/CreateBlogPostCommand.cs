using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Commands;

public sealed record CreateBlogPostCommand(
    string Title,
    string? Summary,
    string Body,
    string? FeaturedImage,
    string? Author,
    bool IsPublished,
    string? Tags,
    string? Category,
    string? MetaTitle,
    string? MetaDescription,
    string? Keywords,
    string? OgImage
) : IRequest<Result<string>>;

