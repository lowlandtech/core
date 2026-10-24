namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default <see cref="IContextDiscoveryService"/> backed by an <see cref="IPluginContextRegistry"/>.
/// </summary>
public sealed class ContextDiscoveryService(IPluginContextRegistry registry) : IContextDiscoveryService
{
    public IReadOnlyCollection<PluginContextDescriptor> All => registry.All;

    public PluginContextDescriptor? ResolveByEntityType<TEntity>() where TEntity : class
        => registry.FindByEntityType(typeof(TEntity));

    public PluginContextDescriptor? ResolveByEntityType(Type entityType)
        => registry.FindByEntityType(entityType);

    public PluginContextDescriptor? ResolveByPluginId(string pluginId)
        => registry.FindByPluginId(pluginId);
}
