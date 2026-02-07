using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed class GetProductDetailsQueryHandler : IRequestHandler<GetProductDetailsQuery, Result<ProductDetailDto>>
{
    private readonly CatalogDbContext _context;

    public GetProductDetailsQueryHandler(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductDetailDto>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result<ProductDetailDto>.Failure(Error.NotFound("Product.NotFound", $"Product with ID '{request.ProductId}' not found."));
        }

        var images = product.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto(
                i.Id,
                i.Url,
                i.Type.ToString(),
                i.IsPrimary,
                i.DisplayOrder
            ))
            .ToList();

        var productDetail = new ProductDetailDto(
            product.Id,
            product.Name,
            product.SKU,
            product.Description,
            product.ShortDescription,
            product.Price,
            product.CompareAtPrice,
            product.StockStatus.ToString(),
            product.Quantity,
            product.Weight,
            product.Length,
            product.Width,
            product.Height,
            product.DimensionUnit,
            product.WeightUnit,
            product.CategoryId,
            product.Category.Name,
            product.BrandId,
            product.Brand?.Name,
            product.IsActive,
            product.IsFeatured,
            images,
            product.CreatedAt
        );

        return Result<ProductDetailDto>.Success(productDetail);
    }
}

