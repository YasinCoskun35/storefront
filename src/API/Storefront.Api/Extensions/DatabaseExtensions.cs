using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Npgsql;

namespace Storefront.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabasesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        
        const int maxRetries = 5;
        const int delayMilliseconds = 2000;
        
        for (int retry = 1; retry <= maxRetries; retry++)
        {
            try
            {
                Console.WriteLine($"🔄 Attempting database initialization (attempt {retry}/{maxRetries})...");
                
                // STEP 1: Create all schemas first
                var identityDb = services.GetRequiredService<IdentityDbContext>();
                
                Console.WriteLine("📋 Creating database schemas...");
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS identity;");
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS catalog;");
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS content;");
                await identityDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS orders;");
                Console.WriteLine("✅ All schemas created");
                
                // STEP 2: Create Identity database and tables
                if (identityDb.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("📦 Applying Identity migrations...");
                    await identityDb.Database.MigrateAsync();
                }
                else
                {
                    await identityDb.Database.EnsureCreatedAsync();
                }
                
                var identityTableCount = await identityDb.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'identity'");
                Console.WriteLine($"✅ Identity schema initialized ({identityTableCount} objects)");
                
                // Create Catalog database and tables
                var catalogDb = services.GetRequiredService<CatalogDbContext>();
                if (catalogDb.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("📦 Applying Catalog migrations...");
                    await catalogDb.Database.MigrateAsync();
                }
                else
                {
                    Console.WriteLine("🔨 Creating Catalog schema and tables...");
                    await catalogDb.Database.EnsureCreatedAsync();
                }
                
                // Verify Catalog tables were created
                var catalogTableCount = await ExecuteScalarAsync<int>(catalogDb, 
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'catalog'");
                
                if (catalogTableCount == 0)
                {
                    Console.WriteLine("⚠️ Catalog tables not found, forcing creation...");
                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""Categories"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""Name"" varchar(200) NOT NULL,
                            ""Description"" varchar(2000),
                            ""Slug"" varchar(200),
                            ""ImageUrl"" varchar(500),
                            ""ParentId"" varchar(450),
                            ""DisplayOrder"" int NOT NULL DEFAULT 0,
                            ""IsActive"" boolean NOT NULL DEFAULT true,
                            ""ShowInNavbar"" boolean NOT NULL DEFAULT false,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp
                        );
                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Categories_Slug"" ON catalog.""Categories"" (""Slug"");
                        CREATE INDEX IF NOT EXISTS ""IX_Categories_ParentId"" ON catalog.""Categories"" (""ParentId"");
                        CREATE INDEX IF NOT EXISTS ""IX_Categories_IsActive"" ON catalog.""Categories"" (""IsActive"");
                    ");
                    
                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""Brands"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""Name"" varchar(200) NOT NULL,
                            ""Description"" varchar(2000),
                            ""LogoUrl"" varchar(500),
                            ""Website"" varchar(500),
                            ""IsActive"" boolean NOT NULL DEFAULT true,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp
                        );
                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Brands_Name"" ON catalog.""Brands"" (""Name"");
                    ");
                    
                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""Products"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""Name"" varchar(500) NOT NULL,
                            ""SKU"" varchar(100) NOT NULL,
                            ""Description"" varchar(5000),
                            ""ShortDescription"" varchar(500),
                            ""Slug"" varchar(500),
                            ""ProductType"" varchar(50) NOT NULL DEFAULT 'Simple',
                            ""Price"" numeric(18,2),
                            ""CompareAtPrice"" numeric(18,2),
                            ""Cost"" numeric(18,2),
                            ""BundlePrice"" numeric(18,2),
                            ""CanBeSoldSeparately"" boolean NOT NULL DEFAULT true,
                            ""CategoryId"" varchar(450),
                            ""BrandId"" varchar(450),
                            ""StockStatus"" varchar(50) NOT NULL,
                            ""StockQuantity"" int NOT NULL DEFAULT 0,
                            ""LowStockThreshold"" int,
                            ""Weight"" numeric(18,2),
                            ""Length"" numeric(18,2),
                            ""Width"" numeric(18,2),
                            ""Height"" numeric(18,2),
                            ""DimensionUnit"" varchar(10),
                            ""WeightUnit"" varchar(10),
                            ""MetaTitle"" varchar(200),
                            ""MetaDescription"" varchar(500),
                            ""IsActive"" boolean NOT NULL DEFAULT true,
                            ""IsFeatured"" boolean NOT NULL DEFAULT false,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp,
                            CONSTRAINT ""FK_Products_Categories"" FOREIGN KEY (""CategoryId"") REFERENCES catalog.""Categories""(""Id"") ON DELETE RESTRICT,
                            CONSTRAINT ""FK_Products_Brands"" FOREIGN KEY (""BrandId"") REFERENCES catalog.""Brands""(""Id"") ON DELETE SET NULL
                        );
                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Products_SKU"" ON catalog.""Products"" (""SKU"");
                        CREATE INDEX IF NOT EXISTS ""IX_Products_Slug"" ON catalog.""Products"" (""Slug"");
                        CREATE INDEX IF NOT EXISTS ""IX_Products_CategoryId"" ON catalog.""Products"" (""CategoryId"");
                        CREATE INDEX IF NOT EXISTS ""IX_Products_BrandId"" ON catalog.""Products"" (""BrandId"");
                        CREATE INDEX IF NOT EXISTS ""IX_Products_ProductType"" ON catalog.""Products"" (""ProductType"");
                        CREATE INDEX IF NOT EXISTS ""IX_Products_Name"" ON catalog.""Products"" USING gin (""Name"" gin_trgm_ops);
                    ");
                    
                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""ProductImages"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""ProductId"" varchar(450) NOT NULL,
                            ""Url"" varchar(1000) NOT NULL,
                            ""AltText"" varchar(200),
                            ""IsPrimary"" boolean NOT NULL DEFAULT false,
                            ""Type"" varchar(50),
                            ""DisplayOrder"" int NOT NULL DEFAULT 0,
                            ""FileSizeBytes"" bigint,
                            ""Width"" int,
                            ""Height"" int,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            CONSTRAINT ""FK_ProductImages_Products"" FOREIGN KEY (""ProductId"") REFERENCES catalog.""Products""(""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_ProductImages_ProductId"" ON catalog.""ProductImages"" (""ProductId"");
                    ");
                    
                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""ProductBundleItems"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""BundleProductId"" varchar(450) NOT NULL,
                            ""ComponentProductId"" varchar(450) NOT NULL,
                            ""Quantity"" int NOT NULL DEFAULT 1,
                            ""PriceOverride"" numeric(18,2),
                            ""IsOptional"" boolean NOT NULL DEFAULT false,
                            ""DisplayOrder"" int NOT NULL DEFAULT 0,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            CONSTRAINT ""FK_BundleItems_BundleProduct"" FOREIGN KEY (""BundleProductId"") REFERENCES catalog.""Products""(""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_BundleItems_ComponentProduct"" FOREIGN KEY (""ComponentProductId"") REFERENCES catalog.""Products""(""Id"") ON DELETE RESTRICT
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_BundleItems_BundleProductId"" ON catalog.""ProductBundleItems"" (""BundleProductId"");
                        CREATE INDEX IF NOT EXISTS ""IX_BundleItems_ComponentProductId"" ON catalog.""ProductBundleItems"" (""ComponentProductId"");
                        CREATE INDEX IF NOT EXISTS ""IX_BundleItems_BundleProduct_DisplayOrder"" ON catalog.""ProductBundleItems"" (""BundleProductId"", ""DisplayOrder"");
                    ");

                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""VariantGroups"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""Name"" varchar(200) NOT NULL,
                            ""Description"" varchar(2000) NOT NULL DEFAULT '',
                            ""DisplayType"" varchar(50) NOT NULL DEFAULT 'Swatch',
                            ""IsRequired"" boolean NOT NULL DEFAULT true,
                            ""AllowMultiple"" boolean NOT NULL DEFAULT false,
                            ""DisplayOrder"" int NOT NULL DEFAULT 0,
                            ""IsActive"" boolean NOT NULL DEFAULT true,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_VariantGroups_IsActive"" ON catalog.""VariantGroups"" (""IsActive"");
                        CREATE INDEX IF NOT EXISTS ""IX_VariantGroups_DisplayOrder"" ON catalog.""VariantGroups"" (""DisplayOrder"");
                    ");

                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""VariantOptions"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""VariantGroupId"" varchar(450) NOT NULL,
                            ""Name"" varchar(200) NOT NULL,
                            ""Code"" varchar(100) NOT NULL,
                            ""HexColor"" varchar(10),
                            ""ImageUrl"" varchar(1000),
                            ""PriceAdjustment"" decimal(18,2),
                            ""IsAvailable"" boolean NOT NULL DEFAULT true,
                            ""DisplayOrder"" int NOT NULL DEFAULT 0,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            CONSTRAINT ""FK_VariantOptions_VariantGroup"" FOREIGN KEY (""VariantGroupId"") REFERENCES catalog.""VariantGroups""(""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_VariantOptions_VariantGroupId"" ON catalog.""VariantOptions"" (""VariantGroupId"");
                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_VariantOptions_GroupId_Code"" ON catalog.""VariantOptions"" (""VariantGroupId"", ""Code"");
                    ");

                    await catalogDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS catalog.""ProductVariantGroups"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""ProductId"" varchar(450) NOT NULL,
                            ""VariantGroupId"" varchar(450) NOT NULL,
                            ""IsRequired"" boolean NOT NULL DEFAULT true,
                            ""DisplayOrder"" int NOT NULL DEFAULT 0,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            CONSTRAINT ""FK_ProductVariantGroups_VariantGroup"" FOREIGN KEY (""VariantGroupId"") REFERENCES catalog.""VariantGroups""(""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_ProductVariantGroups_ProductId"" ON catalog.""ProductVariantGroups"" (""ProductId"");
                        CREATE INDEX IF NOT EXISTS ""IX_ProductVariantGroups_VariantGroupId"" ON catalog.""ProductVariantGroups"" (""VariantGroupId"");
                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ProductVariantGroups_ProductId_GroupId"" ON catalog.""ProductVariantGroups"" (""ProductId"", ""VariantGroupId"");
                    ");

                    Console.WriteLine("✅ Catalog tables created manually");
                }
                else
                {
                    Console.WriteLine($"✅ Catalog schema initialized ({catalogTableCount} tables)");
                }

                // Ensure ShowInNavbar column exists (migration for existing DBs)
                await catalogDb.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE catalog.""Categories"" ADD COLUMN IF NOT EXISTS ""ShowInNavbar"" boolean NOT NULL DEFAULT false;
                ");

                // Ensure variant tables exist (added in Phase 1)
                await catalogDb.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS catalog.""VariantGroups"" (
                        ""Id"" varchar(450) PRIMARY KEY,
                        ""Name"" varchar(200) NOT NULL,
                        ""Description"" varchar(2000) NOT NULL DEFAULT '',
                        ""DisplayType"" varchar(50) NOT NULL DEFAULT 'Swatch',
                        ""IsRequired"" boolean NOT NULL DEFAULT true,
                        ""AllowMultiple"" boolean NOT NULL DEFAULT false,
                        ""DisplayOrder"" int NOT NULL DEFAULT 0,
                        ""IsActive"" boolean NOT NULL DEFAULT true,
                        ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                        ""UpdatedAt"" timestamp
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_VariantGroups_IsActive"" ON catalog.""VariantGroups"" (""IsActive"");
                    CREATE INDEX IF NOT EXISTS ""IX_VariantGroups_DisplayOrder"" ON catalog.""VariantGroups"" (""DisplayOrder"");

                    CREATE TABLE IF NOT EXISTS catalog.""VariantOptions"" (
                        ""Id"" varchar(450) PRIMARY KEY,
                        ""VariantGroupId"" varchar(450) NOT NULL,
                        ""Name"" varchar(200) NOT NULL,
                        ""Code"" varchar(100) NOT NULL,
                        ""HexColor"" varchar(10),
                        ""ImageUrl"" varchar(1000),
                        ""PriceAdjustment"" decimal(18,2),
                        ""IsAvailable"" boolean NOT NULL DEFAULT true,
                        ""DisplayOrder"" int NOT NULL DEFAULT 0,
                        ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                        CONSTRAINT ""FK_VariantOptions_VariantGroup"" FOREIGN KEY (""VariantGroupId"") REFERENCES catalog.""VariantGroups""(""Id"") ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_VariantOptions_VariantGroupId"" ON catalog.""VariantOptions"" (""VariantGroupId"");
                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_VariantOptions_GroupId_Code"" ON catalog.""VariantOptions"" (""VariantGroupId"", ""Code"");

                    CREATE TABLE IF NOT EXISTS catalog.""ProductVariantGroups"" (
                        ""Id"" varchar(450) PRIMARY KEY,
                        ""ProductId"" varchar(450) NOT NULL,
                        ""VariantGroupId"" varchar(450) NOT NULL,
                        ""IsRequired"" boolean NOT NULL DEFAULT true,
                        ""DisplayOrder"" int NOT NULL DEFAULT 0,
                        ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                        CONSTRAINT ""FK_ProductVariantGroups_VariantGroup"" FOREIGN KEY (""VariantGroupId"") REFERENCES catalog.""VariantGroups""(""Id"") ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_ProductVariantGroups_ProductId"" ON catalog.""ProductVariantGroups"" (""ProductId"");
                    CREATE INDEX IF NOT EXISTS ""IX_ProductVariantGroups_VariantGroupId"" ON catalog.""ProductVariantGroups"" (""VariantGroupId"");
                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ProductVariantGroups_ProductId_GroupId"" ON catalog.""ProductVariantGroups"" (""ProductId"", ""VariantGroupId"");
                ");

                // Seed catalog mock data (categories, brands, products) if empty
                await SeedCatalogDataAsync(catalogDb);
                
                // Create Content database and tables
                var contentDb = services.GetRequiredService<ContentDbContext>();
                if (contentDb.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("📦 Applying Content migrations...");
                    await contentDb.Database.MigrateAsync();
                }
                else
                {
                    Console.WriteLine("🔨 Creating Content schema and tables...");
                    await contentDb.Database.EnsureCreatedAsync();
                }
                
                var contentTableCount = await ExecuteScalarAsync<int>(contentDb,
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'content'");
                    
                if (contentTableCount < 3)  // Should have 3 tables: BlogPosts, StaticPages, AppSettings
                {
                    Console.WriteLine($"⚠️ Content tables incomplete ({contentTableCount}/3), creating missing tables...");
                    await contentDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS content.""BlogPosts"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""Title"" varchar(500) NOT NULL,
                            ""Slug"" varchar(500) NOT NULL,
                            ""Summary"" varchar(1000),
                            ""Body"" text NOT NULL,
                            ""FeaturedImage"" varchar(1000),
                            ""Author"" varchar(200),
                            ""Tags"" varchar(500),
                            ""Category"" varchar(200),
                            ""IsPublished"" boolean NOT NULL DEFAULT false,
                            ""PublishedAt"" timestamp,
                            ""SeoMetaTitle"" varchar(200),
                            ""SeoMetaDescription"" varchar(500),
                            ""SeoKeywords"" varchar(500),
                            ""SeoOgImage"" varchar(1000),
                            ""SeoOgType"" varchar(50),
                            ""SeoCanonicalUrl"" varchar(1000),
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp
                        );
                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_BlogPosts_Slug"" ON content.""BlogPosts"" (""Slug"");
                    ");
                    
                    await contentDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS content.""StaticPages"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""Title"" varchar(500) NOT NULL,
                            ""Slug"" varchar(500) NOT NULL,
                            ""Body"" text NOT NULL,
                            ""IsPublished"" boolean NOT NULL DEFAULT false,
                            ""DisplayOrder"" int NOT NULL DEFAULT 0,
                            ""SeoMetaTitle"" varchar(200),
                            ""SeoMetaDescription"" varchar(500),
                            ""SeoKeywords"" varchar(500),
                            ""SeoOgImage"" varchar(1000),
                            ""SeoOgType"" varchar(50),
                            ""SeoCanonicalUrl"" varchar(1000),
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp
                        );
                        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_StaticPages_Slug"" ON content.""StaticPages"" (""Slug"");
                    ");
                    
                    await contentDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS content.""AppSettings"" (
                            ""Key"" varchar(100) PRIMARY KEY,
                            ""Value"" varchar(2000) NOT NULL,
                            ""DisplayName"" varchar(200) NOT NULL,
                            ""Description"" varchar(500),
                            ""Category"" varchar(50) NOT NULL,
                            ""DataType"" varchar(20) NOT NULL,
                            ""UpdatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedBy"" varchar(450)
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_AppSettings_Category"" ON content.""AppSettings"" (""Category"");
                    ");
                    
                    Console.WriteLine("✅ Content tables created manually");
                }
                else
                {
                    Console.WriteLine($"✅ Content schema initialized ({contentTableCount} tables)");
                }

                // Ensure home slider tables exist (migration for existing DBs)
                await contentDb.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS content.""HeroSlides"" (
                        ""Id"" varchar(450) PRIMARY KEY,
                        ""Title"" varchar(500) NOT NULL,
                        ""Subtitle"" varchar(500),
                        ""ImageUrl"" varchar(1000) NOT NULL,
                        ""Link"" varchar(500) NOT NULL,
                        ""LinkText"" varchar(100) NOT NULL,
                        ""DisplayOrder"" int NOT NULL DEFAULT 0,
                        ""IsActive"" boolean NOT NULL DEFAULT true
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_HeroSlides_DisplayOrder"" ON content.""HeroSlides"" (""DisplayOrder"");

                    CREATE TABLE IF NOT EXISTS content.""HomeCategorySlides"" (
                        ""Id"" varchar(450) PRIMARY KEY,
                        ""Name"" varchar(200) NOT NULL,
                        ""Slug"" varchar(200) NOT NULL,
                        ""ImageUrl"" varchar(1000) NOT NULL,
                        ""Link"" varchar(500) NOT NULL,
                        ""ProductCount"" int NOT NULL DEFAULT 0,
                        ""DisplayOrder"" int NOT NULL DEFAULT 0,
                        ""IsActive"" boolean NOT NULL DEFAULT true
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_HomeCategorySlides_DisplayOrder"" ON content.""HomeCategorySlides"" (""DisplayOrder"");

                    CREATE TABLE IF NOT EXISTS content.""FeaturedBrands"" (
                        ""Id"" varchar(450) PRIMARY KEY,
                        ""Name"" varchar(200) NOT NULL,
                        ""LogoUrl"" varchar(1000),
                        ""Link"" varchar(500) NOT NULL,
                        ""DisplayOrder"" int NOT NULL DEFAULT 0,
                        ""IsActive"" boolean NOT NULL DEFAULT true
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_FeaturedBrands_DisplayOrder"" ON content.""FeaturedBrands"" (""DisplayOrder"");
                ");
                
                // Create Orders database and tables (only if Orders module is registered)
                var ordersDb = services.GetService<OrdersDbContext>();
                if (ordersDb is not null)
                {
                    if (ordersDb.Database.GetPendingMigrations().Any())
                    {
                        Console.WriteLine("📦 Applying Orders migrations...");
                        await ordersDb.Database.MigrateAsync();
                    }
                    else
                    {
                        Console.WriteLine("🔨 Creating Orders schema and tables...");
                        await ordersDb.Database.EnsureCreatedAsync();
                    }

                    var ordersTableCount = await ExecuteScalarAsync<int>(ordersDb,
                        "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'orders'");

                    if (ordersTableCount == 0)
                    {
                        Console.WriteLine("⚠️ Orders tables not found, forcing creation...");

                        await ordersDb.Database.ExecuteSqlRawAsync(@"
                            CREATE TABLE IF NOT EXISTS orders.""Carts"" (
                                ""Id"" varchar(450) PRIMARY KEY,
                                ""PartnerUserId"" varchar(450) NOT NULL,
                                ""PartnerCompanyId"" varchar(450) NOT NULL,
                                ""IsActive"" boolean NOT NULL DEFAULT true,
                                ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                                ""UpdatedAt"" timestamp
                            );
                            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Carts_PartnerUserId"" ON orders.""Carts"" (""PartnerUserId"");
                            CREATE INDEX IF NOT EXISTS ""IX_Carts_PartnerCompanyId"" ON orders.""Carts"" (""PartnerCompanyId"");
                        ");

                        await ordersDb.Database.ExecuteSqlRawAsync(@"
                            CREATE TABLE IF NOT EXISTS orders.""CartItems"" (
                                ""Id"" varchar(450) PRIMARY KEY,
                                ""CartId"" varchar(450) NOT NULL,
                                ""ProductId"" varchar(450) NOT NULL,
                                ""ProductName"" varchar(500) NOT NULL,
                                ""ProductSKU"" varchar(100) NOT NULL,
                                ""ProductImageUrl"" varchar(1000),
                                ""Quantity"" int NOT NULL DEFAULT 1,
                                ""SelectedVariants"" text,
                                ""CustomizationNotes"" varchar(2000),
                                ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                                ""UpdatedAt"" timestamp,
                                FOREIGN KEY (""CartId"") REFERENCES orders.""Carts""(""Id"") ON DELETE CASCADE
                            );
                            CREATE INDEX IF NOT EXISTS ""IX_CartItems_CartId"" ON orders.""CartItems"" (""CartId"");
                            CREATE INDEX IF NOT EXISTS ""IX_CartItems_ProductId"" ON orders.""CartItems"" (""ProductId"");
                        ");

                        await ordersDb.Database.ExecuteSqlRawAsync(@"
                            CREATE TABLE IF NOT EXISTS orders.""Orders"" (
                                ""Id"" varchar(450) PRIMARY KEY,
                                ""OrderNumber"" varchar(50) NOT NULL,
                                ""PartnerCompanyId"" varchar(450) NOT NULL,
                                ""PartnerUserId"" varchar(450) NOT NULL,
                                ""PartnerCompanyName"" varchar(200) NOT NULL,
                                ""Status"" int NOT NULL,
                                ""SubTotal"" decimal(18,2),
                                ""TaxAmount"" decimal(18,2),
                                ""ShippingCost"" decimal(18,2),
                                ""Discount"" decimal(18,2),
                                ""TotalAmount"" decimal(18,2),
                                ""Currency"" varchar(10),
                                ""DeliveryAddress"" varchar(500) NOT NULL,
                                ""DeliveryCity"" varchar(100) NOT NULL,
                                ""DeliveryState"" varchar(100) NOT NULL,
                                ""DeliveryPostalCode"" varchar(20) NOT NULL,
                                ""DeliveryCountry"" varchar(100) NOT NULL,
                                ""DeliveryNotes"" varchar(2000),
                                ""RequestedDeliveryDate"" timestamp,
                                ""ExpectedDeliveryDate"" timestamp,
                                ""ActualDeliveryDate"" timestamp,
                                ""Notes"" varchar(5000),
                                ""InternalNotes"" varchar(5000),
                                ""TrackingNumber"" varchar(200),
                                ""ShippingProvider"" varchar(100),
                                ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                                ""UpdatedAt"" timestamp,
                                ""SubmittedAt"" timestamp,
                                ""ConfirmedAt"" timestamp,
                                ""CancelledAt"" timestamp
                            );
                            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Orders_OrderNumber"" ON orders.""Orders"" (""OrderNumber"");
                            CREATE INDEX IF NOT EXISTS ""IX_Orders_PartnerCompanyId"" ON orders.""Orders"" (""PartnerCompanyId"");
                            CREATE INDEX IF NOT EXISTS ""IX_Orders_PartnerUserId"" ON orders.""Orders"" (""PartnerUserId"");
                            CREATE INDEX IF NOT EXISTS ""IX_Orders_Status"" ON orders.""Orders"" (""Status"");
                            CREATE INDEX IF NOT EXISTS ""IX_Orders_CreatedAt"" ON orders.""Orders"" (""CreatedAt"");
                        ");

                        await ordersDb.Database.ExecuteSqlRawAsync(@"
                            CREATE TABLE IF NOT EXISTS orders.""OrderItems"" (
                                ""Id"" varchar(450) PRIMARY KEY,
                                ""OrderId"" varchar(450) NOT NULL,
                                ""ProductId"" varchar(450) NOT NULL,
                                ""ProductName"" varchar(500) NOT NULL,
                                ""ProductSKU"" varchar(100) NOT NULL,
                                ""ProductImageUrl"" varchar(1000),
                                ""Quantity"" int NOT NULL DEFAULT 1,
                                ""SelectedVariants"" text,
                                ""UnitPrice"" decimal(18,2),
                                ""Discount"" decimal(18,2),
                                ""TotalPrice"" decimal(18,2),
                                ""CustomizationNotes"" varchar(2000),
                                ""DisplayOrder"" int NOT NULL DEFAULT 0,
                                ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                                FOREIGN KEY (""OrderId"") REFERENCES orders.""Orders""(""Id"") ON DELETE CASCADE
                            );
                            CREATE INDEX IF NOT EXISTS ""IX_OrderItems_OrderId"" ON orders.""OrderItems"" (""OrderId"");
                            CREATE INDEX IF NOT EXISTS ""IX_OrderItems_ProductId"" ON orders.""OrderItems"" (""ProductId"");
                        ");

                        await ordersDb.Database.ExecuteSqlRawAsync(@"
                            CREATE TABLE IF NOT EXISTS orders.""OrderComments"" (
                                ""Id"" varchar(450) PRIMARY KEY,
                                ""OrderId"" varchar(450) NOT NULL,
                                ""Content"" varchar(5000) NOT NULL,
                                ""Type"" int NOT NULL,
                                ""AuthorId"" varchar(450) NOT NULL,
                                ""AuthorName"" varchar(200) NOT NULL,
                                ""AuthorType"" varchar(50) NOT NULL,
                                ""IsInternal"" boolean NOT NULL DEFAULT false,
                                ""IsSystemGenerated"" boolean NOT NULL DEFAULT false,
                                ""AttachmentUrl"" varchar(1000),
                                ""AttachmentFileName"" varchar(500),
                                ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                                ""UpdatedAt"" timestamp,
                                FOREIGN KEY (""OrderId"") REFERENCES orders.""Orders""(""Id"") ON DELETE CASCADE
                            );
                            CREATE INDEX IF NOT EXISTS ""IX_OrderComments_OrderId"" ON orders.""OrderComments"" (""OrderId"");
                            CREATE INDEX IF NOT EXISTS ""IX_OrderComments_CreatedAt"" ON orders.""OrderComments"" (""CreatedAt"");
                        ");

                        Console.WriteLine("✅ Orders tables created manually");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Orders schema initialized ({ordersTableCount} tables)");
                    }

                    // Ensure SavedAddresses table exists
                    await ordersDb.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS orders.""SavedAddresses"" (
                            ""Id"" varchar(450) PRIMARY KEY,
                            ""PartnerUserId"" varchar(450) NOT NULL,
                            ""PartnerCompanyId"" varchar(450) NOT NULL,
                            ""Label"" varchar(100) NOT NULL,
                            ""Address"" varchar(500) NOT NULL,
                            ""City"" varchar(100) NOT NULL,
                            ""State"" varchar(100) NOT NULL,
                            ""PostalCode"" varchar(20) NOT NULL,
                            ""Country"" varchar(100) NOT NULL,
                            ""IsDefault"" boolean NOT NULL DEFAULT false,
                            ""CreatedAt"" timestamp NOT NULL DEFAULT now(),
                            ""UpdatedAt"" timestamp
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_SavedAddresses_PartnerUserId"" ON orders.""SavedAddresses"" (""PartnerUserId"");
                    ");
                }
                else
                {
                    Console.WriteLine("ℹ️ Orders module not registered (AppMode excludes it), skipping Orders tables");
                }

                Console.WriteLine("✅ All database schemas and tables initialized successfully");
                
                // Seed default application settings
                await SeedDefaultSettingsAsync(contentDb);
                
                // Seed home sliders mock data (if empty)
                await SeedHomeSlidersDataAsync(contentDb);
                
                // If we got here, initialization was successful
                return;
            }
            catch (NpgsqlException ex) when (retry < maxRetries)
            {
                Console.WriteLine($"⚠️ Database connection failed (attempt {retry}/{maxRetries}): {ex.Message}");
                Console.WriteLine($"⏳ Waiting {delayMilliseconds}ms before retry...");
                await Task.Delay(delayMilliseconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during database initialization: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (retry == maxRetries)
                {
                    Console.WriteLine("❌ Max retries reached. Make sure PostgreSQL is running:");
                    Console.WriteLine("   docker-compose up -d");
                    throw;
                }
            }
        }
    }
    
    private static async Task<T> ExecuteScalarAsync<T>(DbContext context, string sql)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        await context.Database.OpenConnectionAsync();
        
        try
        {
            var result = await command.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result ?? 0, typeof(T));
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
    
    private static async Task SeedDefaultSettingsAsync(ContentDbContext contentDb)
    {
        // Check if settings already exist
        var existingSettingsCount = await ExecuteScalarAsync<long>(contentDb,
            "SELECT COUNT(*) FROM content.\"AppSettings\"");
        
        if (existingSettingsCount > 0)
        {
            Console.WriteLine("⏭️ App settings already seeded, skipping...");
            return;
        }
        
        Console.WriteLine("🌱 Seeding default application settings...");
        
        var defaultSettings = new[]
        {
            new { Key = "Features.Blog.Enabled", Value = "false", DisplayName = "Enable Blog", Description = "Show/hide blog section in the application", Category = "Features", DataType = "boolean" },
            new { Key = "Features.HomeSliders.Enabled", Value = "true", DisplayName = "Enable Home Sliders", Description = "Show hero and category sliders on homepage (B2C/Storefront only, hidden in B2B mode)", Category = "Features", DataType = "boolean" },
            new { Key = "Features.Pricing.Enabled", Value = "false", DisplayName = "Enable Pricing", Description = "Show/hide product prices (for B2B quote-to-order)", Category = "Features", DataType = "boolean" },
            new { Key = "Features.PublicCatalog.Enabled", Value = "true", DisplayName = "Enable Public Catalog", Description = "Allow public users to browse products", Category = "Features", DataType = "boolean" },
            new { Key = "Features.PublicStorefront.Enabled", Value = "false", DisplayName = "Enable Public Storefront", Description = "Show public website with header/footer. When disabled, visitors are redirected to Partner login.", Category = "Features", DataType = "boolean" },
            new { Key = "Site.Name", Value = "Storefront", DisplayName = "Site Name", Description = "Name of the website", Category = "General", DataType = "string" },
            new { Key = "Site.ContactEmail", Value = "contact@storefront.com", DisplayName = "Contact Email", Description = "Primary contact email address", Category = "General", DataType = "string" },
            new { Key = "Site.MaintenanceMode", Value = "false", DisplayName = "Maintenance Mode", Description = "Put site in maintenance mode", Category = "General", DataType = "boolean" }
        };
        
        foreach (var setting in defaultSettings)
        {
            await contentDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO content.""AppSettings"" (""Key"", ""Value"", ""DisplayName"", ""Description"", ""Category"", ""DataType"", ""UpdatedAt"")
                VALUES ('{setting.Key}', '{setting.Value}', '{setting.DisplayName}', '{setting.Description}', '{setting.Category}', '{setting.DataType}', NOW())
                ON CONFLICT (""Key"") DO NOTHING;
            ");
        }
        
        Console.WriteLine($"✅ Seeded {defaultSettings.Length} default settings");
    }

    private static async Task SeedHomeSlidersDataAsync(ContentDbContext contentDb)
    {
        var heroCount = await ExecuteScalarAsync<long>(contentDb, "SELECT COUNT(*) FROM content.\"HeroSlides\"");
        if (heroCount > 0)
        {
            Console.WriteLine("⏭️ Hero slides already seeded, skipping...");
            return;
        }

        Console.WriteLine("🌱 Seeding home sliders mock data (Afeks style)...");

        var heroSlides = new[]
        {
            (Id: "hs-1", Title: "Elektrikli El Aletlerinde Fırsat", Subtitle: "Matkaplar, testereler ve daha fazlası", ImageUrl: "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=1200&h=400&fit=crop", Link: "/products?categoryId=cat-elektrikli", LinkText: "Keşfet", DisplayOrder: 0),
            (Id: "hs-2", Title: "Mutfak ve Banyo Çözümleri", Subtitle: "Tezgahlar, eviyeler, hazır mutfaklar", ImageUrl: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=1200&h=400&fit=crop", Link: "/products?categoryId=cat-mutfak", LinkText: "İncele", DisplayOrder: 1),
            (Id: "hs-3", Title: "Mobilya ve Dekorasyon", Subtitle: "Koltuklar, masalar, şifonyerler", ImageUrl: "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=1200&h=400&fit=crop", Link: "/products?categoryId=cat-mobilya", LinkText: "Alışverişe Başla", DisplayOrder: 2),
            (Id: "hs-4", Title: "Beyaz Eşya Kampanyası", Subtitle: "Buzdolabı, çamaşır makinesi, bulaşık makinesi", ImageUrl: "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&h=400&fit=crop", Link: "/products?categoryId=cat-beyaz-esya", LinkText: "Kampanyaları Gör", DisplayOrder: 3),
        };

        foreach (var s in heroSlides)
        {
            await contentDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO content.""HeroSlides"" (""Id"", ""Title"", ""Subtitle"", ""ImageUrl"", ""Link"", ""LinkText"", ""DisplayOrder"", ""IsActive"")
                VALUES ('{s.Id}', '{s.Title.Replace("'", "''")}', '{s.Subtitle?.Replace("'", "''")}', '{s.ImageUrl}', '{s.Link}', '{s.LinkText}', {s.DisplayOrder}, true)
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        var categorySlides = new[]
        {
            (Id: "hcs-1", Name: "Matkaplar", Slug: "matkaplar", ImageUrl: "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=300&h=200&fit=crop", Link: "/products?categoryId=cat-matkaplar", ProductCount: 2, DisplayOrder: 0),
            (Id: "hcs-2", Name: "Mutfak Tezgahları", Slug: "mutfak-tezgahlari", ImageUrl: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=300&h=200&fit=crop", Link: "/products?categoryId=cat-tezgahlar", ProductCount: 1, DisplayOrder: 1),
            (Id: "hcs-3", Name: "Koltuklar", Slug: "koltuklar", ImageUrl: "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=300&h=200&fit=crop", Link: "/products?categoryId=cat-koltuklar", ProductCount: 1, DisplayOrder: 2),
            (Id: "hcs-4", Name: "Banyo Bataryaları", Slug: "banyo-bataryalari", ImageUrl: "https://images.unsplash.com/photo-1552321554-5fefe8c9ef14?w=300&h=200&fit=crop", Link: "/products?q=batarya", ProductCount: 0, DisplayOrder: 3),
            (Id: "hcs-5", Name: "Hırdavat", Slug: "hirdavat", ImageUrl: "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?w=300&h=200&fit=crop", Link: "/products?categoryId=cat-hirdavat", ProductCount: 1, DisplayOrder: 4),
            (Id: "hcs-6", Name: "Beyaz Eşya", Slug: "beyaz-esya", ImageUrl: "https://images.unsplash.com/photo-1571175443880-49e1d25b2bc5?w=300&h=200&fit=crop", Link: "/products?categoryId=cat-beyaz-esya", ProductCount: 1, DisplayOrder: 5),
        };

        foreach (var s in categorySlides)
        {
            await contentDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO content.""HomeCategorySlides"" (""Id"", ""Name"", ""Slug"", ""ImageUrl"", ""Link"", ""ProductCount"", ""DisplayOrder"", ""IsActive"")
                VALUES ('{s.Id}', '{s.Name.Replace("'", "''")}', '{s.Slug}', '{s.ImageUrl}', '{s.Link}', {s.ProductCount}, {s.DisplayOrder}, true)
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        var featuredBrands = new[]
        {
            (Id: "fb-1", Name: "Makita", Link: "/products?brand=Makita", DisplayOrder: 0),
            (Id: "fb-2", Name: "Stanley", Link: "/products?brand=Stanley", DisplayOrder: 1),
            (Id: "fb-3", Name: "Bosch", Link: "/products?brand=Bosch", DisplayOrder: 2),
            (Id: "fb-4", Name: "Electrolux", Link: "/products?brand=Electrolux", DisplayOrder: 3),
            (Id: "fb-5", Name: "Einhell", Link: "/products?brand=Einhell", DisplayOrder: 4),
            (Id: "fb-6", Name: "Dewalt", Link: "/products?brand=Dewalt", DisplayOrder: 5),
            (Id: "fb-7", Name: "Karcher", Link: "/products?brand=Karcher", DisplayOrder: 6),
            (Id: "fb-8", Name: "Ryobi", Link: "/products?brand=Ryobi", DisplayOrder: 7),
        };

        foreach (var b in featuredBrands)
        {
            await contentDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO content.""FeaturedBrands"" (""Id"", ""Name"", ""Link"", ""DisplayOrder"", ""IsActive"")
                VALUES ('{b.Id}', '{b.Name}', '{b.Link}', {b.DisplayOrder}, true)
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        Console.WriteLine("✅ Home sliders mock data seeded");
    }

    private static async Task SeedCatalogDataAsync(CatalogDbContext catalogDb)
    {
        var categoryCount = await ExecuteScalarAsync<long>(catalogDb, "SELECT COUNT(*) FROM catalog.\"Categories\"");
        if (categoryCount > 0)
        {
            Console.WriteLine("⏭️ Catalog data already seeded, skipping...");
            return;
        }

        Console.WriteLine("🌱 Seeding catalog mock data (categories, brands, products)...");

        // Categories - Afeks style (parent + children)
        var categories = new[]
        {
            (Id: "cat-elektrikli", Name: "Elektrikli El Aletleri", Slug: "elektrikli-el-aletleri", ParentId: (string?)null, DisplayOrder: 0, ShowInNavbar: true),
            (Id: "cat-matkaplar", Name: "Matkaplar", Slug: "matkaplar", ParentId: "cat-elektrikli", DisplayOrder: 0, ShowInNavbar: false),
            (Id: "cat-testereler", Name: "Testereler", Slug: "testereler", ParentId: "cat-elektrikli", DisplayOrder: 1, ShowInNavbar: false),
            (Id: "cat-mutfak", Name: "Mutfak", Slug: "mutfak", ParentId: (string?)null, DisplayOrder: 1, ShowInNavbar: true),
            (Id: "cat-tezgahlar", Name: "Mutfak Tezgahları", Slug: "mutfak-tezgahlari", ParentId: "cat-mutfak", DisplayOrder: 0, ShowInNavbar: false),
            (Id: "cat-mobilya", Name: "Mobilya", Slug: "mobilya", ParentId: (string?)null, DisplayOrder: 2, ShowInNavbar: true),
            (Id: "cat-koltuklar", Name: "Koltuklar", Slug: "koltuklar", ParentId: "cat-mobilya", DisplayOrder: 0, ShowInNavbar: false),
            (Id: "cat-masalar", Name: "Masalar", Slug: "masalar", ParentId: "cat-mobilya", DisplayOrder: 1, ShowInNavbar: false),
            (Id: "cat-hirdavat", Name: "Hırdavat", Slug: "hirdavat", ParentId: (string?)null, DisplayOrder: 3, ShowInNavbar: true),
            (Id: "cat-beyaz-esya", Name: "Beyaz Eşya", Slug: "beyaz-esya", ParentId: (string?)null, DisplayOrder: 4, ShowInNavbar: true),
        };

        foreach (var c in categories)
        {
            await catalogDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO catalog.""Categories"" (""Id"", ""Name"", ""Slug"", ""ParentId"", ""DisplayOrder"", ""IsActive"", ""ShowInNavbar"", ""CreatedAt"")
                VALUES ('{c.Id}', '{c.Name.Replace("'", "''")}', '{c.Slug}', {(c.ParentId == null ? "NULL" : $"'{c.ParentId}'")}, {c.DisplayOrder}, true, {c.ShowInNavbar.ToString().ToLowerInvariant()}, NOW())
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        // Brands
        var brands = new[]
        {
            (Id: "brand-makita", Name: "Makita"),
            (Id: "brand-stanley", Name: "Stanley"),
            (Id: "brand-bosch", Name: "Bosch"),
            (Id: "brand-electrolux", Name: "Electrolux"),
            (Id: "brand-dewalt", Name: "Dewalt"),
            (Id: "brand-einhell", Name: "Einhell"),
        };

        foreach (var b in brands)
        {
            await catalogDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO catalog.""Brands"" (""Id"", ""Name"", ""IsActive"", ""CreatedAt"")
                VALUES ('{b.Id}', '{b.Name}', true, NOW())
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        // Products - at least 2 per category
        var products = new[]
        {
            // Matkaplar (2)
            (Id: "prod-1", Name: "Darbeli Matkap 800W", SKU: "MAT-001", Slug: "darbeli-matkap-800w", CategoryId: "cat-matkaplar", BrandId: "brand-makita", Price: 1299.99m, StockQty: 25, ImageUrl: "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop", Featured: true),
            (Id: "prod-2", Name: "Akülü Vidalama Seti", SKU: "VID-001", Slug: "akulu-vidalama-seti", CategoryId: "cat-matkaplar", BrandId: "brand-bosch", Price: 899.00m, StockQty: 18, ImageUrl: "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-2b", Name: "Kırıcı Delici 5 kg", SKU: "MAT-002", Slug: "kirici-delici-5kg", CategoryId: "cat-matkaplar", BrandId: "brand-dewalt", Price: 2199.00m, StockQty: 10, ImageUrl: "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop", Featured: false),
            // Testereler (2)
            (Id: "prod-3", Name: "Daire Testere", SKU: "TES-001", Slug: "daire-testere", CategoryId: "cat-testereler", BrandId: "brand-stanley", Price: 1599.00m, StockQty: 12, ImageUrl: "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-3b", Name: "Şerit Testere", SKU: "TES-002", Slug: "serit-testere", CategoryId: "cat-testereler", BrandId: "brand-einhell", Price: 1299.00m, StockQty: 7, ImageUrl: "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?w=600&h=600&fit=crop", Featured: false),
            // Mutfak Tezgahları (2)
            (Id: "prod-4", Name: "Granit Mutfak Tezgahı", SKU: "TEZ-001", Slug: "granit-mutfak-tezgahi", CategoryId: "cat-tezgahlar", BrandId: (string?)null, Price: 4500.00m, StockQty: 5, ImageUrl: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-4b", Name: "Mermer Tezgah 3m", SKU: "TEZ-002", Slug: "mermer-tezgah-3m", CategoryId: "cat-tezgahlar", BrandId: (string?)null, Price: 5200.00m, StockQty: 4, ImageUrl: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&h=600&fit=crop", Featured: false),
            // Koltuklar (2)
            (Id: "prod-5", Name: "3 Kişilik Koltuk Takımı", SKU: "KOL-001", Slug: "3-kisilik-koltuk-takimi", CategoryId: "cat-koltuklar", BrandId: (string?)null, Price: 12500.00m, StockQty: 8, ImageUrl: "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&h=600&fit=crop", Featured: true),
            (Id: "prod-5b", Name: "Kanepe 2 Kişilik", SKU: "KOL-002", Slug: "kanepe-2-kisilik", CategoryId: "cat-koltuklar", BrandId: (string?)null, Price: 6500.00m, StockQty: 12, ImageUrl: "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&h=600&fit=crop", Featured: false),
            // Masalar (2)
            (Id: "prod-6", Name: "Yemek Masası 6 Kişilik", SKU: "MAS-001", Slug: "yemek-masasi-6-kisilik", CategoryId: "cat-masalar", BrandId: (string?)null, Price: 3200.00m, StockQty: 15, ImageUrl: "https://images.unsplash.com/photo-1617806118233-18e1de247200?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-6b", Name: "Çalışma Masası", SKU: "MAS-002", Slug: "calisma-masasi", CategoryId: "cat-masalar", BrandId: (string?)null, Price: 1899.00m, StockQty: 20, ImageUrl: "https://images.unsplash.com/photo-1617806118233-18e1de247200?w=600&h=600&fit=crop", Featured: false),
            // Hırdavat (2)
            (Id: "prod-7", Name: "Çekiç ve Keski Seti", SKU: "HIR-001", Slug: "cekic-ve-keski-seti", CategoryId: "cat-hirdavat", BrandId: "brand-stanley", Price: 249.99m, StockQty: 50, ImageUrl: "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-7b", Name: "Tornavida Seti 32 Parça", SKU: "HIR-002", Slug: "tornavida-seti-32-parca", CategoryId: "cat-hirdavat", BrandId: "brand-bosch", Price: 179.99m, StockQty: 80, ImageUrl: "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?w=600&h=600&fit=crop", Featured: false),
            // Beyaz Eşya (2)
            (Id: "prod-8", Name: "Buzdolabı A++ 300L", SKU: "BUZ-001", Slug: "buzdolabi-a-300l", CategoryId: "cat-beyaz-esya", BrandId: "brand-electrolux", Price: 8999.00m, StockQty: 6, ImageUrl: "https://images.unsplash.com/photo-1571175443880-49e1d25b2bc5?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-8b", Name: "Çamaşır Makinesi 9 kg", SKU: "CAM-001", Slug: "camasir-makinesi-9kg", CategoryId: "cat-beyaz-esya", BrandId: "brand-electrolux", Price: 5499.00m, StockQty: 9, ImageUrl: "https://images.unsplash.com/photo-1571175443880-49e1d25b2bc5?w=600&h=600&fit=crop", Featured: false),
            // Parent: Elektrikli El Aletleri (2 - general power tools)
            (Id: "prod-e1", Name: "Taşlama Makinesi 230mm", SKU: "ELE-001", Slug: "taslama-makinesi-230mm", CategoryId: "cat-elektrikli", BrandId: "brand-makita", Price: 899.00m, StockQty: 14, ImageUrl: "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-e2", Name: "Kompresör 50L", SKU: "ELE-002", Slug: "kompresor-50l", CategoryId: "cat-elektrikli", BrandId: "brand-stanley", Price: 1599.00m, StockQty: 6, ImageUrl: "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&h=600&fit=crop", Featured: false),
            // Parent: Mutfak (2)
            (Id: "prod-m1", Name: "Eviye Bataryası Krom", SKU: "MUT-001", Slug: "eviye-bataryasi-krom", CategoryId: "cat-mutfak", BrandId: (string?)null, Price: 649.00m, StockQty: 25, ImageUrl: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-m2", Name: "Hazır Mutfak Dolabı Seti", SKU: "MUT-002", Slug: "hazir-mutfak-dolabi-seti", CategoryId: "cat-mutfak", BrandId: (string?)null, Price: 12500.00m, StockQty: 3, ImageUrl: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&h=600&fit=crop", Featured: false),
            // Parent: Mobilya (2)
            (Id: "prod-o1", Name: "Kitaplık 5 Raflı", SKU: "MOB-001", Slug: "kitaplik-5-rafli", CategoryId: "cat-mobilya", BrandId: (string?)null, Price: 899.00m, StockQty: 22, ImageUrl: "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&h=600&fit=crop", Featured: false),
            (Id: "prod-o2", Name: "Gardırop 2 Kapaklı", SKU: "MOB-002", Slug: "gardrop-2-kapakli", CategoryId: "cat-mobilya", BrandId: (string?)null, Price: 4200.00m, StockQty: 5, ImageUrl: "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&h=600&fit=crop", Featured: false),
        };

        foreach (var p in products)
        {
            var priceStr = p.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            await catalogDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO catalog.""Products"" (""Id"", ""Name"", ""SKU"", ""Slug"", ""ShortDescription"", ""ProductType"", ""Price"", ""CategoryId"", ""BrandId"", ""StockStatus"", ""StockQuantity"", ""IsActive"", ""IsFeatured"", ""CreatedAt"")
                VALUES ('{p.Id}', '{p.Name.Replace("'", "''")}', '{p.SKU}', '{p.Slug}', 'Kaliteli malzemeden üretilmiştir.', 'Simple', {priceStr}, '{p.CategoryId}', {(p.BrandId == null ? "NULL" : $"'{p.BrandId}'")}, 'InStock', {p.StockQty}, true, {p.Featured.ToString().ToLowerInvariant()}, NOW())
                ON CONFLICT (""Id"") DO NOTHING;
            ");

            // Product image
            var imgId = $"img-{p.Id}";
            await catalogDb.Database.ExecuteSqlRawAsync($@"
                INSERT INTO catalog.""ProductImages"" (""Id"", ""ProductId"", ""Url"", ""IsPrimary"", ""Type"", ""DisplayOrder"", ""CreatedAt"")
                VALUES ('{imgId}', '{p.Id}', '{p.ImageUrl}', true, 'Original', 0, NOW())
                ON CONFLICT (""Id"") DO NOTHING;
            ");
        }

        Console.WriteLine("✅ Catalog mock data seeded");
    }
}

