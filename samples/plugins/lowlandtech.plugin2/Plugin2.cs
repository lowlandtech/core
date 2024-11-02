using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LowlandTech.Plugin2;

[PluginId(Plugin2Constants.PluginId)]
public class Plugin2 : Plugin
{
    public Plugin2()
    {
        Id = new Guid(Plugin2Constants.PluginId);
    }

    public override void Install(ServiceRegistry services)
    {
        // then add data context;
        services.AddProvider<Plugin2Context>();

        var id = typeof(Plugin2Context).GetCustomAttribute<DbContextIdAttribute>()?.Id;

        if (id is null) return;

        services.For<IDbContextFactory<Plugin2Context>>()
            .Use<PooledDbContextFactory<Plugin2Context>>()
            .Named(id);
    }

    public override async Task Configure(WebApplication app)
    {
        // then create a scope from the service builder;
        using var scope = app.Services.CreateScope();
        // then get context;
        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<Plugin2Context>>();
        // then resolve a context;
        await using var context = await factory.CreateDbContextAsync();
        // then apply migrations to context;
        await app.UseMigration<Plugin2Context>();
        // then seed data use-cases;
        await context.Use<Plugin2UseCase>();
    }
}
