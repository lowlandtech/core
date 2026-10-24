namespace LowlandTech.Core.Sessions;

/// <summary>Aggregates all registered <see cref="INodeKindRegistry"/> instances to resolve a Razor component for a (nodeTypeKey, kind) pair.</summary>
public interface INodeKindDiscovery
{
    /// <summary>Returns the component type for the pair, or <see langword="null"/> if no registry handles it. Callers should fall back to <see cref="Default"/> on null.</summary>
    Type? Find(string nodeTypeKey, string? kind);

    /// <summary>The fallback component type, sourced from <see cref="INodeKindDefault"/>. Always non-null after DI registration.</summary>
    Type Default { get; }

    /// <summary>Returns the deduplicated union of kinds contributed by all registries for the given node type key.</summary>
    IReadOnlyList<KindDescriptor> AvailableKindsFor(string nodeTypeKey);
}
