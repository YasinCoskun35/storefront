using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.Modules.Catalog.Infrastructure.Persistence;
using Storefront.Modules.Content.Infrastructure.Persistence;
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
                
                // Create Identity database and tables
                var identityDb = services.GetRequiredService<IdentityDbContext>();
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
                    
                    Console.WriteLine("✅ Catalog tables created manually");
                }
                else
                {
                    Console.WriteLine($"✅ Catalog schema initialized ({catalogTableCount} tables)");
                }
                
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
                    
                if (contentTableCount == 0)
                {
                    Console.WriteLine("⚠️ Content tables not found, forcing creation...");
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
                    
                    Console.WriteLine("✅ Content tables created manually");
                }
                else
                {
                    Console.WriteLine($"✅ Content schema initialized ({contentTableCount} tables)");
                }
                
                Console.WriteLine("✅ All database schemas and tables initialized successfully");
                
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
}

