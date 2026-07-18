namespace Storefront.SharedKernel;

public interface IExpoPushService
{
    /// <summary>
    /// Sends a push notification to a single Expo push token.
    /// Silently swaps invalid/expired tokens without throwing.
    /// </summary>
    Task SendAsync(string expoPushToken, string title, string body, object? data = null, CancellationToken ct = default);
}
