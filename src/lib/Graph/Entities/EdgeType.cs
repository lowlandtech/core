namespace LowlandTech.Core.Graph.Entities;

public class EdgeType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;        // Verb: "contains", "activates", "produces", "runs", "validates"
    public string Label { get; set; } = string.Empty;
    public string? ClrType { get; set; }                    // Interface type that resolves the GraphService for this intent
    public Guid SourceNodeTypeId { get; set; }              // Noun (subject)
    public Guid TargetNodeTypeId { get; set; }              // Noun (object)
    public bool IsStatic { get; set; }                      // Descriptor-backed schema definitions are immutable in settings

    // Navigation
    public NodeType SourceNodeType { get; set; } = null!;
    public NodeType TargetNodeType { get; set; } = null!;
    public ICollection<Edge> Edges { get; set; } = [];
    public ICollection<Property> Properties { get; set; } = [];
}
