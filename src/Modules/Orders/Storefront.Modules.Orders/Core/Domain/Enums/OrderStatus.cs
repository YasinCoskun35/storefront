namespace Storefront.Modules.Orders.Core.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,          // Cart not submitted yet
    Pending = 1,        // Order submitted, awaiting review
    QuoteSent = 2,      // Admin sent quote/price
    Confirmed = 3,      // Partner confirmed the quote
    Preparing = 4,      // Order being prepared
    QualityCheck = 5,   // Quality control
    ReadyToShip = 6,    // Ready for shipping
    Shipping = 7,       // In transit
    Delivered = 8,      // Delivered to partner
    Cancelled = 9,      // Order cancelled
    Rejected = 10,      // Quote rejected
    PendingPayment = 11 // Approved but awaiting payment
}
