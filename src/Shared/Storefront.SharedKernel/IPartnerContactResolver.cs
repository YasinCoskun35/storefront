namespace Storefront.SharedKernel;

/// <summary>
/// Resolves partner contact details across module boundaries
/// (implemented by Identity, consumed by Orders).
/// </summary>
public interface IPartnerContactResolver
{
    Task<string?> GetEmailAsync(string partnerUserId, CancellationToken ct = default);
}
