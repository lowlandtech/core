namespace LowlandTech.Domain.Entities;

public class DataTable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid WorkspaceId { get; set; }
    public string? Schema { get; set; }
    public string PluralName { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public ICollection<TableColumn> Columns { get; set; } = [];
    public ICollection<Relationship> SourceRelationships { get; set; } = [];
    public ICollection<Relationship> TargetRelationships { get; set; } = [];
}
