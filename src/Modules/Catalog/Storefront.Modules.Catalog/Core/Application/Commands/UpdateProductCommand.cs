using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Application.Utilities;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record UpdateProductCommand(
    string Id,
    string Name,
    string SKU,
    string? Description,
    string? ShortDescription,
    string CategoryId,
    decimal? Weight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    bool IsActive,
    bool IsFeatured
) : IRequest<Result>;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly CatalogDbContext _context;

    public UpdateProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
            return Result.Failure(Error.NotFound("Product.NotFound", $"Product '{request.Id}' not found."));

        // Check SKU uniqueness only if it changed
        if (!string.Equals(product.SKU, request.SKU, StringComparison.OrdinalIgnoreCase))
        {
            var skuTaken = await _context.Products
                .AnyAsync(p => p.SKU == request.SKU && p.Id != request.Id, cancellationToken);

            if (skuTaken)
                return Result.Failure(Error.Conflict("Product.SKUExists", $"A product with SKU '{request.SKU}' already exists."));
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            return Result.Failure(Error.NotFound("Category.NotFound", $"Category '{request.CategoryId}' not found."));

        product.Name = request.Name;
        product.SKU = request.SKU;
        product.Description = request.Description;
        product.ShortDescription = request.ShortDescription;
        product.CategoryId = request.CategoryId;
        product.Weight = request.Weight;
        product.Length = request.Length;
        product.Width = request.Width;
        product.Height = request.Height;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.Slug = SlugGenerator.Generate(request.Name);
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
