using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Core.Application.Interfaces;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.Modules.Identity.Infrastructure.Services;

namespace Storefront.Modules.Identity;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register IdentityDbContext with schema isolation and custom migration history table
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Identity", "identity")));

        // Register ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        // Register JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero,
                // Map JWT "sub" claim to NameIdentifier so User.FindFirst(ClaimTypes.NameIdentifier) works
                NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
        });

        // Register application services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IdentityDataSeeder>();

        // Register PasswordHasher for PartnerUser
        services.AddScoped<IPasswordHasher<PartnerUser>, PasswordHasher<PartnerUser>>();

        // Register partner discount resolver (used by Orders module via SharedKernel interface)
        services.AddScoped<Storefront.SharedKernel.IPartnerDiscountResolver, IdentityPartnerDiscountResolver>();

        // Register partner account service (used by Orders module via SharedKernel interface)
        services.AddScoped<Storefront.SharedKernel.IPartnerAccountService, IdentityPartnerAccountService>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginUserCommand).Assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(LoginUserCommandValidator).Assembly);

        return services;
    }

    public static async Task<IServiceProvider> SeedIdentityDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
        await seeder.SeedAsync();
        return serviceProvider;
    }
}

