namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class Cart
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? PartnerUserId { get; set; }
    public string? PartnerCompanyId { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
