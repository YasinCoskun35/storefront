using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Application.Utilities;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Core.Domain.Enums;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<string>>
{
    private readonly CatalogDbContext _context;

    public CreateProductCommandHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU == request.SKU, cancellationToken);

        if (skuExists)
        {
            return Result<string>.Failure(Error.Conflict("Product.SKUExists", $"A product with SKU '{request.SKU}' already exists."));
        }

        // Verify category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result<string>.Failure(Error.NotFound("Category.NotFound", $"Category with ID '{request.CategoryId}' not found."));
        }

        // Verify brand exists if provided
        if (!string.IsNullOrWhiteSpace(request.BrandId))
        {
            var brandExists = await _context.Brands
                .AnyAsync(b => b.Id == request.BrandId, cancellationToken);

            if (!brandExists)
            {
                return Result<string>.Failure(Error.NotFound("Brand.NotFound", $"Brand with ID '{request.BrandId}' not found."));
            }
        }
        
        // Validate bundle components if this is a bundle product
        if (request.ProductType == ProductType.Bundle && request.BundleItems?.Any() == true)
        {
            var componentIds = request.BundleItems.Select(bi => bi.ComponentProductId).ToList();
            var existingComponents = await _context.Products
                .Where(p => componentIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
                
            var missingComponents = componentIds.Except(existingComponents).ToList();
            if (missingComponents.Any())
            {
                return Result<string>.Failure(Error.NotFound(
                    "Product.ComponentsNotFound", 
                    $"Component products not found: {string.Join(", ", missingComponents)}"));
            }
        }

        // Generate slug from name
        var slug = SlugGenerator.Generate(request.Name);

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            ProductType = request.ProductType,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            BundlePrice = request.BundlePrice,
            CanBeSoldSeparately = request.CanBeSoldSeparately,
            StockStatus = request.StockStatus ?? StockStatus.InStock,  // Default to InStock
            Quantity = request.Quantity ?? 0,  // Default to 0
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Weight = request.Weight,
            Length = request.Length,
            Width = request.Width,
            Height = request.Height,
            DimensionUnit = "cm",
            WeightUnit = "kg",
            Slug = slug,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        
        // Add bundle items if this is a bundle product
        if (request.ProductType == ProductType.Bundle && request.BundleItems?.Any() == true)
        {
            foreach (var bundleItem in request.BundleItems)
            {
                var productBundleItem = new ProductBundleItem
                {
                    Id = Guid.NewGuid().ToString(),
                    BundleProductId = product.Id,
                    ComponentProductId = bundleItem.ComponentProductId,
                    Quantity = bundleItem.Quantity,
                    PriceOverride = bundleItem.PriceOverride,
                    IsOptional = bundleItem.IsOptional,
                    DisplayOrder = bundleItem.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.ProductBundleItems.Add(productBundleItem);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(product.Id);
    }
}

