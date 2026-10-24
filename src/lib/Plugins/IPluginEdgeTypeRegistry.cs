
namespace LowlandTech.Core.Plugins;

/// <summary>
/// Tracks <see cref="EdgeTypeDescriptor"/> contributions from features and resolves
/// available verbs for a given source node type, optionally filtered by feature
/// enabled state.
/// SPEC2050 US04.
/// </summary>
/// <remarks>
/// Named <c>IPluginEdgeTypeRegistry</c> to disambiguate from
/// <see cref="LowlandTech.Core.Graph.IEdgeTypeRegistry"/> in the graph metadata layer.
/// </remarks>
public interface IPluginEdgeTypeRegistry
{
    IReadOnlyCollection<EdgeTypeDescriptor> All { get; }

    /// <exception cref="DuplicatePluginIdException">An edge type with the same (sourceNodeType, verb) already exists.</exception>
    void Register(EdgeTypeDescriptor descriptor);

    /// <summary>Find a specific edge type by source node + verb.</summary>
    EdgeTypeDescriptor? Find(string sourceNodeType, string verb);

    /// <summary>List every edge type whose source node type matches.</summary>
    IReadOnlyCollection<EdgeTypeDescriptor> GetVerbsForNodeType(string sourceNodeType);

    /// <summary>List every edge type whose source matches AND whose feature is currently enabled.</summary>
    IReadOnlyCollection<EdgeTypeDescriptor> GetEnabledVerbsForNodeType(string sourceNodeType, IFeatureRegistry features);

    /// <summary>Remove every edge type registered by a feature.</summary>
    void RemoveByFeature(string featureId);
}

public sealed class PluginEdgeTypeRegistry : DescriptorRegistry<string, EdgeTypeDescriptor>, IPluginEdgeTypeRegistry
{
    public PluginEdgeTypeRegistry() : base(StringComparer.OrdinalIgnoreCase) { }

    protected override string KeyOf(EdgeTypeDescriptor descriptor)
        => MakeKey(descriptor.SourceNodeType, descriptor.Verb);

    protected override void OnDuplicate(string key, EdgeTypeDescriptor existing, EdgeTypeDescriptor incoming)
        => throw new DuplicatePluginIdException(key);

    public EdgeTypeDescriptor? Find(string sourceNodeType, string verb)
    {
        ArgumentNullException.ThrowIfNull(sourceNodeType);
        ArgumentNullException.ThrowIfNull(verb);
        return Find(MakeKey(sourceNodeType, verb));
    }

    public IReadOnlyCollection<EdgeTypeDescriptor> GetVerbsForNodeType(string sourceNodeType)
    {
        ArgumentNullException.ThrowIfNull(sourceNodeType);
        return All
            .Where(d => string.Equals(d.SourceNodeType, sourceNodeType, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public IReadOnlyCollection<EdgeTypeDescriptor> GetEnabledVerbsForNodeType(string sourceNodeType, IFeatureRegistry features)
    {
        ArgumentNullException.ThrowIfNull(features);

        var enabledFeatureIds = new HashSet<string>(
            features.All.Where(f => f.IsEnabled).Select(f => f.Id),
            StringComparer.OrdinalIgnoreCase);

        return GetVerbsForNodeType(sourceNodeType)
            .Where(d => enabledFeatureIds.Contains(d.FeatureId))
            .ToArray();
    }

    public void RemoveByFeature(string featureId)
    {
        ArgumentNullException.ThrowIfNull(featureId);
        RemoveWhere(d => string.Equals(d.FeatureId, featureId, StringComparison.OrdinalIgnoreCase));
    }

    private static string MakeKey(string sourceNodeType, string verb) => $"{sourceNodeType}|{verb}";
}
