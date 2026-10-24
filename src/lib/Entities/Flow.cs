namespace LowlandTech.Domain.Entities;

public class Flow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Step> Steps { get; set; } = [];
}
