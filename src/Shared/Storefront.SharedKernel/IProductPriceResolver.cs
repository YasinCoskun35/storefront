namespace Storefront.SharedKernel;

public interface IProductPriceResolver
{
    Task<decimal?> GetPriceAsync(string productId, CancellationToken ct = default);
}
