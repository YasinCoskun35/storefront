using Storefront.Modules.Identity.Core.Domain.Enums;

namespace Storefront.Modules.Identity.Core.Domain.Entities;

public class PartnerUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Company
    public string PartnerCompanyId { get; set; } = string.Empty;
    public virtual PartnerCompany Company { get; set; } = null!;
    
    // User Information
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Role
    public PartnerRole Role { get; set; } = PartnerRole.User;

    // Scopes (comma-separated list of PartnerScope constants)
    public string? Scopes { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;
    
    // Security
    public int AccessFailedCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Helper Properties
    public string FullName => $"{FirstName} {LastName}";
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    public bool HasScope(string scope) => Scopes?.Split(',').Contains(scope) ?? false;
    public List<string> GetScopesList() => Scopes?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];
}
