namespace LowlandTech.Domain.Entities;

public class Import
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid PackageId { get; set; }

    public Package Package { get; set; } = null!;
}
