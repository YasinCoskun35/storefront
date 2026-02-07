using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Storefront.Modules.Catalog.Core.Application.Interfaces;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Core.Domain.Enums;
using Storefront.Modules.Catalog.Infrastructure.Persistence;

namespace Storefront.Modules.Catalog.Infrastructure.BackgroundJobs;

public sealed class ImageProcessingBackgroundService : BackgroundService
{
    private readonly Channel<ImageUploadMessage> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImageProcessingBackgroundService> _logger;

    public ImageProcessingBackgroundService(
        Channel<ImageUploadMessage> channel,
        IServiceProvider serviceProvider,
        ILogger<ImageProcessingBackgroundService> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Image Processing Background Service started.");

        await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessImageAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing image for Product: {ProductId}, File: {FilePath}",
                    message.ProductId,
                    message.OriginalFilePath);
            }
        }

        _logger.LogInformation("Image Processing Background Service stopped.");
    }

    private async Task ProcessImageAsync(ImageUploadMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing image for Product: {ProductId}, File: {FilePath}",
            message.ProductId,
            message.OriginalFilePath);

        if (!File.Exists(message.OriginalFilePath))
        {
            _logger.LogWarning("Original file not found: {FilePath}", message.OriginalFilePath);
            return;
        }

        // Create product-specific upload directory
        var productUploadDir = Path.Combine("uploads", "products", message.ProductId);
        Directory.CreateDirectory(productUploadDir);

        var imageVariants = new List<(ImageType Type, int Size, string Path)>
        {
            (ImageType.Thumbnail, 200, Path.Combine(productUploadDir, $"thumbnail_{Guid.NewGuid()}.webp")),
            (ImageType.Medium, 600, Path.Combine(productUploadDir, $"medium_{Guid.NewGuid()}.webp")),
            (ImageType.Large, 1200, Path.Combine(productUploadDir, $"large_{Guid.NewGuid()}.webp"))
        };

        var createdImages = new List<ProductImage>();

        using (var image = await Image.LoadAsync(message.OriginalFilePath, cancellationToken))
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Save original as WebP
            var originalWebPPath = Path.Combine(productUploadDir, $"original_{Guid.NewGuid()}.webp");
            await image.SaveAsync(originalWebPPath, new WebpEncoder { Quality = 90 }, cancellationToken);

            var originalFileInfo = new FileInfo(originalWebPPath);
            createdImages.Add(new ProductImage
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = message.ProductId,
                Url = $"/{originalWebPPath.Replace("\\", "/")}",
                Type = ImageType.Original,
                IsPrimary = message.IsPrimary && createdImages.Count == 0,
                DisplayOrder = 0,
                FileSizeBytes = originalFileInfo.Length,
                Width = originalWidth,
                Height = originalHeight,
                CreatedAt = DateTime.UtcNow
            });

            // Generate and save variants
            foreach (var (type, size, path) in imageVariants)
            {
                try
                {
                    using var resizedImage = image.Clone(ctx => ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(size, 0),
                        Mode = ResizeMode.Max,
                        Sampler = KnownResamplers.Lanczos3
                    }));

                    await resizedImage.SaveAsync(path, new WebpEncoder { Quality = 85 }, cancellationToken);

                    var fileInfo = new FileInfo(path);
                    createdImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = message.ProductId,
                        Url = $"/{path.Replace("\\", "/")}",
                        Type = type,
                        IsPrimary = false,
                        DisplayOrder = (int)type,
                        FileSizeBytes = fileInfo.Length,
                        Width = resizedImage.Width,
                        Height = resizedImage.Height,
                        CreatedAt = DateTime.UtcNow
                    });

                    _logger.LogInformation(
                        "Created {Type} variant: {Path} ({Width}x{Height})",
                        type,
                        path,
                        resizedImage.Width,
                        resizedImage.Height);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating {Type} variant for Product: {ProductId}", type, message.ProductId);
                }
            }
        }

        // Save to database
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await dbContext.ProductImages.AddRangeAsync(createdImages, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully processed {Count} image variants for Product: {ProductId}",
            createdImages.Count,
            message.ProductId);

        // Delete the original incoming file
        try
        {
            File.Delete(message.OriginalFilePath);
            _logger.LogInformation("Deleted incoming file: {FilePath}", message.OriginalFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete incoming file: {FilePath}", message.OriginalFilePath);
        }
    }
}

