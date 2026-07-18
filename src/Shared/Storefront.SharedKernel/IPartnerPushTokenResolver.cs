namespace Storefront.SharedKernel;

public interface IPartnerPushTokenResolver
{
    /// <summary>
    /// Returns the Expo push token for a partner user, or null if none is registered.
    /// </summary>
    Task<string?> GetPushTokenAsync(string partnerUserId, CancellationToken ct = default);
}
