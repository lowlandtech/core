namespace LowlandTech.Core.Plugins;

/// <summary>
/// In-memory store of <see cref="PluginDescriptor"/> entries known to the host.
/// SPEC2050 US01.
/// </summary>
public interface IPluginRegistry
{
    /// <summary>
    /// All registered plugin descriptors.
    /// </summary>
    IReadOnlyCollection<PluginDescriptor> All { get; }

    /// <summary>
    /// Look up a plugin by id; returns <c>null</c> when not registered.
    /// </summary>
    PluginDescriptor? Find(string pluginId);

    /// <summary>
    /// Register a plugin descriptor.
    /// </summary>
    /// <exception cref="DuplicatePluginIdException">Thrown when a plugin with the same id is already registered.</exception>
    void Register(PluginDescriptor descriptor);

    /// <summary>
    /// Remove a plugin descriptor (uninstall path).
    /// </summary>
    bool Unregister(string pluginId);
}
