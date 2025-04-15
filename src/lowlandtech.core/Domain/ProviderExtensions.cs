using Microsoft.EntityFrameworkCore.Internal;

namespace LowlandTech.Core.Domain;

/// <summary>
/// Provider extensions.
/// </summary>
public static class ProviderExtensions
{
    /// <summary>
    /// Adds the provider to the service collection.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="prefix"></param>
    public static void AddProvider<TContext>(this ServiceRegistry services, string? prefix = null)
        where TContext : DbContext
    {
        var serviceProvider = services
            .BuildServiceProvider();

        var configuration = serviceProvider
            .GetRequiredService<IConfiguration>();

        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("LowlandTech.Core.Domain.ProviderExtensions");

        var providerOptions = serviceProvider
            .GetRequiredService<IOptions<ProviderOptions>>()
            .Value;

        var cs = providerOptions.Provider == Providers.InMemory ? 
            providerOptions.Provider.GetConnectionString() : 
            configuration[$"ConnectionStrings:{providerOptions.Provider}"];

        var migration = prefix ?? $"{providerOptions.Prefix}.{providerOptions.Provider}";

        var builder = new DbContextOptionsBuilder<TContext>();

        builder.ConfigureWarnings(b =>
            b.Ignore(CoreEventId.DetachedLazyLoadingWarning));

        if (providerOptions.UseProxies)
        {
            builder.UseLazyLoadingProxies();
        }

        if (providerOptions.UseSensitiveDataLogging)
        {
            builder.EnableSensitiveDataLogging();
        }

        if (providerOptions.UseTriggers)
        {
            builder.UseTriggers();
        }

        switch (providerOptions.Provider)
        {
            case Providers.Sqlite:
                builder.UseSqlite(cs, builder =>
                    builder.MigrationsAssembly(migration));
                break;
            case Providers.PgSql:
                builder.UseNpgsql(cs, builder =>
                    builder.MigrationsAssembly(migration));
                break;
            case Providers.SqlServer:
                builder.UseSqlServer(cs, builder =>
                    builder.MigrationsAssembly(migration));
                break;
            default:
                builder.UseInMemoryDatabase(cs);
                break;
        }

        // Register the factories with unique names
        services.For<DbContextOptions<TContext>>().Use(ctx => builder.Options);
        services.For<IDbContextFactory<TContext>>()
            .Use<PooledDbContextFactory<TContext>>();
        services.For<TContext>()
            .Use<TContext>();
    }

    /// <summary>
    /// Manages the migration of the database.
    /// </summary>
    /// <param name="container"></param>
    /// <typeparam name="TContext"></typeparam>
    public static async Task UseMigration<TContext>(this IContainer container)
        where TContext : DbContext
    {
        using var scope = container.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var factory = serviceProvider
            .GetRequiredService<IDbContextFactory<TContext>>();

        var providerOptions = serviceProvider
            .GetRequiredService<IOptions<ProviderOptions>>()
            .Value;

        await using var context = await factory.CreateDbContextAsync();

        if (providerOptions.Provider == Providers.InMemory)
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            //if (context.Database.HasPendingModelChanges())
            await context.Database.MigrateAsync();
        }
    }

    /// <summary>
    /// Gets the connection string for the sqlite in-memory database.
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static string? GetConnectionString(this Providers provider)
    {
        return $"Data Source={Path.GetRandomFileName()};Mode=Memory;Cache=Shared";
    }
}