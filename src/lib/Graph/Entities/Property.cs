namespace LowlandTech.Core.Graph.Entities;

public class Property
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? View { get; set; }                       // Nullable view hint for rendering

    // Discriminator — exactly one of these is set
    public Guid? NodeId { get; set; }
    public Guid? EdgeId { get; set; }
    public Guid? NodeTypeId { get; set; }
    public Guid? EdgeTypeId { get; set; }

    // Navigation
    public Node? Node { get; set; }
    public Edge? Edge { get; set; }
    public NodeType? NodeType { get; set; }
    public EdgeType? EdgeType { get; set; }
    public ICollection<Rule> Rules { get; set; } = [];
    public ICollection<RuleGroup> RuleGroups { get; set; } = [];
}
