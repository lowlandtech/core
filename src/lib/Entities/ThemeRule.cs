namespace LowlandTech.Domain.Entities;

public class ThemeRule
{
    public Guid Id { get; set; }
    public string Selector { get; set; } = string.Empty;
    public string Overrides { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public Guid ThemeId { get; set; }
    public Theme Theme { get; set; } = null!;
}
