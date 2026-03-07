using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly CatalogDbContext _context;

    public DeleteCategoryCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            return Result.Failure(Error.NotFound("Category.NotFound", $"Category with ID '{request.CategoryId}' not found."));
        }

        // Check if category has products
        if (category.Products.Any())
        {
            return Result.Failure(Error.Conflict(
                "Category.HasProducts",
                $"Cannot delete category because it contains {category.Products.Count} product(s). Please move or delete the products first."));
        }

        // Check if category has child categories
        if (category.Children.Any())
        {
            return Result.Failure(Error.Conflict(
                "Category.HasChildren",
                $"Cannot delete category because it has {category.Children.Count} subcategory(ies). Please delete the subcategories first."));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
