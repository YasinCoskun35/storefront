using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
            .WithPortBinding(5433, 5432)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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

            // Build the service provider and apply migrations
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            ApplyMigrations(scopedServices);
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

    private static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        try
        {
            var identityContext = serviceProvider.GetRequiredService<IdentityDbContext>();
            identityContext.Database.EnsureCreated();

            var catalogContext = serviceProvider.GetRequiredService<CatalogDbContext>();
            catalogContext.Database.EnsureCreated();

            var contentContext = serviceProvider.GetRequiredService<ContentDbContext>();
            contentContext.Database.EnsureCreated();

            var ordersContext = serviceProvider.GetRequiredService<OrdersDbContext>();
            ordersContext.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            // Log or handle migration errors
            Console.WriteLine($"Migration error: {ex.Message}");
            throw;
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

