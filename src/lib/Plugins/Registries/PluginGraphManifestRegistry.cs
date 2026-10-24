namespace LowlandTech.Core.Plugins.Registries;

/// <summary>
/// Thread-safe in-memory <see cref="IPluginGraphManifestRegistry"/>.
/// Manifests are keyed by <see cref="PluginGraphManifest.PluginId"/> (case-insensitive).
/// </summary>
public sealed class PluginGraphManifestRegistry : IPluginGraphManifestRegistry
{
    private readonly ConcurrentDictionary<string, PluginGraphManifest> _store = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<PluginGraphManifest> All => _store.Values.ToArray();

    public PluginGraphManifest? FindByPluginId(string pluginId)
        => _store.TryGetValue(pluginId, out var manifest) ? manifest : null;

    public void Register(PluginGraphManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (!_store.TryAdd(manifest.PluginId, manifest))
            throw new InvalidOperationException(
                $"A graph manifest for plugin '{manifest.PluginId}' is already registered.");
    }

    public IReadOnlyList<PluginGraphDescriptorEntry> AllDescriptors
        => _store.Values
            .SelectMany(m => m.NodeDescriptors.Concat(m.EdgeDescriptors))
            .ToArray();

    public IReadOnlyList<PluginGraphDescriptorEntry>? FindDescriptorsByPluginId(string pluginId)
        => _store.TryGetValue(pluginId, out var manifest)
            ? manifest.NodeDescriptors.Concat(manifest.EdgeDescriptors).ToArray()
            : null;
}
