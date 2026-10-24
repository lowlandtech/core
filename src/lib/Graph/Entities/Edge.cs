namespace LowlandTech.Core.Graph.Entities;

public class Edge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EdgeTypeId { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public EdgeType EdgeType { get; set; } = null!;
    public Node SourceNode { get; set; } = null!;
    public Node TargetNode { get; set; } = null!;
    public ICollection<Property> Properties { get; set; } = [];
}
