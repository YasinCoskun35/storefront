using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Application.Utilities;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<string>>
{
    private readonly CatalogDbContext _context;

    public CreateCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Generate slug if not provided
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(request.Name)
            : SlugGenerator.Generate(request.Slug);

        // Check if slug already exists
        var slugExists = await _context.Categories
            .AnyAsync(c => c.Slug == slug, cancellationToken);

        if (slugExists)
        {
            return Result<string>.Failure(Error.Conflict(
                "Category.SlugExists",
                $"A category with slug '{slug}' already exists"));
        }

        // Validate parent category if provided
        if (!string.IsNullOrWhiteSpace(request.ParentId))
        {
            var parentExists = await _context.Categories
                .AnyAsync(c => c.Id == request.ParentId, cancellationToken);

            if (!parentExists)
            {
                return Result<string>.Failure(Error.NotFound(
                    "Category.ParentNotFound",
                    $"Parent category with ID '{request.ParentId}' not found"));
            }
        }

        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            ParentId = request.ParentId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            ShowInNavbar = request.ShowInNavbar,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(category.Id);
    }
}



