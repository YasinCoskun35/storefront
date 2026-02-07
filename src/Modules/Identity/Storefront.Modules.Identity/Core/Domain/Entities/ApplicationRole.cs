using Microsoft.AspNetCore.Identity;

namespace Storefront.Modules.Identity.Core.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

