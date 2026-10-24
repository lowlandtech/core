
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default thread-safe in-memory <see cref="IPluginRegistry"/>.
/// </summary>
public sealed class PluginRegistry : DescriptorRegistry<string, PluginDescriptor>, IPluginRegistry
{
    public PluginRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(PluginDescriptor descriptor) => descriptor.Id;

    protected override void OnDuplicate(string key, PluginDescriptor existing, PluginDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    IReadOnlyCollection<PluginDescriptor> IPluginRegistry.All => All;

    PluginDescriptor? IPluginRegistry.Find(string pluginId) => Find(pluginId);

    void IPluginRegistry.Register(PluginDescriptor descriptor) => Register(descriptor);

    bool IPluginRegistry.Unregister(string pluginId) => Unregister(pluginId);
}
