namespace LowlandTech.Domain.Entities;

public class Relationship
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid SourceTableId { get; set; }
    public Guid TargetTableId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string SourceNavigationProperty { get; set; } = string.Empty;
    public string TargetNavigationProperty { get; set; } = string.Empty;
    public string ForeignKeyColumn { get; set; } = string.Empty;
    public string OnDeleteBehavior { get; set; } = "Cascade";

    public DataTable SourceTable { get; set; } = null!;
    public DataTable TargetTable { get; set; } = null!;
}
