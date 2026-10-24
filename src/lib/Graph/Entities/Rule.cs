namespace LowlandTech.Core.Graph.Entities;

public class Rule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? RuleGroupId { get; set; }
    public Guid? PropertyId { get; set; }
    public Guid? PropertyDefinitionId { get; set; }

    public string Left { get; set; } = string.Empty;        // Left operand: property key, expression, or literal
    public string Operator { get; set; } = string.Empty;    // ==, !=, >, <, >=, <=, contains, startsWith, endsWith, matches, in, notIn, isNull, isNotNull
    public string? Right { get; set; }                       // Right operand: value, expression, or null for unary operators
    public string? Message { get; set; }                     // Validation/display message when rule evaluates to false
    public int SortOrder { get; set; }

    // Navigation
    public RuleGroup? RuleGroup { get; set; }
    public Property? Property { get; set; }
    public PropertyDefinition? PropertyDefinition { get; set; }
}
