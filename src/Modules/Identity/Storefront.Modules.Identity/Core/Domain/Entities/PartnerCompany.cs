using Storefront.Modules.Identity.Core.Domain.Enums;

namespace Storefront.Modules.Identity.Core.Domain.Entities;

public class PartnerCompany
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Company Information
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    // Address
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Business Details
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    
    // Notes
    public string? Notes { get; set; }
    
    // Status
    public PartnerStatus Status { get; set; } = PartnerStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }  // Admin user ID
    public string? ApprovalNotes { get; set; }
    
    // Timestamps
    public DateTime? UpdatedAt { get; set; }
    
    // Pricing Policy
    public decimal DiscountRate { get; set; } = 0;  // Percentage 0–100

    // Current Account (Cari Hesap)
    public decimal CurrentBalance { get; set; } = 0;  // Positive = partner owes money

    // Navigation Properties
    public virtual ICollection<PartnerUser> Users { get; set; } = new List<PartnerUser>();
    public virtual ICollection<PartnerContact> Contacts { get; set; } = new List<PartnerContact>();
    public virtual ICollection<PartnerAccountTransaction> AccountTransactions { get; set; } = new List<PartnerAccountTransaction>();
}
