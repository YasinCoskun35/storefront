namespace Storefront.Modules.Identity.Core.Domain.Entities;

public class PartnerContact
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Company
    public string PartnerCompanyId { get; set; } = string.Empty;
    public virtual PartnerCompany Company { get; set; } = null!;
    
    // Contact Information
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    // Flags
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
