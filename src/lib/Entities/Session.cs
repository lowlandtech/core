namespace LowlandTech.Domain.Entities;

public class Session
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Type { get; set; } = string.Empty;
    public Guid? TypeId { get; set; }
    public Guid? ParentId { get; set; }
    public int Order { get; set; }

    public Session? Parent { get; set; }
    public ICollection<Session> Children { get; set; } = [];
    public ICollection<SessionItem> Messages { get; set; } = [];
}
