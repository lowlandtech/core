namespace LowlandTech.Domain.Entities;

public class TableColumn
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid TableId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsRequired { get; set; }
    public bool IsKey { get; set; }
    public string? DefaultValue { get; set; }
    public string? AllowedValues { get; set; }
    public int SortOrder { get; set; }

    public DataTable Table { get; set; } = null!;
}
