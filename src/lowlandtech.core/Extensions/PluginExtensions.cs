namespace LowlandTech.Core.Extensions;

/// <summary>
/// Extensions for plugins.
/// </summary>
public static class PluginExtensions
{
    /// <summary>
    /// Adds a plugin to the service registry.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="plugin"></param>
    public static void AddPlugin(this ServiceRegistry services, IPlugin plugin)
    {
        var pluginId = Guard.Against.MissingPluginId(plugin, nameof(plugin), $"ExtensionPlugin '{plugin.GetType().ShortDisplayName()}' does not provide a plugin id.");

        // add plugin to the installed plugins list
        if (services.All(s => s.ServiceType == plugin.GetType())) return;
        // install plugin
        plugin.Install(services);
        services.For<IPlugin>().Add(plugin).Named(pluginId);
    }

    /// <summary>
    /// Adds a plugin to the service registry.
    /// </summary>
    /// <typeparam name="T">The plugin</typeparam>
    /// <param name="services">The service registry</param>
    public static void AddPlugin<T>(this ServiceRegistry services) where T : IPlugin, new()
    {
        var plugin = new T();
        plugin.Name = plugin.GetType().Namespace!;
        services.AddPlugin(plugin);
    }

    /// <summary>
    /// Adds plugins to the service registry from the configuration.
    /// </summary>
    /// <param name="services">The service registry</param>
    public static void AddPlugins(this ServiceRegistry services)
    {
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        var logger = factory.CreateLogger("LowlandTech.Core.Extensions");
        var configuration = provider.GetRequiredService<IConfiguration>();

        // then configure data context options;
        services.Configure<ProviderOptions>(configuration
                .GetSection(ProviderOptions.ProviderContext));

        var options = new PluginOptions
        {
            Plugins = configuration
                .GetSection(PluginOptions.Name)
                .Get<List<PluginConfig>>()!
        };

        try
        {
            // Get the currently executing assembly
            var currentAssembly = Assembly.GetExecutingAssembly();

            // Get the AssemblyLoadContext for the current assembly
            var loadContext = AssemblyLoadContext.GetLoadContext(currentAssembly)!;
            var root = Path.Combine(new FileInfo(currentAssembly.Location).DirectoryName!);

            // Attempt to load the assembly
            foreach (var config in options.Plugins.Where(p => p.IsActive == true))
            {

                var assemblyName = AssemblyName.GetAssemblyName(Path.Combine(root, config.Name + ".dll"));

                if (assemblyName is null) continue;

                var assembly = loadContext.LoadFromAssemblyName(assemblyName);

                // Do something with the loaded assembly
                logger.LogInformation("Assembly {0} loaded successfully.", assembly.FullName);

                // Get the first type that implements the IPlugin interface
                var pluginType = assembly.GetTypes().FirstOrDefault(type => typeof(IPlugin).IsAssignableFrom(type));

                if (pluginType is not null)
                {
                    // Create an instance of the type
                    var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;

                    // Use the plugin
                    plugin.Name = config.Name;
                    plugin.IsActive = config.IsActive;
                    services.AddPlugin(plugin);

                    logger.LogInformation("{0}: Found and instantiated", pluginType.FullName);
                }
                else
                {
                    logger.LogWarning("No type implementing IPlugin found in the assembly.");
                }
            }
        }
        catch (Exception ex)
        {
            // Handle the exception if the assembly cannot be loaded
            logger.LogError("Error loading assembly: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Use plugins.
    /// </summary>
    /// <param name="app">The web application</param>
    public static void UsePlugins(this WebApplication app)
    {
        var container = app.Services.GetRequiredService<IContainer>();
        var plugins = container.GetAllInstances<IPlugin>();
        // and configure plugins;
        foreach (var plugin in plugins)
        {
            plugin.Configure(app);
        }
    }
}