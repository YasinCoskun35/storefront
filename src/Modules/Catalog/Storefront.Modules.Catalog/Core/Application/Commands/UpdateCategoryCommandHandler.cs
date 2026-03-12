using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Application.Utilities;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly CatalogDbContext _context;

    public UpdateCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("Category.NotFound", $"Category '{request.Id}' not found."));

        var slug = string.IsNullOrEmpty(request.Slug)
            ? SlugGenerator.Generate(request.Name)
            : request.Slug;

        // Check slug uniqueness (excluding current category)
        var slugExists = await _context.Categories
            .AnyAsync(c => c.Slug == slug && c.Id != request.Id, cancellationToken);

        if (slugExists)
            return Result.Failure(Error.Conflict("Category.SlugExists", $"A category with slug '{slug}' already exists."));

        category.Name = request.Name;
        category.Description = request.Description;
        category.Slug = slug;
        category.ParentId = string.IsNullOrEmpty(request.ParentId) ? null : request.ParentId;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.ShowInNavbar = request.ShowInNavbar;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
