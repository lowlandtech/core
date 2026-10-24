namespace LowlandTech.Core.Graph.Entities;

public class NodeType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? BlockComponent { get; set; }
    public int SortOrder { get; set; }
    public bool IsLeaf { get; set; }
    public bool IsHeader { get; set; }
    public bool IsStatic { get; set; }

    // Navigation
    public ICollection<PropertyDefinition> PropertyDefinitions { get; set; } = [];
    public ICollection<Node> Nodes { get; set; } = [];
}
