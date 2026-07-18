using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Infrastructure.Services;

public class IdentityPartnerContactResolver : IPartnerContactResolver
{
    private readonly IdentityDbContext _context;

    public IdentityPartnerContactResolver(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetEmailAsync(string partnerUserId, CancellationToken ct = default)
    {
        return await _context.PartnerUsers
            .AsNoTracking()
            .Where(pu => pu.Id == partnerUserId && pu.IsActive)
            .Select(pu => pu.Email)
            .FirstOrDefaultAsync(ct);
    }
}
