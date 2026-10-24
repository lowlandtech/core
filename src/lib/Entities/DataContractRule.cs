namespace LowlandTech.Domain.Entities;

public class DataContractRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool IsRequired { get; set; }
    public string? AllowedValues { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public Guid DataContractId { get; set; }
    public DataContract DataContract { get; set; } = null!;
}
