
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks i18n resource keys and namespace contributions from plugins and features.
/// SPEC2050 US09.
/// </summary>
public interface IResourceRegistry
{
    IReadOnlyCollection<string> RegisteredNamespaces { get; }
    IReadOnlyCollection<ResourceDescriptor> All { get; }

    bool IsKeyRegistered(string key);

    /// <summary>Register a resource namespace owned by a plugin.</summary>
    void RegisterNamespace(string @namespace, string pluginId);

    /// <summary>Register a resource key (must be inside an already-registered namespace).</summary>
    /// <exception cref="DuplicatePluginIdException">A descriptor with the same key is already registered.</exception>
    void Register(ResourceDescriptor descriptor);

    /// <summary>Throw if the supplied key is not registered.</summary>
    /// <exception cref="UnknownResourceKeyException">The key is not in the registry.</exception>
    void EnsureRegistered(string key);

    /// <summary>Remove all namespaces and resource keys contributed by a plugin (uninstall path).</summary>
    void RemoveByPlugin(string pluginId);
}

public sealed class ResourceRegistry : DescriptorRegistry<string, ResourceDescriptor>, IResourceRegistry
{
    private readonly ConcurrentDictionary<string, string> _namespaces = new(StringComparer.OrdinalIgnoreCase); // namespace → pluginId

    public ResourceRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(ResourceDescriptor descriptor) => descriptor.Key;

    protected override void OnDuplicate(string key, ResourceDescriptor existing, ResourceDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    public IReadOnlyCollection<string> RegisteredNamespaces => _namespaces.Keys.ToArray();

    public bool IsKeyRegistered(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return IsRegistered(key);
    }

    public void RegisterNamespace(string @namespace, string pluginId)
    {
        ArgumentNullException.ThrowIfNull(@namespace);
        ArgumentNullException.ThrowIfNull(pluginId);
        _namespaces[@namespace] = pluginId;
    }

    public void EnsureRegistered(string key)
    {
        if (!IsRegistered(key))
            throw new UnknownResourceKeyException(key);
    }

    public void RemoveByPlugin(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        RemoveWhere(d => string.Equals(d.PluginId, pluginId, StringComparison.OrdinalIgnoreCase));

        foreach (var kvp in _namespaces.ToArray())
        {
            if (string.Equals(kvp.Value, pluginId, StringComparison.OrdinalIgnoreCase))
                _namespaces.TryRemove(kvp.Key, out _);
        }
    }
}
