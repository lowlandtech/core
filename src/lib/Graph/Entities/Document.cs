namespace LowlandTech.Core.Graph.Entities;

/// <summary>
/// Materialized cache of a fully hydrated <see cref="Node"/> (node + properties + type).
/// Read-through: populated by <c>GraphSession.GetAsync&lt;Node&gt;</c> on first access,
/// invalidated/refreshed on writes that touch the underlying node.
/// </summary>
public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NodeId { get; set; }
    public Guid NodeTypeId { get; set; }
    public string Data { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Node Node { get; set; } = null!;
    public NodeType NodeType { get; set; } = null!;
}
