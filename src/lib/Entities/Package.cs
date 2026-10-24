namespace LowlandTech.Domain.Entities;

public class Package
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string PackageVersion { get; set; } = string.Empty;

    public ICollection<Template> Templates { get; set; } = [];
    public ICollection<Import> Imports { get; set; } = [];
}
