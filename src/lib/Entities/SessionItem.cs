namespace LowlandTech.Domain.Entities;

public class SessionItem
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public int EditCount { get; set; }

    public Session Session { get; set; } = null!;
    public SessionItem? Parent { get; set; }
    public ICollection<SessionItem> Children { get; set; } = [];
    public ICollection<SessionBlock> Blocks { get; set; } = [];
}
