namespace LowlandTech.Core.Sessions;

public interface INodeKindRegistry
{
    /// <summary>Higher priority wins when multiple registries can resolve the same (nodeTypeKey, kind) pair.</summary>
    int Priority => 0;

    /// <summary>Returns the Razor component type for a (nodeTypeKey, kind) pair, or null if not handled by this registry.</summary>
    Type? GetComponent(string nodeTypeKey, string? kind);

    /// <summary>Lists Kinds this registry contributes for a given NodeType key. Empty for unrelated types.</summary>
    IReadOnlyList<KindDescriptor> ListKinds(string nodeTypeKey);
}
