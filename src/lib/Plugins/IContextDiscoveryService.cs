namespace LowlandTech.Core.Plugins;

/// <summary>
/// Resolves the plugin context descriptor that owns a given entity type.
/// SPEC2050 US02.
/// </summary>
/// <remarks>
/// This is a lookup-only abstraction over <see cref="IPluginContextRegistry"/>; the
/// runtime resolution of an actual <c>DbContext</c> instance lives in
/// <see cref="IPluginContextFactory"/>.
/// </remarks>
public interface IContextDiscoveryService
{
    /// <summary>
    /// Resolve the plugin context descriptor that owns <typeparamref name="TEntity"/>,
    /// or <c>null</c> if no plugin claims the type.
    /// </summary>
    PluginContextDescriptor? ResolveByEntityType<TEntity>() where TEntity : class;

    /// <summary>
    /// Non-generic counterpart for reflection-based callers.
    /// </summary>
    PluginContextDescriptor? ResolveByEntityType(Type entityType);

    /// <summary>
    /// Resolve the plugin context descriptor for a given plugin id, or <c>null</c>.
    /// </summary>
    PluginContextDescriptor? ResolveByPluginId(string pluginId);

    /// <summary>
    /// Enumerate every registered plugin context descriptor.
    /// </summary>
    IReadOnlyCollection<PluginContextDescriptor> All { get; }
}
