using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storefront.Modules.Orders.Core.Application.Commands;
using Storefront.Modules.Orders.Infrastructure.Persistence;

namespace Storefront.Modules.Orders;

public static class OrdersModuleExtensions
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register OrdersDbContext with schema isolation
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Orders", "orders")));

        // Register MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AddToCartCommand).Assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(AddToCartCommandValidator).Assembly);

        return services;
    }
}
