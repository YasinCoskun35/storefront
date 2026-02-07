using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storefront.Modules.Content.Core.Application.Commands;
using Storefront.Modules.Content.Core.Application.Interfaces;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.Modules.Content.Infrastructure.Services;

namespace Storefront.Modules.Content;

public static class ContentModuleExtensions
{
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register ContentDbContext with schema isolation and custom migration history table
        services.AddDbContext<ContentDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Content", "content")));

        // Register services
        services.AddScoped<ISlugService, SlugService>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBlogPostCommand).Assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(CreateBlogPostCommandValidator).Assembly);

        return services;
    }
}

