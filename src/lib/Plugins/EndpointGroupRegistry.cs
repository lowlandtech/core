
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Default thread-safe in-memory <see cref="IEndpointGroupRegistry"/>.
/// </summary>
public sealed class EndpointGroupRegistry : DescriptorRegistry<string, EndpointGroupDescriptor>, IEndpointGroupRegistry
{
    public EndpointGroupRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(EndpointGroupDescriptor d) => d.GroupPrefix;

    protected override void OnDuplicate(string key, EndpointGroupDescriptor existing, EndpointGroupDescriptor incoming)
        => throw new EndpointPrefixConflictException(key, existing.FeatureId, incoming.FeatureId);

    /// <summary>
    /// Removes all endpoint groups registered by <paramref name="featureId"/>.
    /// This overrides the base key-based <c>Unregister</c> to perform a feature-scoped sweep.
    /// </summary>
    public new void Unregister(string featureId)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        RemoveWhere(d => string.Equals(d.FeatureId, featureId, StringComparison.OrdinalIgnoreCase));
    }
}
