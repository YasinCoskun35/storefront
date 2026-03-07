using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;

namespace Storefront.Modules.Orders.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderComment> OrderComments => Set<OrderComment>();
    public DbSet<SavedAddress> SavedAddresses => Set<SavedAddress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("orders");

        ConfigureCart(builder);
        ConfigureCartItem(builder);
        ConfigureOrder(builder);
        ConfigureOrderItem(builder);
        ConfigureOrderComment(builder);
        ConfigureSavedAddress(builder);
    }

    private void ConfigureCart(ModelBuilder builder)
    {
        builder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasMaxLength(450);

            entity.Property(c => c.PartnerUserId).HasMaxLength(450);
            entity.Property(c => c.PartnerCompanyId).HasMaxLength(450);

            entity.HasIndex(c => c.PartnerUserId).IsUnique().HasFilter("\"PartnerUserId\" IS NOT NULL");
            entity.HasIndex(c => c.PartnerCompanyId);
        });
    }

    private void ConfigureCartItem(ModelBuilder builder)
    {
        builder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Id).HasMaxLength(450);

            entity.Property(ci => ci.CartId).IsRequired().HasMaxLength(450);
            entity.Property(ci => ci.ProductId).IsRequired().HasMaxLength(450);
            entity.Property(ci => ci.ProductName).IsRequired().HasMaxLength(500);
            entity.Property(ci => ci.ProductSKU).IsRequired().HasMaxLength(100);
            entity.Property(ci => ci.ProductImageUrl).HasMaxLength(1000);
            entity.Property(ci => ci.SelectedVariants).HasColumnType("text");
            entity.Property(ci => ci.CustomizationNotes).HasMaxLength(2000);

            entity.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ci => ci.CartId);
            entity.HasIndex(ci => ci.ProductId);
        });
    }

    private void ConfigureOrder(ModelBuilder builder)
    {
        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasMaxLength(450);

            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(o => o.PartnerCompanyId).HasMaxLength(450);
            entity.Property(o => o.PartnerUserId).HasMaxLength(450);
            entity.Property(o => o.PartnerCompanyName).HasMaxLength(200);

            entity.Property(o => o.Status).IsRequired();

            entity.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(o => o.Discount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.Currency).HasMaxLength(10);

            entity.Property(o => o.DeliveryAddress).IsRequired().HasMaxLength(500);
            entity.Property(o => o.DeliveryCity).IsRequired().HasMaxLength(100);
            entity.Property(o => o.DeliveryState).IsRequired().HasMaxLength(100);
            entity.Property(o => o.DeliveryPostalCode).IsRequired().HasMaxLength(20);
            entity.Property(o => o.DeliveryCountry).IsRequired().HasMaxLength(100);
            entity.Property(o => o.DeliveryNotes).HasMaxLength(2000);

            entity.Property(o => o.Notes).HasMaxLength(5000);
            entity.Property(o => o.InternalNotes).HasMaxLength(5000);
            entity.Property(o => o.TrackingNumber).HasMaxLength(200);
            entity.Property(o => o.ShippingProvider).HasMaxLength(100);

            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.PartnerCompanyId);
            entity.HasIndex(o => o.PartnerUserId);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.CreatedAt);
        });
    }

    private void ConfigureOrderItem(ModelBuilder builder)
    {
        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.Id).HasMaxLength(450);

            entity.Property(oi => oi.OrderId).IsRequired().HasMaxLength(450);
            entity.Property(oi => oi.ProductId).IsRequired().HasMaxLength(450);
            entity.Property(oi => oi.ProductName).IsRequired().HasMaxLength(500);
            entity.Property(oi => oi.ProductSKU).IsRequired().HasMaxLength(100);
            entity.Property(oi => oi.ProductImageUrl).HasMaxLength(1000);
            entity.Property(oi => oi.SelectedVariants).HasColumnType("text");

            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.Discount).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(oi => oi.CustomizationNotes).HasMaxLength(2000);

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(oi => oi.OrderId);
            entity.HasIndex(oi => oi.ProductId);
        });
    }

    private void ConfigureOrderComment(ModelBuilder builder)
    {
        builder.Entity<OrderComment>(entity =>
        {
            entity.ToTable("OrderComments");
            entity.HasKey(oc => oc.Id);
            entity.Property(oc => oc.Id).HasMaxLength(450);

            entity.Property(oc => oc.OrderId).IsRequired().HasMaxLength(450);
            entity.Property(oc => oc.Content).IsRequired().HasMaxLength(5000);
            entity.Property(oc => oc.Type).IsRequired();
            entity.Property(oc => oc.AuthorId).IsRequired().HasMaxLength(450);
            entity.Property(oc => oc.AuthorName).IsRequired().HasMaxLength(200);
            entity.Property(oc => oc.AuthorType).IsRequired().HasMaxLength(50);
            entity.Property(oc => oc.AttachmentUrl).HasMaxLength(1000);
            entity.Property(oc => oc.AttachmentFileName).HasMaxLength(500);

            entity.HasOne(oc => oc.Order)
                .WithMany(o => o.Comments)
                .HasForeignKey(oc => oc.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(oc => oc.OrderId);
            entity.HasIndex(oc => oc.CreatedAt);
        });
    }

    private void ConfigureSavedAddress(ModelBuilder builder)
    {
        builder.Entity<SavedAddress>(entity =>
        {
            entity.ToTable("SavedAddresses");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasMaxLength(450);

            entity.Property(a => a.PartnerUserId).IsRequired().HasMaxLength(450);
            entity.Property(a => a.PartnerCompanyId).IsRequired().HasMaxLength(450);
            entity.Property(a => a.Label).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Address).IsRequired().HasMaxLength(500);
            entity.Property(a => a.City).IsRequired().HasMaxLength(100);
            entity.Property(a => a.State).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(a => a.Country).IsRequired().HasMaxLength(100);

            entity.HasIndex(a => a.PartnerUserId);
        });
    }
}
