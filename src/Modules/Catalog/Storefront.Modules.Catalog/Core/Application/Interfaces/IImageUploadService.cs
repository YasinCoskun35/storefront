using Microsoft.AspNetCore.Http;

namespace Storefront.Modules.Catalog.Core.Application.Interfaces;

public interface IImageUploadService
{
    Task<string> QueueImageForProcessingAsync(string productId, IFormFile file, bool isPrimary, CancellationToken cancellationToken = default);
}

