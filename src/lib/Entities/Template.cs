namespace LowlandTech.Domain.Entities;

public class Template
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<TemplateVersion> Versions { get; set; } = [];
    public ICollection<Package> Packages { get; set; } = [];
}
