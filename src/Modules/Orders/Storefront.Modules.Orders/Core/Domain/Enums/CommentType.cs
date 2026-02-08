namespace Storefront.Modules.Orders.Core.Domain.Enums;

public enum CommentType
{
    General = 0,        // General comment
    StatusChange = 1,   // Status was changed
    Quote = 2,          // Quote/price information
    Payment = 3,        // Payment related
    Shipping = 4,       // Shipping information
    Internal = 5        // Internal note (not visible to partner)
}
