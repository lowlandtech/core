namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default <see cref="IPluginContextFactory"/> that resolves DbContext instances from
/// the ambient <see cref="IServiceProvider"/> using the type recorded in the registry.
/// </summary>
public sealed class PluginContextFactory(
    IPluginContextRegistry registry,
    IServiceProvider services) : IPluginContextFactory
{
    public object Resolve(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        var descriptor = registry.FindByPluginId(pluginId)
            ?? throw new UnknownPluginException(pluginId);

        var resolved = services.GetService(descriptor.ContextType)
            ?? throw new UnknownPluginException(
                pluginId,
                $"Plugin '{pluginId}' is registered but no service of type '{descriptor.ContextType.FullName}' is available in the current scope.");

        return resolved;
    }
}
