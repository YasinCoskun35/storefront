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
}

