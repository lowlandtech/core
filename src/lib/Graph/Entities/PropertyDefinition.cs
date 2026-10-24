namespace LowlandTech.Core.Graph.Entities;

public class PropertyDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NodeTypeId { get; set; }
    public string Key { get; set; } = string.Empty;         // Property key this definition governs
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "string";            // string, int, bool, decimal, date, enum
    public string? DefaultValue { get; set; }
    public string? ValueList { get; set; }                   // JSON array for enum values e.g. ["Active","Inactive"]
    public string? View { get; set; }                        // Default view/renderer hint
    public string? Icon { get; set; }
    public string? Header { get; set; }                      // Column/group header for UI layout
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsVisible { get; set; } = true;

    // Navigation
    public NodeType NodeType { get; set; } = null!;
    public ICollection<Rule> Rules { get; set; } = [];
}
