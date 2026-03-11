using Storefront.Modules.Identity.Core.Domain.Enums;

namespace Storefront.Modules.Identity.Core.Domain.Entities;

public class PartnerAccountTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PartnerCompanyId { get; set; } = string.Empty;
    public virtual PartnerCompany Company { get; set; } = null!;

    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }               // Always positive
    public PaymentMethod? PaymentMethod { get; set; } // Only for PaymentCredit
    public string? OrderReference { get; set; }
    public string? Notes { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
