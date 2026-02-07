namespace Storefront.Modules.Content.Core.Application.Interfaces;

public interface ISlugService
{
    Task<string> GenerateUniqueSlugAsync(string text, string? entityId = null, CancellationToken cancellationToken = default);
}

