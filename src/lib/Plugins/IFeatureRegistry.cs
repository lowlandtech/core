namespace LowlandTech.Core.Plugins;

/// <summary>
/// In-memory registry of <see cref="FeatureDescriptor"/> entries plus their activation state.
/// SPEC2050 US03.
/// </summary>
public interface IFeatureRegistry
{
    IReadOnlyCollection<FeatureDescriptor> All { get; }

    FeatureDescriptor? Find(string featureId);
    FeatureDescriptor? FindByContract(Type contractType);

    /// <summary>
    /// Register a feature descriptor.
    /// </summary>
    /// <exception cref="DuplicatePluginIdException">A feature with the same id is already registered.</exception>
    /// <exception cref="FeatureContractConflictException">Another feature already binds the same contract type.</exception>
    void Register(FeatureDescriptor descriptor);

    bool Unregister(string featureId);

    /// <summary>
    /// Toggle a feature's enabled state.
    /// </summary>
    /// <exception cref="FeatureNotRegisteredException">The feature id is not registered.</exception>
    void SetEnabled(string featureId, bool enabled);

    /// <summary>
    /// Swap the implementation type bound to a feature's contract.
    /// </summary>
    /// <exception cref="FeatureNotRegisteredException">The feature id is not registered.</exception>
    void Remap(string featureId, Type newImplementationType);
}
