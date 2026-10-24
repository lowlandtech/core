
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default thread-safe in-memory <see cref="IPluginContextRegistry"/>.
/// Inherits primary storage from <see cref="DescriptorRegistry{TKey,TDescriptor}"/> and
/// maintains a secondary <see cref="_entityOwner"/> index for entity-type lookups.
/// </summary>
public sealed class PluginContextRegistry
    : DescriptorRegistry<string, PluginContextDescriptor>, IPluginContextRegistry
{
    // Secondary index: Type → owning plugin id. Reverse-indexed for O(1) entity lookups.
    private readonly ConcurrentDictionary<Type, string> _entityOwner = new();

    // Serialises Register/Unregister so entity-owner checks and primary-store writes are atomic.
    private readonly object _writeLock = new();

    public PluginContextRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(PluginContextDescriptor descriptor) => descriptor.PluginId;

    protected override void OnDuplicate(string key, PluginContextDescriptor existing, PluginContextDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    public PluginContextDescriptor? FindByPluginId(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);
        return Find(pluginId);
    }

    public PluginContextDescriptor? FindByEntityType(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        return _entityOwner.TryGetValue(entityType, out var pluginId) ? Find(pluginId) : null;
    }

    public override void Register(PluginContextDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        lock (_writeLock)
        {
            // Reject any entity already claimed by another plugin (ambiguous owner).
            foreach (var entityType in descriptor.EntityTypes)
            {
                if (_entityOwner.TryGetValue(entityType, out var existingOwner))
                    throw new AmbiguousEntityOwnerException(entityType, existingOwner, descriptor.PluginId);
            }

            // Delegates id-uniqueness check + primary storage to base (throws DuplicatePluginIdException).
            base.Register(descriptor);

            // Only reached when base.Register succeeded — write secondary index.
            foreach (var entityType in descriptor.EntityTypes)
                _entityOwner[entityType] = descriptor.PluginId;
        }
    }

    public override bool Unregister(string pluginId)
    {
        ArgumentNullException.ThrowIfNull(pluginId);

        lock (_writeLock)
        {
            var descriptor = Find(pluginId);
            if (descriptor is null) return false;

            if (!base.Unregister(pluginId)) return false;

            foreach (var entityType in descriptor.EntityTypes)
                _entityOwner.TryRemove(entityType, out _);
            return true;
        }
    }
}
