namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks which plugins own which DbContexts and which entity types live in each plugin schema.
/// SPEC2050 US02.
/// </summary>
public interface IPluginContextRegistry
{
    /// <summary>All registered plugin context descriptors.</summary>
    IReadOnlyCollection<PluginContextDescriptor> All { get; }

    /// <summary>
    /// Register a plugin context descriptor.
    /// </summary>
    /// <exception cref="DuplicatePluginIdException">A descriptor with the same plugin id is already registered.</exception>
    /// <exception cref="AmbiguousEntityOwnerException">An entity type in the descriptor is already claimed by another plugin.</exception>
    void Register(PluginContextDescriptor descriptor);

    /// <summary>Remove a plugin context descriptor (uninstall path).</summary>
    bool Unregister(string pluginId);

    /// <summary>Look up a plugin context descriptor by plugin id; returns <c>null</c> when not registered.</summary>
    PluginContextDescriptor? FindByPluginId(string pluginId);

    /// <summary>Find the descriptor that owns a given entity type, or <c>null</c>.</summary>
    PluginContextDescriptor? FindByEntityType(Type entityType);
}
