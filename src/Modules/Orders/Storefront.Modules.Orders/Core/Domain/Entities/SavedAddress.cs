namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class SavedAddress
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PartnerUserId { get; set; } = string.Empty;
    public string PartnerCompanyId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
