namespace LowlandTech.Domain.Entities;

public class SettingItem
{
    public Guid Id { get; set; }
    public Guid SettingId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Setting Setting { get; set; } = null!;
}