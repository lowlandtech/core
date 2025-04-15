namespace LowlandTech.Plugin1;

[PluginId(Plugin1Constants.PluginId)]
public class Plugin1 : Plugin
{
    public Plugin1()
    {
        Id = new Guid(Plugin1Constants.PluginId);
    }

    public override void Install(ServiceRegistry services)
    {
        // then add data context;
        services.AddProvider<Plugin1Context>();
        // then add entity triggers;
        services.AddTransient<IBeforeSaveTrigger<Plugin1Entity>, Plugin1EntitySaving>();
        services.AddTransient<IAfterSaveTrigger<Plugin1Entity>, Plugin1EntitySaved>();
    }

    public override async Task Configure(IContainer container)
    {
        // then apply migrations to context;
        await container.UseMigration<Plugin1Context>();
        // then create a scope from the service builder;
        using var scope = container.CreateScope();
        // then get context;
        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<Plugin1Context>>();
        // then resolve a context;
        await using var context = await factory.CreateDbContextAsync();
        // then seed data use-cases;
        await context.Use<Plugin1UseCase>();
    }
}