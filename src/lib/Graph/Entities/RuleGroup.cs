namespace LowlandTech.Core.Graph.Entities;

public class RuleGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentId { get; set; }
    public Guid? PropertyId { get; set; }
    public string Operator { get; set; } = "and";           // "and", "or", "not"
    public string? Label { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public RuleGroup? Parent { get; set; }
    public Property? Property { get; set; }
    public ICollection<RuleGroup> Children { get; set; } = [];
    public ICollection<Rule> Rules { get; set; } = [];
}
