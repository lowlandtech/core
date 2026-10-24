namespace LowlandTech.Domain.Entities;

public class ThemeToken
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "color";
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public Guid ThemeId { get; set; }
    public Theme Theme { get; set; } = null!;
}
