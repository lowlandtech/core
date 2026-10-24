namespace LowlandTech.Domain.Entities;

public class Setting
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public ICollection<SettingItem> Items { get; set; } = [];
}