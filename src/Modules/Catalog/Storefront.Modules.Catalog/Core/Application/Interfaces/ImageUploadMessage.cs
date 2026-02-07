namespace Storefront.Modules.Catalog.Core.Application.Interfaces;

public sealed record ImageUploadMessage(
    string ProductId,
    string OriginalFilePath,
    bool IsPrimary
);

