using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Infrastructure.Services;

public class IdentityPartnerPushTokenResolver : IPartnerPushTokenResolver
{
    private readonly IdentityDbContext _context;

    public IdentityPartnerPushTokenResolver(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetPushTokenAsync(string partnerUserId, CancellationToken ct = default)
    {
        return await _context.PartnerUsers
            .AsNoTracking()
            .Where(u => u.Id == partnerUserId)
            .Select(u => u.PushToken)
            .FirstOrDefaultAsync(ct);
    }
}
