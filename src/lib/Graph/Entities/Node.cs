namespace LowlandTech.Core.Graph.Entities;

public class Node
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NodeTypeId { get; set; }
    public string? Label { get; set; }                      // Cached display label (denormalized from entity)
    public int SortOrder { get; set; }

    // Navigation
    public NodeType NodeType { get; set; } = null!;
    public ICollection<Property> Properties { get; set; } = [];
    public ICollection<Edge> OutgoingEdges { get; set; } = [];
    public ICollection<Edge> IncomingEdges { get; set; } = [];
}
