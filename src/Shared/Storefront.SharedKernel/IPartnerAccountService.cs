namespace Storefront.SharedKernel;

public interface IPartnerAccountService
{
    /// <summary>
    /// Records an order debit against the partner's current account and
    /// decreases their CurrentBalance by the order total.
    /// </summary>
    Task RecordOrderDebitAsync(
        string partnerCompanyId,
        string orderNumber,
        decimal amount,
        string createdBy,
        CancellationToken ct = default);
}
