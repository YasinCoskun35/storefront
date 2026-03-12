namespace Storefront.Modules.Identity.Core.Domain.Enums;

public enum TransactionType
{
    OrderDebit = 0,       // Order charged to account
    PaymentCredit = 1,    // Payment received (cash, check, etc.)
    ManualAdjustment = 2  // Admin manual correction
}
