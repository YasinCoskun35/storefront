using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;

namespace Storefront.Modules.Identity.Infrastructure.Persistence;

public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PartnerCompany> PartnerCompanies => Set<PartnerCompany>();
    public DbSet<PartnerUser> PartnerUsers => Set<PartnerUser>();
    public DbSet<PartnerContact> PartnerContacts => Set<PartnerContact>();
    public DbSet<PartnerAccountTransaction> PartnerAccountTransactions => Set<PartnerAccountTransaction>();
    public DbSet<PartnerPayment> PartnerPayments => Set<PartnerPayment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set default schema for the Identity module
        builder.HasDefaultSchema("identity");

        // Configure Identity tables with custom names in the identity schema
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Configure RefreshToken entity
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Id).HasMaxLength(450);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.Property(rt => rt.UserId).IsRequired().HasMaxLength(450);

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
        });

        // Configure ApplicationUser additional properties
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.IsActive).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        // Configure ApplicationRole additional properties
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.Property(r => r.CreatedAt).IsRequired();
        });

        // Configure PartnerCompany
        builder.Entity<PartnerCompany>(entity =>
        {
            entity.ToTable("PartnerCompanies");
            entity.HasKey(pc => pc.Id);
            entity.Property(pc => pc.Id).HasMaxLength(450);
            
            entity.Property(pc => pc.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(pc => pc.TaxId).IsRequired().HasMaxLength(50);
            entity.Property(pc => pc.Email).IsRequired().HasMaxLength(100);
            entity.Property(pc => pc.Phone).IsRequired().HasMaxLength(20);
            
            entity.Property(pc => pc.Address).IsRequired().HasMaxLength(500);
            entity.Property(pc => pc.City).IsRequired().HasMaxLength(100);
            entity.Property(pc => pc.State).IsRequired().HasMaxLength(100);
            entity.Property(pc => pc.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(pc => pc.Country).IsRequired().HasMaxLength(100);
            
            entity.Property(pc => pc.Industry).HasMaxLength(100);
            entity.Property(pc => pc.Website).HasMaxLength(200);
            entity.Property(pc => pc.Notes).HasMaxLength(2000);
            entity.Property(pc => pc.ApprovedBy).HasMaxLength(450);
            entity.Property(pc => pc.ApprovalNotes).HasMaxLength(1000);
            
            entity.Property(pc => pc.Status).IsRequired();
            entity.Property(pc => pc.CreatedAt).IsRequired();

            entity.Property(pc => pc.DiscountRate).HasColumnType("decimal(5,2)");
            entity.Property(pc => pc.CurrentBalance).HasColumnType("decimal(18,2)");

            entity.HasIndex(pc => pc.TaxId).IsUnique();
            entity.HasIndex(pc => pc.Email);
            entity.HasIndex(pc => pc.Status);
        });

        // Configure PartnerUser
        builder.Entity<PartnerUser>(entity =>
        {
            entity.ToTable("PartnerUsers");
            entity.HasKey(pu => pu.Id);
            entity.Property(pu => pu.Id).HasMaxLength(450);
            
            entity.Property(pu => pu.PartnerCompanyId).IsRequired().HasMaxLength(450);
            entity.Property(pu => pu.Email).IsRequired().HasMaxLength(100);
            entity.Property(pu => pu.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(pu => pu.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(pu => pu.LastName).IsRequired().HasMaxLength(100);
            entity.Property(pu => pu.Phone).HasMaxLength(20);
            entity.Property(pu => pu.Scopes).HasMaxLength(500);

            entity.Property(pu => pu.Role).IsRequired();
            entity.Property(pu => pu.IsActive).IsRequired();
            entity.Property(pu => pu.EmailConfirmed).IsRequired();
            entity.Property(pu => pu.CreatedAt).IsRequired();
            
            entity.HasOne(pu => pu.Company)
                .WithMany(pc => pc.Users)
                .HasForeignKey(pu => pu.PartnerCompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(pu => pu.Email).IsUnique();
            entity.HasIndex(pu => pu.PartnerCompanyId);
            entity.HasIndex(pu => new { pu.Email, pu.PartnerCompanyId });
        });

        // Configure PartnerContact
        builder.Entity<PartnerContact>(entity =>
        {
            entity.ToTable("PartnerContacts");
            entity.HasKey(pc => pc.Id);
            entity.Property(pc => pc.Id).HasMaxLength(450);
            
            entity.Property(pc => pc.PartnerCompanyId).IsRequired().HasMaxLength(450);
            entity.Property(pc => pc.Name).IsRequired().HasMaxLength(100);
            entity.Property(pc => pc.Title).IsRequired().HasMaxLength(100);
            entity.Property(pc => pc.Email).IsRequired().HasMaxLength(100);
            entity.Property(pc => pc.Phone).IsRequired().HasMaxLength(20);
            
            entity.Property(pc => pc.IsPrimary).IsRequired();
            entity.Property(pc => pc.IsActive).IsRequired();
            entity.Property(pc => pc.CreatedAt).IsRequired();
            
            entity.HasOne(pc => pc.Company)
                .WithMany(c => c.Contacts)
                .HasForeignKey(pc => pc.PartnerCompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(pc => pc.PartnerCompanyId);
            entity.HasIndex(pc => pc.Email);
        });

        // Configure PartnerAccountTransaction
        builder.Entity<PartnerAccountTransaction>(entity =>
        {
            entity.ToTable("PartnerAccountTransactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasMaxLength(450);

            entity.Property(t => t.PartnerCompanyId).IsRequired().HasMaxLength(450);
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.OrderReference).HasMaxLength(200);
            entity.Property(t => t.Notes).HasMaxLength(1000);
            entity.Property(t => t.CreatedBy).IsRequired().HasMaxLength(450);

            entity.HasOne(t => t.Company)
                .WithMany(pc => pc.AccountTransactions)
                .HasForeignKey(t => t.PartnerCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => t.PartnerCompanyId);
            entity.HasIndex(t => t.CreatedAt);
        });

        // Configure PartnerPayment
        builder.Entity<PartnerPayment>(entity =>
        {
            entity.ToTable("PartnerPayments");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasMaxLength(450);
            entity.Property(p => p.PartnerCompanyId).IsRequired().HasMaxLength(450);
            entity.Property(p => p.PartnerUserId).IsRequired().HasMaxLength(450);
            entity.Property(p => p.IyzicoToken).IsRequired().HasMaxLength(200);
            entity.Property(p => p.ConversationId).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Status).IsRequired().HasMaxLength(20);
            entity.Property(p => p.CheckoutFormContent).IsRequired();
            entity.Property(p => p.IyzicoPaymentId).HasMaxLength(200);
            entity.Property(p => p.FailureReason).HasMaxLength(1000);

            entity.HasIndex(p => p.IyzicoToken).IsUnique();
            entity.HasIndex(p => p.PartnerCompanyId);
        });
    }
}

