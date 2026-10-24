
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Host-level wiring for discovered plugins.
/// <para>
/// Call <see cref="AddBackendPlugins"/> / <see cref="AddFrontendPlugins"/> during
/// service registration; both discover plugins, call <c>ConfigureServices</c> on each,
/// and store the list in DI so the post-build <c>Use</c> loop can retrieve it:
/// <code>
/// // Program.cs — service registration
/// var plugins = builder.Services.AddBackendPlugins(builder.Configuration);
///
/// // Program.cs — after app.Build()
/// foreach (var plugin in plugins)
///     plugin.Use(app);
/// </code>
/// </para>
/// </summary>
public static class PluginHostExtensions
{
    /// <summary>
    /// Discovers all <see cref="IApiPlugin"/> implementations, calls
    /// <see cref="IApiPlugin.ConfigureServices"/> on each, and registers the list
    /// in DI as <c>IReadOnlyList&lt;IApiPlugin&gt;</c>.
    /// </summary>
    public static IReadOnlyList<IApiPlugin> AddBackendPlugins(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var plugins = PluginDiscovery.DiscoverApiPlugins(configuration);
        foreach (var plugin in plugins)
            plugin.ConfigureServices(services, configuration);
        services.AddSingleton(plugins);
        return plugins;
    }

    /// <summary>
    /// Discovers all <see cref="IAppPlugin"/> implementations, calls
    /// <see cref="IAppPlugin.ConfigureServices"/> on each, and registers the list
    /// in DI as <c>IReadOnlyList&lt;IAppPlugin&gt;</c>.
    /// </summary>
    public static IReadOnlyList<IAppPlugin> AddFrontendPlugins(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var plugins = PluginDiscovery.DiscoverAppPlugins(configuration);
        foreach (var plugin in plugins)
            plugin.ConfigureServices(services, configuration);
        services.AddSingleton(plugins);
        return plugins;
    }
}
