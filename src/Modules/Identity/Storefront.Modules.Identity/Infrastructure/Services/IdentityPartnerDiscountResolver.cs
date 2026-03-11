using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Infrastructure.Services;

public class IdentityPartnerDiscountResolver : IPartnerDiscountResolver
{
    private readonly IdentityDbContext _context;

    public IdentityPartnerDiscountResolver(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetDiscountRateAsync(string partnerCompanyId, CancellationToken ct = default)
    {
        var company = await _context.PartnerCompanies
            .AsNoTracking()
            .Where(pc => pc.Id == partnerCompanyId)
            .Select(pc => (decimal?)pc.DiscountRate)
            .FirstOrDefaultAsync(ct);

        return company ?? 0m;
    }
}
