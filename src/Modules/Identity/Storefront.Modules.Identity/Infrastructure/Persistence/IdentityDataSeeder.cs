using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Storefront.Modules.Identity.Core.Domain.Entities;

namespace Storefront.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDataSeeder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(IServiceProvider serviceProvider, ILogger<IdentityDataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed default admin user
        await SeedAdminUserAsync(userManager);
    }

    private async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[] { "Admin", "Manager", "User" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Description = $"{roleName} role",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role '{RoleName}': {Errors}", 
                        roleName, 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@storefront.com";
        const string adminPassword = "AdminPassword123!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            _logger.LogInformation("Admin user already exists.");
            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            _logger.LogInformation("Default admin user created successfully with email: {Email}", adminEmail);
        }
        else
        {
            _logger.LogError("Failed to create admin user: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

