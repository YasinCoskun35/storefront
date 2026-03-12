namespace Storefront.SharedKernel;

public interface IPartnerDiscountResolver
{
    Task<decimal> GetDiscountRateAsync(string partnerCompanyId, CancellationToken ct = default);
}
