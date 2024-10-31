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
        // then resolve configuration;
        var configuration = services
            .BuildServiceProvider()
            .GetRequiredService<IConfiguration>();

        // then configure data context options;
        services
            .Configure<ProviderOptions>(configuration
                .GetSection(ProviderOptions.ProviderContext));

        // then add data context;
        services.AddProvider<Plugin1Context>();
        // then add entity triggers;
        services.AddTransient<IBeforeSaveTrigger<Plugin1Entity>, Plugin1EntitySaving>();
        services.AddTransient<IAfterSaveTrigger<Plugin1Entity>, Plugin1EntitySaved>();
    }

    public override async void Configure(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        // then get context;
        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<Plugin1Context>>();
        // then apply migrations to context;
        await app.UseMigration<Plugin1Context>();
        // then seed data use-cases;
        await factory.Use<Plugin1UseCase, Plugin1Context>();
    }
}