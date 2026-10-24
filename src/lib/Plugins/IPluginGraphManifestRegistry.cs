namespace LowlandTech.Core.Plugins;

/// <summary>
/// Registry of <see cref="PluginGraphManifest"/> entries. Populated at startup
/// from plugins that implement <see cref="IGraphContributingPlugin"/>.
/// </summary>
public interface IPluginGraphManifestRegistry
{
    /// <summary>All registered plugin graph manifests.</summary>
    IReadOnlyCollection<PluginGraphManifest> All { get; }

    /// <summary>Look up a manifest by plugin id; returns <c>null</c> when not found.</summary>
    PluginGraphManifest? FindByPluginId(string pluginId);

    /// <summary>Register a plugin graph manifest.</summary>
    void Register(PluginGraphManifest manifest);

    /// <summary>
    /// All descriptor entries across all plugins.
    /// </summary>
    IReadOnlyList<PluginGraphDescriptorEntry> AllDescriptors { get; }

    /// <summary>
    /// Find descriptors owned by a specific plugin id.
    /// </summary>
    IReadOnlyList<PluginGraphDescriptorEntry>? FindDescriptorsByPluginId(string pluginId);
}
