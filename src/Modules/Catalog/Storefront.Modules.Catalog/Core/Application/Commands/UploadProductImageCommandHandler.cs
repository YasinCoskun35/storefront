using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Core.Application.Interfaces;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, Result<string>>
{
    private readonly CatalogDbContext _context;
    private readonly IImageUploadService _imageUploadService;

    public UploadProductImageCommandHandler(
        CatalogDbContext context,
        IImageUploadService imageUploadService)
    {
        _context = context;
        _imageUploadService = imageUploadService;
    }

    public async Task<Result<string>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        // Verify product exists
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

        if (!productExists)
        {
            return Result<string>.Failure(Error.NotFound("Product.NotFound", $"Product with ID '{request.ProductId}' not found."));
        }

        try
        {
            // Queue the image for background processing
            var fileName = await _imageUploadService.QueueImageForProcessingAsync(
                request.ProductId,
                request.File,
                request.IsPrimary,
                cancellationToken);

            return Result<string>.Success(fileName);
        }
        catch (InvalidOperationException ex)
        {
            return Result<string>.Failure(Error.Validation("Image.Invalid", ex.Message));
        }
    }
}

