namespace Storefront.Modules.Identity.Core.Domain.Entities;

public class PartnerPayment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PartnerCompanyId { get; set; } = string.Empty;
    public string PartnerUserId { get; set; } = string.Empty;
    public string IyzicoToken { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // Pending | Success | Failed
    public string Status { get; set; } = "Pending";

    // HTML/JS form content returned by iyzico (needed to serve the payment form)
    public string CheckoutFormContent { get; set; } = string.Empty;

    public string? IyzicoPaymentId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
