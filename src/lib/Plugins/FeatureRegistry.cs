
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default thread-safe in-memory <see cref="IFeatureRegistry"/>.
/// Inherits primary storage from <see cref="DescriptorRegistry{TKey,TDescriptor}"/> and
/// maintains a secondary <see cref="_byContract"/> index for contract-type lookups.
/// </summary>
public sealed class FeatureRegistry
    : DescriptorRegistry<string, FeatureDescriptor>, IFeatureRegistry
{
    // Secondary index: ContractType → featureId.
    // ConcurrentDictionary gives lock-free reads; mutations are serialised through the
    // two-phase Register / Unregister overrides below.
    private readonly ConcurrentDictionary<Type, string> _byContract = new();

    public FeatureRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(FeatureDescriptor descriptor) => descriptor.Id;

    protected override void OnDuplicate(string key, FeatureDescriptor existing, FeatureDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    public override void Register(FeatureDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        // Check contract conflict before the id-conflict check so that callers get the
        // more-specific exception when the contract is already bound.
        if (_byContract.TryGetValue(descriptor.ContractType, out var existingId))
            throw new FeatureContractConflictException(descriptor.ContractType, existingId, descriptor.Id);

        // Delegates id-uniqueness check + storage to base (throws DuplicatePluginIdException).
        base.Register(descriptor);

        // Only reached when base.Register succeeded — write secondary index.
        _byContract[descriptor.ContractType] = descriptor.Id;
    }

    public override bool Unregister(string featureId)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        var descriptor = Find(featureId);
        if (descriptor is null) return false;

        if (!base.Unregister(featureId)) return false;

        _byContract.TryRemove(descriptor.ContractType, out _);
        return true;
    }

    public FeatureDescriptor? FindByContract(Type contractType)
    {
        ArgumentNullException.ThrowIfNull(contractType);
        return _byContract.TryGetValue(contractType, out var id) ? Find(id) : null;
    }

    public void SetEnabled(string featureId, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        var descriptor = Find(featureId) ?? throw new FeatureNotRegisteredException(featureId);
        Replace(featureId, descriptor with { IsEnabled = enabled });
    }

    public void Remap(string featureId, Type newImplementationType)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        ArgumentNullException.ThrowIfNull(newImplementationType);
        var descriptor = Find(featureId) ?? throw new FeatureNotRegisteredException(featureId);

        if (!descriptor.ContractType.IsAssignableFrom(newImplementationType))
            throw new ArgumentException(
                $"'{newImplementationType.FullName}' does not implement contract '{descriptor.ContractType.FullName}'.",
                nameof(newImplementationType));

        Replace(featureId, descriptor with { ImplementationType = newImplementationType });
    }
}
