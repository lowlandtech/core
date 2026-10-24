namespace LowlandTech.Core.Plugins;

/// <summary>
/// Processes a release branch merge into main by registering new feature contributions
/// (EdgeTypes, NodeTypes) into the running plugin registries and emitting
/// <see cref="GraphUpgraded"/>.
/// SPEC2050 US05 SC11.
/// </summary>
public sealed class ReleaseUpgradeProcessor(
    IPluginEdgeTypeRegistry edgeTypes)
{
    /// <summary>
    /// Register each descriptor from <paramref name="newFeatures"/> and emit the upgrade event.
    /// </summary>
    public GraphUpgraded Apply(IReadOnlyList<EdgeTypeDescriptor> newEdgeTypes)
    {
        ArgumentNullException.ThrowIfNull(newEdgeTypes);

        var registeredVerbs = new List<string>();
        var activatedFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var descriptor in newEdgeTypes)
        {
            edgeTypes.Register(descriptor);
            registeredVerbs.Add(descriptor.Verb);
            activatedFeatures.Add(descriptor.FeatureId);
        }

        return new GraphUpgraded(
            AppliedFeatureIds: activatedFeatures.ToList(),
            NewEdgeTypeVerbs: registeredVerbs,
            NewNodeTypeKeys: [],
            UpgradedAt: DateTimeOffset.UtcNow);
    }
}
