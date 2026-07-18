using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.Modules.Orders.Infrastructure.Persistence;

namespace Storefront.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabasesAsync(this IServiceProvider serviceProvider)
    {
        using var scope  = serviceProvider.CreateScope();
        var services     = scope.ServiceProvider;
        var logger       = services.GetRequiredService<ILogger<Program>>();

        const int maxRetries        = 5;
        const int delayMilliseconds = 2000;

        for (int retry = 1; retry <= maxRetries; retry++)
        {
            try
            {
                logger.LogInformation("Database initialization attempt {Retry}/{Max}…", retry, maxRetries);

                var identityDb = services.GetRequiredService<IdentityDbContext>();
                var catalogDb  = services.GetRequiredService<CatalogDbContext>();
                var contentDb  = services.GetRequiredService<ContentDbContext>();
                var ordersDb   = services.GetRequiredService<OrdersDbContext>();

                // ── Schemas must exist before migrations run ────────────────────
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS identity;");
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS catalog;");
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS content;");
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS orders;");
                logger.LogInformation("Schemas verified");

                // ── Run EF migrations for each module ───────────────────────────
                // MigrateAsync is idempotent: only applies migrations not yet in
                // the module-scoped __EFMigrationsHistory_* table.
                await identityDb.Database.MigrateAsync();
                logger.LogInformation("Identity migrations applied");

                await catalogDb.Database.MigrateAsync();
                logger.LogInformation("Catalog migrations applied");

                await contentDb.Database.MigrateAsync();
                logger.LogInformation("Content migrations applied");

                await ordersDb.Database.MigrateAsync();
                logger.LogInformation("Orders migrations applied");

                // ── Additive patches for columns added after InitialCreate ──────
                // Safe to run on every startup — IF NOT EXISTS makes them no-ops.
                await ApplyAdditivePatches(identityDb, catalogDb, ordersDb, logger);

                logger.LogInformation("Database initialization complete");

                await SeedDefaultSettingsAsync(contentDb, logger);
                return;
            }
            catch (NpgsqlException ex) when (retry < maxRetries)
            {
                logger.LogWarning(ex,
                    "Database connection failed (attempt {Retry}/{Max}). Retrying in {Delay}ms…",
                    retry, maxRetries, delayMilliseconds);
                await Task.Delay(delayMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed on attempt {Retry}/{Max}", retry, maxRetries);
                if (retry == maxRetries)
                {
                    logger.LogCritical("Max retries reached — is PostgreSQL running? (docker-compose up -d)");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Columns added after the InitialCreate migration. These ALTER TABLE statements
    /// are idempotent (IF NOT EXISTS) so they are safe to run on every startup.
    /// When a proper follow-up migration is generated for a new column, remove
    /// the corresponding line from here.
    /// </summary>
    private static async Task ApplyAdditivePatches(
        IdentityDbContext identityDb,
        CatalogDbContext catalogDb,
        OrdersDbContext ordersDb,
        ILogger logger)
    {
        logger.LogInformation("Applying additive column patches…");

        // Identity
        await identityDb.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE identity.""PartnerUsers"" ADD COLUMN IF NOT EXISTS ""PushToken"" varchar(500);

            ALTER TABLE identity.""PartnerCompanies"" ADD COLUMN IF NOT EXISTS ""DiscountRate""    decimal(5,2)  NOT NULL DEFAULT 0;
            ALTER TABLE identity.""PartnerCompanies"" ADD COLUMN IF NOT EXISTS ""CurrentBalance""  decimal(18,2) NOT NULL DEFAULT 0;

            CREATE TABLE IF NOT EXISTS identity.""PartnerAccountTransactions"" (
                ""Id""               varchar(450) PRIMARY KEY,
                ""PartnerCompanyId"" varchar(450) NOT NULL,
                ""Type""             int          NOT NULL,
                ""Amount""           decimal(18,2) NOT NULL,
                ""PaymentMethod""    int,
                ""OrderReference""   varchar(200),
                ""Notes""            varchar(1000),
                ""CreatedBy""        varchar(450) NOT NULL,
                ""CreatedAt""        timestamp    NOT NULL DEFAULT now(),
                CONSTRAINT ""FK_PAT_Company"" FOREIGN KEY (""PartnerCompanyId"")
                    REFERENCES identity.""PartnerCompanies""(""Id"") ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS ""IX_PAT_CompanyId"" ON identity.""PartnerAccountTransactions"" (""PartnerCompanyId"");
            CREATE INDEX IF NOT EXISTS ""IX_PAT_CreatedAt"" ON identity.""PartnerAccountTransactions"" (""CreatedAt"");

            CREATE TABLE IF NOT EXISTS identity.""PartnerPayments"" (
                ""Id""                  varchar(450) PRIMARY KEY,
                ""PartnerCompanyId""    varchar(450) NOT NULL,
                ""PartnerUserId""       varchar(450) NOT NULL,
                ""IyzicoToken""         varchar(200) NOT NULL,
                ""ConversationId""      varchar(200) NOT NULL,
                ""Amount""              decimal(18,2) NOT NULL,
                ""Status""              varchar(20)  NOT NULL DEFAULT 'Pending',
                ""CheckoutFormContent"" text         NOT NULL,
                ""IyzicoPaymentId""     varchar(200),
                ""FailureReason""       varchar(1000),
                ""CreatedAt""           timestamp    NOT NULL DEFAULT now(),
                ""CompletedAt""         timestamp
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PartnerPayments_IyzicoToken""      ON identity.""PartnerPayments"" (""IyzicoToken"");
            CREATE INDEX        IF NOT EXISTS ""IX_PartnerPayments_PartnerCompanyId"" ON identity.""PartnerPayments"" (""PartnerCompanyId"");
        ");

        // Catalog
        await catalogDb.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE catalog.""Categories"" ADD COLUMN IF NOT EXISTS ""ShowInNavbar"" boolean NOT NULL DEFAULT false;

            CREATE TABLE IF NOT EXISTS catalog.""VariantGroups"" (
                ""Id""           varchar(450) PRIMARY KEY,
                ""Name""         varchar(200) NOT NULL,
                ""Description""  varchar(2000) NOT NULL DEFAULT '',
                ""DisplayType""  varchar(50)  NOT NULL DEFAULT 'Swatch',
                ""IsRequired""   boolean      NOT NULL DEFAULT true,
                ""AllowMultiple"" boolean     NOT NULL DEFAULT false,
                ""DisplayOrder"" int          NOT NULL DEFAULT 0,
                ""IsActive""     boolean      NOT NULL DEFAULT true,
                ""CreatedAt""    timestamp    NOT NULL DEFAULT now(),
                ""UpdatedAt""    timestamp
            );
            CREATE INDEX IF NOT EXISTS ""IX_VariantGroups_IsActive""     ON catalog.""VariantGroups"" (""IsActive"");
            CREATE INDEX IF NOT EXISTS ""IX_VariantGroups_DisplayOrder""  ON catalog.""VariantGroups"" (""DisplayOrder"");

            CREATE TABLE IF NOT EXISTS catalog.""VariantOptions"" (
                ""Id""              varchar(450) PRIMARY KEY,
                ""VariantGroupId""  varchar(450) NOT NULL,
                ""Name""            varchar(200) NOT NULL,
                ""Code""            varchar(100) NOT NULL,
                ""HexColor""        varchar(10),
                ""ImageUrl""        varchar(1000),
                ""PriceAdjustment"" decimal(18,2),
                ""IsAvailable""     boolean      NOT NULL DEFAULT true,
                ""DisplayOrder""    int          NOT NULL DEFAULT 0,
                ""CreatedAt""       timestamp    NOT NULL DEFAULT now(),
                CONSTRAINT ""FK_VariantOptions_VariantGroup"" FOREIGN KEY (""VariantGroupId"")
                    REFERENCES catalog.""VariantGroups""(""Id"") ON DELETE CASCADE
            );
            CREATE INDEX        IF NOT EXISTS ""IX_VariantOptions_VariantGroupId""   ON catalog.""VariantOptions"" (""VariantGroupId"");
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_VariantOptions_GroupId_Code""      ON catalog.""VariantOptions"" (""VariantGroupId"", ""Code"");

            CREATE TABLE IF NOT EXISTS catalog.""ProductVariantGroups"" (
                ""Id""             varchar(450) PRIMARY KEY,
                ""ProductId""      varchar(450) NOT NULL,
                ""VariantGroupId"" varchar(450) NOT NULL,
                ""IsRequired""     boolean      NOT NULL DEFAULT true,
                ""DisplayOrder""   int          NOT NULL DEFAULT 0,
                ""CreatedAt""      timestamp    NOT NULL DEFAULT now(),
                CONSTRAINT ""FK_ProductVariantGroups_VariantGroup"" FOREIGN KEY (""VariantGroupId"")
                    REFERENCES catalog.""VariantGroups""(""Id"") ON DELETE CASCADE
            );
            CREATE INDEX        IF NOT EXISTS ""IX_ProductVariantGroups_ProductId""       ON catalog.""ProductVariantGroups"" (""ProductId"");
            CREATE INDEX        IF NOT EXISTS ""IX_ProductVariantGroups_VariantGroupId""  ON catalog.""ProductVariantGroups"" (""VariantGroupId"");
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ProductVariantGroups_ProductId_GroupId"" ON catalog.""ProductVariantGroups"" (""ProductId"", ""VariantGroupId"");
        ");

        // Orders
        await ordersDb.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE orders.""CartItems"" ADD COLUMN IF NOT EXISTS ""UnitPrice"" decimal(18,2);

            CREATE TABLE IF NOT EXISTS orders.""SavedAddresses"" (
                ""Id""               varchar(450) PRIMARY KEY,
                ""PartnerUserId""    varchar(450) NOT NULL,
                ""PartnerCompanyId"" varchar(450) NOT NULL,
                ""Label""            varchar(100) NOT NULL,
                ""Address""          varchar(500) NOT NULL,
                ""City""             varchar(100) NOT NULL,
                ""State""            varchar(100) NOT NULL,
                ""PostalCode""       varchar(20)  NOT NULL,
                ""Country""          varchar(100) NOT NULL,
                ""IsDefault""        boolean      NOT NULL DEFAULT false,
                ""CreatedAt""        timestamp    NOT NULL DEFAULT now(),
                ""UpdatedAt""        timestamp
            );
            CREATE INDEX IF NOT EXISTS ""IX_SavedAddresses_PartnerUserId"" ON orders.""SavedAddresses"" (""PartnerUserId"");
        ");

        logger.LogInformation("Additive patches applied");
    }

    private static async Task SeedDefaultSettingsAsync(ContentDbContext contentDb, ILogger logger)
    {
        var existing = await contentDb.AppSettings.CountAsync();
        if (existing > 0)
        {
            logger.LogInformation("App settings already seeded ({Count} entries), skipping", existing);
            return;
        }

        logger.LogInformation("Seeding default application settings…");

        var defaults = new[]
        {
            ("Features.Blog.Enabled",             "false", "Enable Blog",             "Show/hide blog section",                                                                        "Features", "boolean"),
            ("Features.Pricing.Enabled",          "false", "Enable Pricing",          "Show/hide product prices (B2B quote-to-order mode when false)",                                  "Features", "boolean"),
            ("Features.PublicCatalog.Enabled",    "true",  "Enable Public Catalog",   "Allow public users to browse products",                                                          "Features", "boolean"),
            ("Features.PublicStorefront.Enabled", "false", "Enable Public Storefront","Show public website. When disabled, visitors are redirected to Partner login.",                  "Features", "boolean"),
            ("Site.Name",                         "Storefront", "Site Name",          "Name of the website",                                                                            "General",  "string"),
            ("Site.ContactEmail",                 "contact@storefront.com", "Contact Email", "Primary contact email address",                                                           "General",  "string"),
            ("Site.MaintenanceMode",              "false", "Maintenance Mode",        "Put site in maintenance mode",                                                                   "General",  "boolean"),
        };

        foreach (var (key, value, displayName, description, category, dataType) in defaults)
        {
            await contentDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO content.""AppSettings"" (""Key"",""Value"",""DisplayName"",""Description"",""Category"",""DataType"",""UpdatedAt"")
                VALUES ('{key}','{value}','{displayName}','{description}','{category}','{dataType}', NOW())
                ON CONFLICT (""Key"") DO NOTHING;
            ");
        }

        logger.LogInformation("Seeded {Count} default settings", defaults.Length);
    }
}
