
namespace LowlandTech.Core.Abstractions;

public record GraphSchema(
    List<NodeType> NodeTypes,
    List<EdgeType> EdgeTypes,
    List<PropertyDefinition> PropertyDefinitions);

public record GraphTree(List<Node> Nodes, List<Edge> Edges);

public record GraphPath(List<Guid> AncestorIds, List<Node> Nodes, List<Edge> Edges);

public record UserContextView(Node Header, List<Node> Sessions);

public interface IGraphService
{
    // Schema
    Task<GraphSchema> GetSchemaAsync(CancellationToken ct = default);

    // Tree (all nodes/edges reachable from a root) — full BFS, prefer GetChildrenAsync for lazy navigation
    Task<GraphTree> GetTreeAsync(Guid rootNodeId, string? effectiveUserId = null, CancellationToken ct = default);

    // Shallow children load — returns the parent node, its direct children, parent->child edges,
    // and child->grandchild edges (existence hints, no grandchild node payloads).
    Task<GraphTree> GetChildrenAsync(Guid parentNodeId, string? effectiveUserId = null, CancellationToken ct = default);

    // Ancestor path — returns the chain from root to the target node along with siblings at each level,
    // so the tree can render an expanded path to a deep-linked selection.
    Task<GraphPath> GetPathAsync(Guid nodeId, string? effectiveUserId = null, CancellationToken ct = default);

    // Node CRUD
    Task<Node?> GetNodeAsync(Guid nodeId, CancellationToken ct = default);
    Task<Node> CreateNodeAsync(Node node, CancellationToken ct = default);
    Task<Node> UpdateNodeAsync(Node node, CancellationToken ct = default);
    Task DeleteNodeAsync(Guid nodeId, CancellationToken ct = default);

    // Edge CRUD
    Task<Edge> CreateEdgeAsync(Edge edge, CancellationToken ct = default);
    Task<Edge> UpdateEdgeAsync(Edge edge, CancellationToken ct = default);
    Task DeleteEdgeAsync(Guid edgeId, CancellationToken ct = default);

    // Properties
    Task SetPropertyAsync(Guid nodeId, string key, string? value, CancellationToken ct = default);
    Task RemovePropertyAsync(Guid nodeId, string key, CancellationToken ct = default);

    // User context
    Task<Node?> GetUserContextAsync(string userId, CancellationToken ct = default);
    Task<Node> EnsureUserContextAsync(string userId, string userName, CancellationToken ct = default);

    // Bundled user-context view: walks (uc) -[has.sessions]-> (header) -[has.sessions]-> (session)
    // in a single round-trip and returns the header + session root nodes WITH their graph properties.
    // Returns null if the user-context node does not exist.
    Task<UserContextView?> GetUserContextViewAsync(Guid userContextNodeId, CancellationToken ct = default);
}
