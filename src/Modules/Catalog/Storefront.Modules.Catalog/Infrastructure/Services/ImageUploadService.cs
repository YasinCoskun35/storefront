using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Storefront.Modules.Catalog.Core.Application.Interfaces;

namespace Storefront.Modules.Catalog.Infrastructure.Services;

public sealed class ImageUploadService : IImageUploadService
{
    private readonly Channel<ImageUploadMessage> _channel;
    private readonly ILogger<ImageUploadService> _logger;
    private const string IncomingFolder = "uploads/incoming";

    public ImageUploadService(
        Channel<ImageUploadMessage> channel,
        ILogger<ImageUploadService> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public async Task<string> QueueImageForProcessingAsync(
        string productId,
        IFormFile file,
        bool isPrimary,
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("File is empty.");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException($"File type '{fileExtension}' is not supported. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Validate file size (max 10MB)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {maxFileSize / (1024 * 1024)}MB.");
        }

        // Ensure incoming directory exists
        Directory.CreateDirectory(IncomingFolder);

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var incomingFilePath = Path.Combine(IncomingFolder, uniqueFileName);

        // Save file to incoming folder
        using (var stream = new FileStream(incomingFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        _logger.LogInformation(
            "Image saved to incoming folder: {FilePath} for Product: {ProductId}",
            incomingFilePath,
            productId);

        // Queue the message for background processing
        var message = new ImageUploadMessage(productId, incomingFilePath, isPrimary);
        await _channel.Writer.WriteAsync(message, cancellationToken);

        _logger.LogInformation(
            "Image processing queued for Product: {ProductId}, File: {FileName}",
            productId,
            uniqueFileName);

        return uniqueFileName;
    }
}

