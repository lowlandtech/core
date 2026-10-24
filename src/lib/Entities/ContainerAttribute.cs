namespace LowlandTech.Domain.Entities;

public class ContainerAttribute
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public string Source { get; set; } = "structural";  // "structural" | "theme" | "user"
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public Guid ContainerItemId { get; set; }

    public ContainerItem ContainerItem { get; set; } = null!;
}
