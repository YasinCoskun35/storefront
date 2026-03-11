using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Infrastructure.Services;

public class IdentityPartnerAccountService : IPartnerAccountService
{
    private readonly IdentityDbContext _context;

    public IdentityPartnerAccountService(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task RecordOrderDebitAsync(
        string partnerCompanyId,
        string orderNumber,
        decimal amount,
        string createdBy,
        CancellationToken ct = default)
    {
        var company = await _context.PartnerCompanies
            .FirstOrDefaultAsync(pc => pc.Id == partnerCompanyId, ct);

        if (company is null) return;

        // Prevent duplicate debit for the same order
        var alreadyRecorded = await _context.PartnerAccountTransactions
            .AnyAsync(t => t.PartnerCompanyId == partnerCompanyId
                        && t.OrderReference == orderNumber
                        && t.Type == TransactionType.OrderDebit, ct);

        if (alreadyRecorded) return;

        var transaction = new PartnerAccountTransaction
        {
            PartnerCompanyId = partnerCompanyId,
            Type = TransactionType.OrderDebit,
            Amount = amount,
            OrderReference = orderNumber,
            Notes = $"Sipariş onayı: {orderNumber}",
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };

        company.CurrentBalance += amount;

        _context.PartnerAccountTransactions.Add(transaction);
        await _context.SaveChangesAsync(ct);
    }
}
