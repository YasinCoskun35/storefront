using Microsoft.EntityFrameworkCore;

namespace Storefront.Infrastructure;

public abstract class ModuleDbContext : DbContext
{
    private readonly string _schema;

    protected ModuleDbContext(
        DbContextOptions options,
        string schema)
        : base(options)
    {
        _schema = schema;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_schema);
        base.OnModelCreating(modelBuilder);
    }

    public static void ConfigureNpgsql(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        string schema,
        string migrationsHistoryTable)
    {
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable(migrationsHistoryTable, schema);
            });
    }
}
