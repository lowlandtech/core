namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks endpoint group registrations contributed by features.
/// SPEC2050 US11.
/// </summary>
public interface IEndpointGroupRegistry
{
    IReadOnlyCollection<EndpointGroupDescriptor> All { get; }

    /// <summary>
    /// Register an endpoint group.
    /// </summary>
    /// <exception cref="EndpointPrefixConflictException">A different feature already registered the same prefix.</exception>
    void Register(EndpointGroupDescriptor descriptor);

    /// <summary>
    /// Remove all groups registered by a feature (called on feature disable / uninstall).
    /// </summary>
    void Unregister(string featureId);
}
