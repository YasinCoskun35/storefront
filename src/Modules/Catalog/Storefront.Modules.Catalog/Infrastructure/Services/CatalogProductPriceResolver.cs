using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Infrastructure.Services;

public class CatalogProductPriceResolver : IProductPriceResolver
{
    private readonly CatalogDbContext _context;

    public CatalogProductPriceResolver(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<decimal?> GetPriceAsync(string productId, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => p.Price)
            .FirstOrDefaultAsync(ct);
    }
}
