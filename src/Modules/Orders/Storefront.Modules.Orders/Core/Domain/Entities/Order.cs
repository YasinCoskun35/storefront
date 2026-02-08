using Storefront.Modules.Orders.Core.Domain.Enums;

namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Order Number (human-readable)
    public string OrderNumber { get; set; } = string.Empty; // e.g., "ORD-2024-0001"
    
    // Partner Information
    public string PartnerCompanyId { get; set; } = string.Empty;
    public string PartnerUserId { get; set; } = string.Empty; // Who created the order
    public string PartnerCompanyName { get; set; } = string.Empty; // Denormalized for performance
    
    // Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Pricing (B2B - set by admin after review)
    public decimal? SubTotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; } = "USD";
    
    // Delivery Information
    public string DeliveryAddress { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryState { get; set; } = string.Empty;
    public string DeliveryPostalCode { get; set; } = string.Empty;
    public string DeliveryCountry { get; set; } = string.Empty;
    public string? DeliveryNotes { get; set; }
    
    // Dates
    public DateTime? RequestedDeliveryDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    
    // Additional Information
    public string? Notes { get; set; } // Partner's notes
    public string? InternalNotes { get; set; } // Admin's internal notes
    
    // Tracking
    public string? TrackingNumber { get; set; }
    public string? ShippingProvider { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderComment> Comments { get; set; } = new List<OrderComment>();
}
