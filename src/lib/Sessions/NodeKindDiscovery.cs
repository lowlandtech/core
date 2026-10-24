
namespace LowlandTech.Core.Sessions;

/// <summary>Priority-aware aggregator over <see cref="INodeKindRegistry"/> instances. The first registry (in descending priority order) that returns a non-null component wins; multi-registry collisions are logged.</summary>
public class NodeKindDiscovery(
    IEnumerable<INodeKindRegistry> registries,
    INodeKindDefault @default,
    ILogger<NodeKindDiscovery>? logger = null) : INodeKindDiscovery
{
    private readonly IReadOnlyList<INodeKindRegistry> _ordered =
        registries.OrderByDescending(r => r.Priority).ToList();

    public Type Default => @default.ComponentType;

    public Type? Find(string nodeTypeKey, string? kind)
    {
        // Collect-all-then-pick-first mirrors NodeBehaviorDiscovery (src/api/Services/NodeBehaviorDiscovery.cs)
        // so a multi-registry collision can be logged. Cost is O(N) registries; with the current
        // single-registry chat path it is one call. Do not refactor to early-exit without removing
        // the collision diagnostic too — see DRD pattern docs/drd/readme.md.
        var matches = new List<(INodeKindRegistry Registry, Type Component)>();
        foreach (var registry in _ordered)
        {
            var component = registry.GetComponent(nodeTypeKey, kind);
            if (component is not null)
                matches.Add((registry, component));
        }

        if (matches.Count == 0) return null;

        if (matches.Count > 1)
        {
            logger?.LogWarning(
                "Multiple INodeKindRegistry instances resolved ({NodeTypeKey}, {Kind}): {Registries}. Using highest-priority match.",
                nodeTypeKey, kind, string.Join(", ", matches.Select(m => m.Registry.GetType().Name)));
        }

        return matches[0].Component;
    }

    public IReadOnlyList<KindDescriptor> AvailableKindsFor(string nodeTypeKey)
        => _ordered
            .SelectMany(r => r.ListKinds(nodeTypeKey))
            .GroupBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
}
