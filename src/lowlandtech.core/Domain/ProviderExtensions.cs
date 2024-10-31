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
    public static void AddProvider<TContext>(this IServiceCollection services, string? prefix = null)
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

        services.AddDbContextFactory<TContext>(options =>
        {
            options.ConfigureWarnings(b =>
                b.Ignore(CoreEventId.DetachedLazyLoadingWarning));

            if (providerOptions.UseProxies)
            {
                options.UseLazyLoadingProxies();
            }

            if (providerOptions.UseSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            if (providerOptions.UseTriggers)
            {
                options.UseTriggers();
            }

            switch (providerOptions.Provider)
            {
                case Providers.Sqlite:
                    options.UseSqlite(cs, builder =>
                        builder.MigrationsAssembly(migration));
                    break;
                case Providers.PgSql:
                    options.UseNpgsql(cs, builder =>
                        builder.MigrationsAssembly(migration));
                    break;
                case Providers.SqlServer:
                    options.UseSqlServer(cs, builder =>
                        builder.MigrationsAssembly(migration));
                    break;
                default:
                    options.UseInMemoryDatabase(cs);
                    break;
            }
        });

    }

    /// <summary>
    /// Manages the migration of the database.
    /// </summary>
    /// <param name="app"></param>
    /// <typeparam name="TContext"></typeparam>
    public static async Task UseMigration<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
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
    /// Checks if the environment is a test environment.
    /// </summary>
    /// <param name="env">The web host environment</param>
    /// <returns>True or false</returns>
    public static bool IsTest(this IWebHostEnvironment env)
    {
        return env.EnvironmentName == "Test";
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