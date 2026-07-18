using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Storefront.Modules.Catalog.Core.Application.Interfaces;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace Storefront.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public CustomWebApplicationFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("storefront_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build(); // random host port — avoids conflict with dev Postgres on 5432/5433
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Inject test-only config values so the app can boot without user-secrets
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]   = "test-integration-secret-key-that-is-at-least-32-chars",
                ["Jwt:Issuer"]   = "StorefrontTest",
                ["Jwt:Audience"] = "StorefrontTest",
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Cookie:SameSite"]     = "Lax",
                ["Cookie:SecurePolicy"] = "None",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            RemoveDbContexts(services);

            // Add DbContexts with test container connection string
            var connectionString = _dbContainer.GetConnectionString();

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(connectionString, x =>
                    x.MigrationsHistoryTable("__EFMigrationsHistory_Identity", "identity")));

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(connectionString, x =>
                    x.MigrationsHistoryTable("__EFMigrationsHistory_Catalog", "catalog")));

            services.AddDbContext<ContentDbContext>(options =>
                options.UseNpgsql(connectionString, x =>
                    x.MigrationsHistoryTable("__EFMigrationsHistory_Content", "content")));

            services.AddDbContext<OrdersDbContext>(options =>
                options.UseNpgsql(connectionString, x =>
                    x.MigrationsHistoryTable("__EFMigrationsHistory_Orders", "orders")));

            // Mock IImageUploadService to avoid file system operations during tests
            var mockImageUploadService = Substitute.For<IImageUploadService>();
            mockImageUploadService.QueueImageForProcessingAsync(
                Arg.Any<string>(),
                Arg.Any<IFormFile>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult("mock-image-path.jpg"));

            services.RemoveAll<IImageUploadService>();
            services.AddSingleton(mockImageUploadService);

        });

        builder.UseEnvironment("Testing");
    }


    private static void RemoveDbContexts(IServiceCollection services)
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<IdentityDbContext>) ||
            d.ServiceType == typeof(DbContextOptions<CatalogDbContext>) ||
            d.ServiceType == typeof(DbContextOptions<ContentDbContext>) ||
            d.ServiceType == typeof(DbContextOptions<OrdersDbContext>))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        try { return base.CreateHost(builder); }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Test host startup failed — {ex.GetType().Name}: {ex.Message}", ex);
        }
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}

