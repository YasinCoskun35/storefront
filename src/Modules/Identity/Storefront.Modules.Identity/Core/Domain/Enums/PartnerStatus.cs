namespace Storefront.Modules.Identity.Core.Domain.Enums;

public enum PartnerStatus
{
    Pending = 0,      // Awaiting admin approval
    Active = 1,       // Approved and active
    Suspended = 2,    // Temporarily disabled
    Rejected = 3      // Registration rejected
}
