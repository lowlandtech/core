namespace LowlandTech.Domain.Entities;

public class SessionBlock
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string BlockType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SessionItem Message { get; set; } = null!;
}
