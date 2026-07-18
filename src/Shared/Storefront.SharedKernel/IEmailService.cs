namespace Storefront.SharedKernel;

/// <summary>
/// Sends transactional emails. Implementations must never throw for delivery
/// failures — callers use email as a side effect, not a critical path.
/// </summary>
public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken ct = default);
}
