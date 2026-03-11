using System.Threading.Channels;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storefront.Modules.Catalog.Core.Application.Commands;
using Storefront.Modules.Catalog.Core.Application.Interfaces;
using Storefront.Modules.Catalog.Core.Application.Settings;
using Storefront.Modules.Catalog.Infrastructure.BackgroundJobs;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.Modules.Catalog.Infrastructure.Services;
namespace Storefront.Modules.Catalog;

public static class CatalogModuleExtensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register Catalog Settings
        services.Configure<CatalogSettings>(
            configuration.GetSection(CatalogSettings.SectionName));
        
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CatalogSettings>>();
            return options.Value;
        });

        // Register CatalogDbContext with schema isolation and custom migration history table
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Catalog", "catalog")));

        // Register Channel for image processing
        services.AddSingleton(Channel.CreateUnbounded<ImageUploadMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        }));

        // Register services
        services.AddScoped<IImageUploadService, ImageUploadService>();
        services.AddScoped<Storefront.SharedKernel.IProductPriceResolver, CatalogProductPriceResolver>();
        // Register background service
        services.AddHostedService<ImageProcessingBackgroundService>();

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);

        return services;
    }
}

